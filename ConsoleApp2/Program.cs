using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("teste");
            var regex = new Regex(@"[^\d]");
            var accessnumber = regex.Replace("348351-343", "");
            Console.Write(accessnumber);

            regex = new Regex(@"[!@#$%^&*.:,;<>\/|=+-´{}]");
            accessnumber = regex.Replace("sdasd asdads . : ; <> asdasdasd", "_");
            Console.Write(accessnumber);
            Main2(args).GetAwaiter().GetResult();
        }

        static async Task Main2(string[] args)
        {

            Console.WriteLine("teste2");
            HttpClient client = new HttpClient();
            string host = "http://172.17.100.30:8080/";
            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders.Accept.Clear();
            var token = "username=tor&password=tor@1234&grant_type=password";
            Console.WriteLine("teste3");
            try
            {
                var  result = await GetTokenAsync(client, token);

                Console.WriteLine("teste4");
                //string msg;
               
                    Console.WriteLine("teste5");
                   
                  
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                    Console.WriteLine(result.access_token);
                    Console.WriteLine(result.token_type);
                    Console.WriteLine(result.expires_in);
                //var teste = data.ToObject<string[]>();
                //msg = teste[0];
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.access_token);
                var report = @" 
TOMOGRAFIA COMPUTADORIZADA DO CRÂNIO

COMENTÁRIOS: Exame realizado em aparelho de tomografia computadorizada multidetector, com aquisição de imagens através de sequências volumétricas sem a administração endovenosa do contraste não iônico.

Os seguintes aspectos foram observados:

                Ausência de coleções intra ou extra-axiais.

Hipoatenuação difusa da substância branca peri-ventricular e dos centros semi-ovais.

Proeminência de sulcos e fissuras encefálicas.

Ectasia compensatória do sistema ventricular supratentorial

IV ventrículo normoconfigurado.

Calcificações ateromatosas das artérias carótidas internas intra - cavernosas.

Fossa posterior de aspecto usual.

Linha média centrada.

Sem sinais de fraturas detectáveis

OPINIÃO:
Achados sugestivos de leucoaraiose/ microangiopatia
Redução volumétrica encefálica difusa ";
                 report = string.Format(@"{{\rtf1\fbidis\ansi\ansicpg1252\deff0\deflang1046{{\fonttbl{{\f0\froman\fprq2\fcharset0 LUCIDA CONSOLE;}}{{\f1\fnil\fcharset0 LUCIDA CONSOLE;}}{{\f2\fnil\fcharset178 Courier New;}}}  {{\stylesheet{{ Normal;}}{{\s1 heading 1;}}  \viewkind4\uc1\pard\ltrpar\keepn\s1\b\f0\fs23 
			{0}

			\par \par \b \par }}", report.Replace("\r\n", @" \par "));
                var laudo = new Laudos
                {
                    Id = 45,
                    Laudo = report,
                    Crm = "43485"
                };


                // Create a new product
                CadastraLaudo cadastroLaudo = new CadastraLaudo
                    {
                        Laudos = new object[] { laudo },
                        NomePaciente = "Paciente Benner",
                        Os = 21789465
                };

                    var teste = JsonConvert.SerializeObject(cadastroLaudo);
                     var response = await SendLaudoAsync(client, cadastroLaudo);
                    
                    Console.WriteLine(response.Codigo);
                    Console.WriteLine(response.Dados);
                    Console.WriteLine(response.Descricao);
                    Console.WriteLine(response.Mensagens);
                    Console.WriteLine(response.StatusRetorno);

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.Message);
            }
            
        }

        static async Task<ResultCadastroLaudo> SendLaudoAsync(HttpClient client, CadastraLaudo cadastraLaudo)
        {
            var content = new StringContent(JsonConvert.SerializeObject(cadastraLaudo), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("webapi/api/integracoes/laudo/cadastrarLaudo", content);
            var result = new ResultCadastroLaudo();
            result.Descricao = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<ResultCadastroLaudo>(data);
            }
            // return URI of the created resource.
            return result;
        }

        static async Task<Token> GetTokenAsync(HttpClient client, string token)
        {
            //StringContent content = new StringContent(JsonConvert.SerializeObject(token), Encoding.UTF8, "application/json");
            var content = new StringContent(token, Encoding.UTF8, "text/plain");
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var response = await client.PostAsync("webapi/token", content);
            var result = new Token();
            result.token_type = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";

            if (response.IsSuccessStatusCode)
            {
                //var data = (Newtonsoft.Json.Linq.JArray)await response.Content.ReadAsAsync<object>();
                //var teste = data.ToObject<string[]>();
                //msg = teste[0];
                var data = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<Token>(data);

            }
            return result;
        }


    }
    [Serializable]
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    [Serializable]
    public class CadastraLaudo
    {
        public long Os { get; set; }
        public string NomePaciente { get; set; }
        public object[] Laudos { get; set; }
    }
    [Serializable]
    public class Laudos
    {
        public int Id { get; set; }
        public string Laudo { get; set; }
        public string Crm { get; set; }
    }
    [Serializable]
    public class ResultCadastroLaudo
    {
        public string Dados { get; set; }
        public int Codigo { get; set; }
        public string Descricao { get; set; }
        public int StatusRetorno { get; set; }
        public string[] Mensagens { get; set; }
    }

}
