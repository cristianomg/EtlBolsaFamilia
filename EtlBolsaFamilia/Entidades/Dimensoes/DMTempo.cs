using System;
using System.Collections.Generic;
using System.Text;

namespace EtlBolsaFamilia.Entidades.Dimensoes
{
    class DMTempo
    {
        public int Id { get; set; }
        public int Ano { get; set; }
        public int NumMes { get; set; }
        public string NmMes { get; set; }
        public string Semestre { get; set; }
        public string Bimestre { get; set; }
        public string Trimestre { get; set; }
    }
}
