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
            //Main2(args).GetAwaiter().GetResult();
        }

        static async Task Main2(string[] args)
        {

            Console.WriteLine("teste2");
            HttpClient client = new HttpClient();
            string host = "http://172.17.100.30:8080/";
            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            StringContent content = new StringContent("username=tor&password=tor@1234&grant_type=password", Encoding.UTF8, "text/plain");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            Console.WriteLine("teste3");
            try
            {
                var  result = await GetTokenAsync(client, content);

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

                    Laudos laudo = new Laudos
                    {
                        Id = 266816938,
                        Laudo = "teste",
                        Crm = "2424",
                    };



                    // Create a new product
                    CadastraLaudo cadastroLaudo = new CadastraLaudo
                    {
                        Laudos = new object[] { laudo },
                        NomePaciente = "Paciente Benner",
                        Os = 18943559
                    };

                    var teste = JsonConvert.SerializeObject(cadastroLaudo);
                     var response = await SendLaudoAsync(client, cadastroLaudo);
                    var data = await response.Content.ReadAsStringAsync();//<object>();
                     var result2 = JsonConvert.DeserializeObject<ResultCadastroLaudo>(data);
                    Console.WriteLine(result2.Codigo);
                    Console.WriteLine(result2.Dados);
                    Console.WriteLine(result2.Descricao);
                    Console.WriteLine(result2.Mensagens);
                    Console.WriteLine(result2.StatusRetorno);

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.Message);
            }
            
        }

        static async Task<HttpResponseMessage> SendLaudoAsync(HttpClient client, CadastraLaudo cadastraLaudo)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(cadastraLaudo), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("webapi/api/integracoes/laudo/cadastrarLaudo", content);
            //response.EnsureSuccessStatusCode();
            
            return response;
        }

        static async Task<Token> GetTokenAsync(HttpClient client, StringContent content)
        {
            var  response = await client.PostAsync("webapi/token", content);
            var ret = default(Token);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                ret = JsonConvert.DeserializeObject<Token>(data);
            }
            return ret;
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
