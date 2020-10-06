using System;
using System.Collections.Generic;
using System.Text;

namespace EtlBolsaFamilia.Entidades.WebService
{
    public class Ibge
    {
        public int id { get; set; }
        public string nome { get; set; }
        public Municipio municipio { get; set; }
    }
    public class Regiao
    {
        public int id { get; set; }
        public string sigla { get; set; }
        public string nome { get; set; }
    }

    public class UF
    {
        public int id { get; set; }
        public string sigla { get; set; }
        public string nome { get; set; }
        public Regiao regiao { get; set; }
    }

    public class Mesorregiao
    {
        public int id { get; set; }
        public string nome { get; set; }
        public UF UF { get; set; }
    }

    public class Microrregiao
    {
        public int id { get; set; }
        public string nome { get; set; }
        public Mesorregiao mesorregiao { get; set; }
    }

    public class Municipio
    {
        public int id { get; set; }
        public string nome { get; set; }
        public Microrregiao microrregiao { get; set; }
    }
}
