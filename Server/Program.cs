using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://localhost:8094");
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(route =>
                        {
                            route.MapPost("/", async context =>
                            {
                                var part1 = new MemoryStream();
                                var sw = new StreamWriter(part1, leaveOpen: true);
                                await sw.WriteAsync(@"{ ""Name"": ""Arthur"" }");
                                await sw.DisposeAsync();
                                part1.Seek(0, SeekOrigin.Begin);

                                var part2 = new MemoryStream();
                                sw = new StreamWriter(part2, leaveOpen: true);
                                await sw.WriteAsync(@"{ ""Name"": ""Candace"" }");
                                await sw.DisposeAsync();
                                part2.Seek(0, SeekOrigin.Begin);

                                var part3 = new MemoryStream();
                                sw = new StreamWriter(part3, leaveOpen: true);
                                await sw.WriteAsync(@"{ ""Name"": ""Timothy"" }");
                                await sw.DisposeAsync();
                                part3.Seek(0, SeekOrigin.Begin);

                                var result = new MultipartResult();
                                result.Add(new MultipartContent
                                {
                                    ContentType = "application/xml",
                                    FileName = "part1.xml",
                                    Stream = part1
                                });
                                result.Add(new MultipartContent
                                {
                                    ContentType = "application/xml",
                                    FileName = "part2.xml",
                                    Stream = part2
                                });
                                result.Add(new MultipartContent
                                {
                                    ContentType = "application/xml",
                                    FileName = "part3.xml",
                                    Stream = part3
                                });
                                await result.ExecuteResultAsync(context);
                            });
                        });
                    });
                })
                .Build().Run();
        }
    }
}
