using System;
using System.Collections.Generic;
using System.Text;

namespace EtlBolsaFamilia.Entidades
{
    public class ExtracaoBolsaFamilia
    {
        public DateTime DataReferencia { get; set; }
        public string nomeIBGE { get; set; }
        public string codigoIBGE { get; set; }
        public string SiglaUf { get; set; }
        public string NomeUf { get; set; }
        public decimal Valor { get; set; }
        public long QtdBeneficiados { get; set; }
    }
}
