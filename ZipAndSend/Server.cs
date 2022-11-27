using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ZipAndSend
{
    internal class Server
    {
        static void Main(string[] args)
        {
            StartServer();
            Console.ReadKey();
        }

        public static void StartServer()
        {
            var httpListener = new HttpListener();
            var simpleServer = new SimpleServer(httpListener, "http://127.0.0.1:8000/", ProcessYourResponse);
            simpleServer.Start();
        }

        public static byte[] ProcessYourResponse(string test)
        {
            Console.WriteLine(test);
            return new byte[0]; // TODO when you want return some response
        }
    }

    public delegate byte[] ProcessDataDelegate(string data);

    public class SimpleServer
    {
        private const int HandlerThread = 2;
        private readonly ProcessDataDelegate handler;
        private readonly HttpListener listener;

        public SimpleServer(HttpListener listener, string url, ProcessDataDelegate handler)
        {
            this.listener = listener;
            this.handler = handler;
            listener.Prefixes.Add(url);
        }

        public void Start()
        {
            if (listener.IsListening)
                return;

            listener.Start();

            for (int i = 0; i < HandlerThread; i++)
            {
                listener.GetContextAsync().ContinueWith(ProcessRequestHandler);
            }
        }

        public void Stop()
        {
            if (listener.IsListening)
                listener.Stop();
        }

        private void ProcessRequestHandler(Task<HttpListenerContext> result)
        {
            var context = result.Result;

            if (!listener.IsListening)
                return;

            // Start new listener which replace this
            listener.GetContextAsync().ContinueWith(ProcessRequestHandler);

            // Read request
            string request = new StreamReader(context.Request.InputStream).ReadToEnd();

            // Prepare response
            var responseBytes = handler.Invoke(request);
            context.Response.ContentLength64 = responseBytes.Length;

            var output = context.Response.OutputStream;
            output.WriteAsync(responseBytes, 0, responseBytes.Length);
            output.Close();
            if (context.Request.HttpMethod == "GET") { }
               // date = new RequestDate(context.Request.QueryString, contextRequest);
            else
            {
                var stream = context.Request.InputStream;
                string[] pairs;
                using (var reader = new StreamReader(stream))
                    pairs = reader.ReadToEnd().Split('&');
                Console.WriteLine("pairs: "+pairs[0]);
                for (int i = 0; i < pairs.Length; i++)
                {
                    Console.WriteLine("pair: " + pairs[i]);
                }
                //Теперь pairs содержит строки типа "Name=Ilya" "Password=petux"
            }
            Console.WriteLine(context.Request.HttpMethod + " " + context.Request.Url + " " + context.Response.StatusCode + " ");
        }
    }
}
