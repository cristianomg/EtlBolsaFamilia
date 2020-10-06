using System;
using System.Collections.Generic;
using System.Text;

namespace EtlBolsaFamilia.Entidades
{
    public class TransformacaoFato
    {
        public string CdCidade { get; set; }
        public DateTime DataRegistro { get; set; }
        public long QtdBeneficiadosTotal { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
