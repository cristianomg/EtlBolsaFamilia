using EtlBolsaFamilia.Entidades.WebService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace EtlBolsaFamilia.Util
{
    public class MunicipiosWebService
    {
        private readonly Uri baseUrl;
        public MunicipiosWebService()
        {
            this.baseUrl = new Uri("https://servicodados.ibge.gov.br/api/v1/");
        }

        public List<int> ObterMunicipios()
        {
            using (var conexao = new HttpClient {BaseAddress = baseUrl })
            {
                var response = conexao.GetAsync("localidades/estados/se/distritos").Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<List<Ibge>>(response.Content.ReadAsStringAsync().Result);
                    return result.Select(x=>x.municipio.id).ToList();
                }
                return new List<int>();
            }
        }

    }
}
