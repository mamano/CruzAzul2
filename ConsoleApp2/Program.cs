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

        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            /*Console.WriteLine("teste");
            var regex = new Regex(@"[^\d]");
            var accessnumber = regex.Replace("348351-343", "");
            Console.Write(accessnumber);

            regex = new Regex(@"[!@#$%^&*.:,;<>\/|=+-´{}]");
            accessnumber = regex.Replace("sdasd asdads . : ; <> asdasdasd", "_");
            Console.Write(accessnumber);
            Main2(args).GetAwaiter().GetResult();*/
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {

            Console.WriteLine("teste2");
            string host = "http://7.43.240.126:8080/";

            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders.Accept.Clear();
            client.Timeout = TimeSpan.FromMinutes(5);
            var token = "username=tor&password=tor@1234&grant_type=password";
            Console.WriteLine("teste3");
            try
            {
                Console.WriteLine(DateTime.Now);
                var  result = GetTokenAsync(client, token).GetAwaiter().GetResult();

                Console.WriteLine("teste4");


                Console.WriteLine(DateTime.Now);
                Console.WriteLine(JsonConvert.SerializeObject(result));
                    Console.WriteLine(result.access_token);
                    Console.WriteLine(result.token_type);
                    Console.WriteLine(result.expires_in);
                //var teste = data.ToObject<string[]>();
                //msg = teste[0];
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.access_token);
                var report = @"TOMOGRAFIA COMPUTADORIZADA DO CRÂNIO

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
Redução volumétrica encefálica difusa";
                //var append = new StringBuilder();
                //append.AppendFormat("{{\\rtf1\\fbidis\\ansi\\ansicpg1252\\deff0\\deflang1046{{\\fonttbl{{\\f0\\froman\\fprq2\\fcharset0 LUCIDA CONSOLE;}}{{\\f1\\fnil\\fcharset0 LUCIDA CONSOLE;}}{{\\f2\\fnil\\fcharset178 Courier New;}}}  {{\\stylesheet{{ Normal;}}{{\\s1 heading 1;}}  \\viewkind4\\uc1\\pard\\ltrpar\\keepn\\s1\\b\\f0\\fs23 {0} \\par \\par \\b \\par }}", report.Replace("\r\n", ""));
                
                var laudo = new Laudos
                {
                    Id = 45,
                    Laudo = PlainTextToRtf(report),
                    Crm = "43485"
                };


                // Create a new product
                CadastraLaudo cadastroLaudo = new CadastraLaudo
                    {
                        Laudos = new object[] { laudo },
                        NomePaciente = "ANDREA MARIA DE AZEVEDO",
                        Os = 21789465
                };

                    var teste = JsonConvert.SerializeObject(cadastroLaudo);
                     var response = await SendLaudoAsync(client, cadastroLaudo);
                    
                    Console.WriteLine(response.Codigo);
                    Console.WriteLine(response.Dados);
                    Console.WriteLine(response.Descricao);
                    Console.WriteLine(response.Mensagens);
                    Console.WriteLine(response.StatusRetorno);

                Console.WriteLine(DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.Message);
                Console.WriteLine(DateTime.Now);
            }
            
        }

        public static string PlainTextToRtf(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";

            string escapedPlainText = plainText.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");
            escapedPlainText = EncodeCharacters(escapedPlainText);

            string rtf = @"{\rtf1\ansi\ansicpg1250\deff0{\fonttbl\f0\fswiss Helvetica;}\f0\pard ";
            rtf += escapedPlainText.Replace(Environment.NewLine, "\\par\r\n ");
            rtf += " }";
            return rtf;
        }

        private static string EncodeCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text
                .Replace("ą", @"\'b9")
                .Replace("ć", @"\'e6")
                .Replace("ę", @"\'ea")
                .Replace("ł", @"\'b3")
                .Replace("ń", @"\'f1")
                .Replace("ó", @"\'f3")
                .Replace("ś", @"\'9c")
                .Replace("ź", @"\'9f")
                .Replace("ż", @"\'bf")
                .Replace("Ą", @"\'a5")
                .Replace("Ć", @"\'c6")
                .Replace("Ę", @"\'ca")
                .Replace("Ł", @"\'a3")
                .Replace("Ń", @"\'d1")
                .Replace("Ó", @"\'d3")
                .Replace("Ś", @"\'8c")
                .Replace("Ź", @"\'8f")
                .Replace("Ż", @"\'af");
        }

        static async Task<ResultCadastroLaudo> SendLaudoAsync(HttpClient client, CadastraLaudo cadastraLaudo)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "webapi/api/integracoes/laudo/cadastrarLaudo");
            request.Content = new StringContent(JsonConvert.SerializeObject(cadastraLaudo), Encoding.UTF8, "application/json");
            //client.Timeout = new TimeSpan(0, 0, 40);
            var response = await client.SendAsync(request);
            

            var result = new ResultCadastroLaudo();
            

            if (response.IsSuccessStatusCode)
            {
                result.Descricao = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";
                var data = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<ResultCadastroLaudo>(data);
            }
            // return URI of the created resource.
            return result;
        }

        static async Task<Token> GetTokenAsync(HttpClient client, string token)
        {
            //StringContent content = new StringContent(JsonConvert.SerializeObject(token), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "webapi/token");
            request.Content = new StringContent(token, Encoding.UTF8, "application/x-www-form-urlencoded");
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            //client.Timeout = new TimeSpan(0, 0, 40);
            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            var result = new Token();
            

            if (response.IsSuccessStatusCode)
            {
                result.token_type = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";
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
