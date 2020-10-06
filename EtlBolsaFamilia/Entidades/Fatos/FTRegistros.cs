using System;
using System.Collections.Generic;
using System.Text;

namespace EtlBolsaFamilia.Entidades.Fatos
{
    public class FTRegistros
    {
        public int IdCidade { get; set; }
        public int IdTempo { get; set; }
        public decimal VlGasto { get; set; }
        public long QtdBeneficiados { get; set; }

    }
}
