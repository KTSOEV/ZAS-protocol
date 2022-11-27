using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using xNetStandard;

namespace ZipAndSend
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fileName;
            string url;
            if (args.Length > 0)
                url = args[0];
            else
                url = "http://127.0.0.1:5000/";
            if(args.Length > 1)
                fileName = args[1];
            else
                fileName = "doks.txt";

            using (ZipArchive zip = ZipFile.Open(fileName+".zip", ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(fileName, fileName);
            }
            Stopwatch timer = new Stopwatch();
            using (var request = new HttpRequest())
            {
                var multipartContent = new MultipartContent() {
                    {new FileContent(fileName+".zip"), "file", fileName+".zip"}
                };
                timer.Start();
                request.Post(url, multipartContent).None();
            }
            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;
            Console.WriteLine(timeTaken.Milliseconds);
            File.Delete(fileName + ".zip");
        }
    }
}
