using EtlBolsaFamilia.Entidades;
using EtlBolsaFamilia.Entidades.WebService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EtlBolsaFamilia.Exatracao
{
    public class BolsaFamiliaWebService
    {
        public BolsaFamiliaWebService()
        {

        }
        public async Task<List<ExtracaoBolsaFamilia>> ObterDadosBolsaFamilia(string mesAno, string codigoIbge)
        {
            try
            {
                using (var conexao = new HttpClient())
                {
                    conexao.DefaultRequestHeaders.Add("chave-api-dados", "deae42f72f22d5425a0e191a144d62be");

                    var UriBuilder = new UriBuilder("http://www.transparencia.gov.br/api-de-dados/bolsa-familia-por-municipio");

                    UriBuilder.Query = $"mesAno={mesAno}&codigoIbge={codigoIbge}";

                    var response = await conexao.GetAsync(UriBuilder.Uri);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody =  await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<List<BolsaFamilia>>(responseBody);
                        return result.Select(x => new ExtracaoBolsaFamilia
                        {
                            nomeIBGE = x.municipio.nomeIBGE,
                            codigoIBGE = x.municipio.codigoIBGE,
                            NomeUf = x.municipio.uf.nome,
                            SiglaUf = x.municipio.uf.sigla, 
                            Valor = x.valor,
                            QtdBeneficiados = x.quantidadeBeneficiados,
                            DataReferencia = Convert.ToDateTime(x.dataReferencia),
                        }).ToList();
                    }
                    return new List<ExtracaoBolsaFamilia>();
                }
            }
            catch
            {
                return new List<ExtracaoBolsaFamilia>();
            }
            
        }
    }
}
