using System;
using System.Collections.Generic;
using System.Text;

namespace EtlBolsaFamilia.Entidades.WebService
{
    public class BolsaFamilia
    {
        public int id { get; set; }
        public string dataReferencia { get; set; }
        public MunicipioBolsaFamilia municipio { get; set; }
        public Tipo tipo { get; set; }
        public decimal valor { get; set; }
        public int quantidadeBeneficiados { get; set; }
    }
    public class Uf
    {
        public string sigla { get; set; }
        public string nome { get; set; }
    }

    public class MunicipioBolsaFamilia
    {
        public string codigoIBGE { get; set; }
        public string nomeIBGE { get; set; }
        public string nomeIBGEsemAcento { get; set; }
        public string pais { get; set; }
        public Uf uf { get; set; }
    }

    public class Tipo
    {
        public int id { get; set; }
        public string descricao { get; set; }
        public string descricaoDetalhada { get; set; }
    }


}
