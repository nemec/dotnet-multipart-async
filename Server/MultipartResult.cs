using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Server
{
    // https://github.com/aspnet/Mvc/issues/4933#issuecomment-236625904
    public class MultipartContent
    {
        public string ContentType { get; set; }

        public string FileName { get; set; }

        public Stream Stream { get; set; }
    }

    public class MultipartResult : Collection<MultipartContent>
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> Serializers
            = new ConcurrentDictionary<Type, XmlSerializer>();

        private readonly System.Net.Http.MultipartContent content;

        public MultipartResult(string subtype = "byteranges", string boundary = null)
        {
            if (boundary == null)
            {
                this.content = new System.Net.Http.MultipartContent(subtype);
            }
            else
            {
                this.content = new System.Net.Http.MultipartContent(subtype, boundary);
            }
        }

        public async Task ExecuteResultAsync(HttpContext context)
        {
            foreach (var item in this)
            {
                if (item.Stream != null)
                {
                    var content = new StreamContent(item.Stream);

                    if (item.ContentType != null)
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(item.ContentType);
                    }

                    if (item.FileName != null)
                    {
                        var contentDisposition = new ContentDispositionHeaderValue("attachment");
                        contentDisposition.FileName = item.FileName;
                        content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                        content.Headers.ContentDisposition.FileName = contentDisposition.FileName;
                        content.Headers.ContentDisposition.FileNameStar = contentDisposition.FileNameStar;
                    }

                    this.content.Add(content);
                }
            }

            // Don't set ContentLength, Transfer-Encoding: Chunked is set
            // automatically by kestrel and simulates an unknown number of body elements
            //context.Response.ContentLength = content.Headers.ContentLength;
            context.Response.ContentType = content.Headers.ContentType.ToString();

            var str = await content.ReadAsStringAsync();

            var sw = new StreamWriter(context.Response.Body);
            var multipartBoundary = this.content.Headers.ContentType.Parameters
                .First(p => p.Name == "boundary").Value.Trim('"');
            
            var parts = str.Split(multipartBoundary + "\r\n");
            for(var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                await sw.WriteAsync(part);
                if(i < parts.Length - 1)
                {
                    await sw.WriteAsync(multipartBoundary + "\r\n");
                }
                await sw.FlushAsync();
                if(i < parts.Length - 1)
                {
                    // Artificial delay to simulate asynchronous processing and response
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
            await sw.DisposeAsync();
        }
    }
}