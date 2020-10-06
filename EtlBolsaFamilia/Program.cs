using EtlBolsaFamilia.Entidades;
using EtlBolsaFamilia.Entidades.Dimensoes;
using EtlBolsaFamilia.Exatracao;
using EtlBolsaFamilia.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EtlBolsaFamilia
{
    class Program
    {
        static void Main(string[] args)
        {

            var conexaoDW = "Server=DESKTOP-J3G20FU\\SQLEXPRESS;Database=BolsaFamiliaDW;Trusted_Connection=True;"; 

            Console.WriteLine(template("Carregando municipios"));


            var municipios = new MunicipiosWebService().ObterMunicipios();

            Console.WriteLine(template($"Total de municipios coletados : {municipios.Count}"));


            var listaDadosBolsaFamilia = new ConcurrentBag<ExtracaoBolsaFamilia>();

            var webServiceBolsaFamilia = new BolsaFamiliaWebService();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var data = "201603";

            float porcentagem = 0;
            Console.WriteLine(template("Iniciando extração"));


            Parallel.ForEach(municipios, x =>
            {
                var dados = webServiceBolsaFamilia.ObterDadosBolsaFamilia(data, x.ToString()).Result;
                dados.ForEach(x => listaDadosBolsaFamilia.Add(x));
                Console.WriteLine($"codigo da cidade sendo extraida: {x}");
            });

            stopwatch.Stop();

            Console.WriteLine($"tempo de extração : {stopwatch.Elapsed.TotalSeconds}");

            Console.WriteLine(template("DadosBolsaFamilia extraidos"));


            Console.WriteLine(template("iniciando transformação"));

            var culture = new CultureInfo("pt-Br");

            var listaDMTempo = listaDadosBolsaFamilia.GroupBy(x=>x.DataReferencia)
                .Select(x=>new DMTempo
                {
                    Ano = x.Key.Year,
                    NumMes = x.Key.Month,
                    NmMes = culture.DateTimeFormat.GetMonthName(x.Key.Month),
                    Bimestre = DeParaBimestre(x.Key.Month),
                    Semestre = DeParaSemestre(x.Key.Month),
                    Trimestre = DeParaTrimestre(x.Key.Month),
                });


            using (SqlConnection connection = new SqlConnection(conexaoDW))
            {
                connection.Open();
                foreach (var dmTempo in listaDMTempo)
                {
                    try
                    {


                        var idTempo = ObterIdTempo(dmTempo.Ano, dmTempo.NumMes, connection);

                        if (idTempo > 0) continue;

                        var queryString = "INSERT INTO DM_TEMPO(ANO,NUM_MES,NM_MES,BIMESTRE,SEMESTRE,TRIMESTRE) VALUES (@ANO,@NUM_MES,@NM_MES,@BIMESTRE,@SEMESTRE,@TRIMESTRE)";
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@ANO", dmTempo.Ano);
                        command.Parameters.AddWithValue("@NUM_MES", dmTempo.NumMes);
                        command.Parameters.AddWithValue("@NM_MES", dmTempo.NmMes);
                        command.Parameters.AddWithValue("@BIMESTRE", dmTempo.Bimestre);
                        command.Parameters.AddWithValue("@SEMESTRE", dmTempo.Semestre);
                        command.Parameters.AddWithValue("@TRIMESTRE", dmTempo.Trimestre);
                        command.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Erro ao inserir dm tempo: {ex.Message}");
                        return;
                    }

                }
                connection.Close();

            }
            var listaDMCidade = listaDadosBolsaFamilia.GroupBy(x => new { x.nomeIBGE, x.codigoIBGE, x.NomeUf, x.SiglaUf})
                .Select(x=> new DMCidade
                {
                    CdCidade = x.Key.codigoIBGE,
                    NmCidade = x.Key.nomeIBGE,
                    NmUf = x.Key.NomeUf,
                    SglUf = x.Key.SiglaUf,
                });

            using (SqlConnection connection = new SqlConnection(conexaoDW))
            {
                connection.Open();
                foreach (var dmCidade in listaDMCidade)
                {
                    try
                    {

                        var idCidade = ObterIdCidade(dmCidade.CdCidade, connection);

                        if (idCidade > 0) continue;

                        var queryString = "INSERT INTO DM_CIDADE(CD_CIDADE,NOM_CIDADE,NOM_UF,SGL_UF) VALUES (@CD_CIDADE,@NOM_CIDADE,@NOM_UF,@SGL_UF)";
                        SqlCommand command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@CD_CIDADE", dmCidade.CdCidade);
                        command.Parameters.AddWithValue("@NOM_CIDADE", dmCidade.NmCidade);
                        command.Parameters.AddWithValue("@NOM_UF", dmCidade.NmUf);
                        command.Parameters.AddWithValue("@SGL_UF", dmCidade.SglUf);
                        command.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Erro ao inserir dm Cidade: {ex.Message}");
                        return;
                    }

                }
                connection.Close();

            }

            var listaFTRegistro = listaDadosBolsaFamilia.GroupBy(x => new { x.codigoIBGE, x.DataReferencia })
                .Select(x => new TransformacaoFato 
                {
                    CdCidade = x.Key.codigoIBGE,
                    QtdBeneficiadosTotal = x.Sum(y => y.QtdBeneficiados),
                    ValorTotal = x.Sum(y => y.Valor),
                    DataRegistro = x.Key.DataReferencia
                });

            using (SqlConnection connection = new SqlConnection(conexaoDW))
            {
                connection.Open();
                foreach (var ft in listaFTRegistro)
                {
                    try
                    {
                        var idCidade = ObterIdCidade(ft.CdCidade, connection);
                        var idTempo = ObterIdTempo(ft.DataRegistro.Year, ft.DataRegistro.Month, connection);

                        var idFato = ObterIdFatoRegistros(idCidade, idTempo, ft.ValorTotal, ft.QtdBeneficiadosTotal, connection);

                        if (idFato > 0) continue;

                        var queryString = "INSERT INTO FT_REGISTROS(ID_CIDADE,ID_TEMPO,VLR_GASTO,QTD_BENEFICIADOS) VALUES (@ID_CIDADE,@ID_TEMPO,@VLR_GASTO,@QTD_BENEFICIADOS)";
                        var command = new SqlCommand(queryString, connection);
                        command.Parameters.AddWithValue("@ID_CIDADE", idCidade);
                        command.Parameters.AddWithValue("@ID_TEMPO", idTempo);
                        command.Parameters.AddWithValue("@VLR_GASTO", ft.ValorTotal);
                        command.Parameters.AddWithValue("@QTD_BENEFICIADOS", ft.QtdBeneficiadosTotal);
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Erro ao inserir dm Cidade : {ex.Message}");
                        return;
                    }

                }
                connection.Close();
            }


        }
        static string template(string texto)
        {
            return $"----------------------{texto}----------------------";
        }
        static string DeParaBimestre(int mes)
        {
            if (mes >= 1 && mes <= 2)
                return "Primeiro";
            else if (mes >= 3 && mes <= 4)
                return "Segundo";
            else if (mes >= 5 && mes <= 6)
                return "Terceiro";
            else if (mes >= 7 && mes <= 8)
                return "Quarto";
            else if (mes >= 9 && mes <= 10)
                return "Quinto";
            else if (mes >= 11 && mes <= 12)
                return "Sexto";
            else
                return "";
        }
        static string DeParaTrimestre(int mes)
        {
            if (mes >= 1 && mes <= 3)
                return "Primeiro";
            else if (mes >= 4 && mes <= 6)
                return "Segundo";
            else if (mes >= 7 && mes <= 9)
                return "Terceiro";
            else if (mes >= 10 && mes <= 12)
                return "Quarto";
            else
                return "";
        }
        static string DeParaSemestre(int mes)
        {
            if (mes >= 1 && mes <= 6)
                return "Primeiro";
            else if (mes >= 7 && mes <= 12)
                return "Segundo";
            else
                return "";
        }

        static int ObterIdCidade(string cdCidade, SqlConnection connection)
        {
            var queryString = "SELECT TOP(1) ID_CIDADE FROM DM_Cidade WHERE CD_CIDADE = @CD_CIDADE";
            SqlCommand command = new SqlCommand(queryString, connection);
            command.Parameters.AddWithValue("@CD_CIDADE", cdCidade);
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
            var dr = command.ExecuteReader();

            var idCidade = 0;
            while (dr.Read())
            {
                idCidade = dr.GetInt32(0);
            }

            dr.Close();

            return idCidade;
        }
        static int ObterIdTempo(int ano, int mes, SqlConnection connection)
        {
            var queryString = "SELECT TOP(1) ID_TEMPO FROM DM_TEMPO WHERE ANO = @ANO AND NUM_MES = @MES";
            var command = new SqlCommand(queryString, connection);
            command.Parameters.AddWithValue("@ANO", ano);
            command.Parameters.AddWithValue("@MES", mes);
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();

            var dr = command.ExecuteReader();

            var idTempo = 0;
            while (dr.Read())
            {
                idTempo = dr.GetInt32(0);
            }

            dr.Close();

            return idTempo;
        }
        static int ObterIdFatoRegistros(int idCidade, int idTempo, decimal vlrGastos, long qtdBeneficiados, SqlConnection connection)
        {
            var queryString = "SELECT TOP(1) ID_FATO FROM FT_REGISTROS WHERE ID_CIDADE = @ID_CIDADE AND ID_TEMPO = @ID_TEMPO AND VLR_GASTO = @VLR_GASTO AND QTD_BENEFICIADOS = @QTD_BENEFICIADOS";
            var command = new SqlCommand(queryString, connection);
            command.Parameters.AddWithValue("@ID_CIDADE", idCidade);
            command.Parameters.AddWithValue("@ID_TEMPO", idTempo);
            command.Parameters.AddWithValue("@VLR_GASTO", vlrGastos);
            command.Parameters.AddWithValue("@QTD_BENEFICIADOS", qtdBeneficiados);
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();

            var dr = command.ExecuteReader();

            var idFato = 0;
            while (dr.Read())
            {
                idFato = dr.GetInt32(0);
            }

            dr.Close();

            return idFato;
        }
        static void PrintarPorcentagem(float porcentagem)
        {
            if (porcentagem >= 25 && porcentagem <= 30)
            {
                Console.WriteLine($"Carregamento em {porcentagem}%");
            }
            else if (porcentagem >= 50 && porcentagem <= 55)
            {
                Console.WriteLine($"Carregamento em {porcentagem}%");

            }
            else if (porcentagem >= 75 && porcentagem <= 80)
            {
                Console.WriteLine($"Carregamento em {porcentagem}%");

            }
            else if (porcentagem == 100)
            {
                Console.WriteLine($"Carregamento em {porcentagem}%");

            }
        }
    }

}
