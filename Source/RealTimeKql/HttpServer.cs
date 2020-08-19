// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/
namespace RealTimeKql
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    // This is a simple web server, implementing the protocol described here:
    // https://www.html5rocks.com/en/tutorials/eventsource/basics/
    //
    // The javascrpt code in the browser can now use the EventSource feature of HTML5:
    // https://www.w3schools.com/html/html5_serversentevents.asp

    public class HttpServer
    {
        static readonly HttpListener _listener = new HttpListener();
        static readonly List<StreamWriter> _eventSessions = new List<StreamWriter>();

        public HttpServer(string listenUri)
        {
            _listener.Prefixes.Add(listenUri);
            _listener.Start();

            Console.Write("Listening on ");
            foreach (var p in _listener.Prefixes) Console.WriteLine(p);

            // Start Async listening
            _listener.BeginGetContext(Context, _listener);
        }

        private static void Context(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;

            try
            {
                HttpListenerContext context = listener.EndGetContext(result);

                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.RawUrl.StartsWith("/events"))
                {
                    response.ContentType = "text/event-stream";
                    var writer = new StreamWriter(response.OutputStream);
                    _eventSessions.Add(writer);
                }
                else
                {
                    try
                    {
                        byte[] bytes = Get(request);
                        response.ContentLength64 = bytes.Length;
                        response.OutputStream.Write(bytes, 0, bytes.Length);
                        response.OutputStream.Close();
                    }
                    catch (FileNotFoundException)
                    {
                        response.StatusCode = 404;
                        response.OutputStream.Close();
                    }
                    catch (DirectoryNotFoundException)
                    {
                        response.StatusCode = 404;
                        response.OutputStream.Close();
                    }

                    context.Response.Close();
                    listener.BeginGetContext(Context, listener);
                }
            }
            catch (ObjectDisposedException)
            {
                //Intentionally not doing anything with the exception.
            }
        }

        public static bool HasActiveSessions()
        {
            return _eventSessions.Count > 0;
        }

        public void PostStatus(string status)
        {
            var failed = new List<StreamWriter>();
            foreach (StreamWriter writer in _eventSessions)
            {
                try
                {
                    writer.Write("status: {0}\n\n", status);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    failed.Add(writer);
                }
            }

            foreach (StreamWriter w in failed)
            {
                _eventSessions.Remove(w);
            }
        }

        public void PostEvent(string evt)
        {
            var failed = new List<StreamWriter>();
            foreach (StreamWriter writer in _eventSessions)
            {
                try
                {
                    writer.Write("data: {0}\n\n", evt);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    failed.Add(writer);
                }
            }

            foreach (StreamWriter w in failed)
                _eventSessions.Remove(w);
        }

        private static byte[] Get(HttpListenerRequest request)
        {
            if (request.RawUrl == "/")
            {
                return StaticContent("ClientApp\\Index.html");
            }

            return StaticContent($"ClientApp\\{request.RawUrl}");
        }

        private static byte[] StaticContent(string relativeName)
        {
            Stream stream = File.OpenRead(relativeName);

            if (stream == null) return null;

            string content = new StreamReader(stream).ReadToEnd();
            return Encoding.UTF8.GetBytes(content);
        }
    }
}