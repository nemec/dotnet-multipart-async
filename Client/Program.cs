using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var path = "http://localhost:8094";
            var httpClient = new HttpClient();
            using (var response = await httpClient.PostAsync(path,
                                    new StringContent("", Encoding.UTF8, "text/plain")))
            {
                if (response.IsSuccessStatusCode)
                {
                    // https://medium.com/@deep_blue_day/how-to-read-multipart-mime-data-from-httpresponsemessage-in-net-standard-9664904a7ca9
                    MultipartMemoryStreamProvider multipart = await response.Content.ReadAsMultipartAsync(); //Extension method available in Microsoft.AspNet.WebApi.Client
                    foreach (var content in multipart.Contents)
                    {
                        var json = JObject.Parse(await content.ReadAsStringAsync());
                        Console.WriteLine(json["Name"].Value<string>());
                    }
                }
            }
        }
    }
}
