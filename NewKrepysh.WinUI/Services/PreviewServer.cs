using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NewKrepysh.WinUI.Services
{
    public class PreviewServer
    {
        private HttpListener? _listener;
        private string _rootDirectory = string.Empty;
        private int _port = 0;
        private bool _isRunning = false;

        public string BaseUrl => $"http://localhost:{_port}/";

        public string? Start(string rootDirectory)
        {
            if (_isRunning && _rootDirectory == rootDirectory)
            {
                return null; // Already running with same root
            }

            Stop();

            _rootDirectory = rootDirectory;
            
            // Try to find an available port starting at 8080
            for (int port = 8080; port < 8090; port++)
            {
                try
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://localhost:{port}/");
                    _listener.Start();
                    _port = port;
                    _isRunning = true;
                    break;
                }
                catch
                {
                    _listener?.Close();
                    _listener = null;
                }
            }

            if (_listener == null)
            {
                // Fallback to random dynamic port using TcpListener
                try
                {
                    _listener = new HttpListener();
                    int freePort = GetFreePort();
                    _listener.Prefixes.Add($"http://localhost:{freePort}/");
                    _listener.Start();
                    _port = freePort;
                    _isRunning = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to start preview server: {ex.Message}");
                    return null;
                }
            }

            _ = Task.Run(ListenLoop);

            return _listener.Prefixes.FirstOrDefault();
        }

        private int GetFreePort()
        {
            var l = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private async Task ListenLoop()
        {
            while (_isRunning && _listener != null)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(context));
                }
                catch
                {
                    break;
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                string urlPath = context.Request.Url?.AbsolutePath ?? "/";
                if (urlPath == "/") urlPath = "/index.html";

                string filePath = Path.Combine(_rootDirectory, urlPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(filePath))
                 {
                    byte[] bytes = File.ReadAllBytes(filePath);
                    context.Response.ContentType = GetContentType(filePath);
                    context.Response.ContentLength64 = bytes.Length;
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    using var writer = new StreamWriter(context.Response.OutputStream);
                    writer.Write("404 - Not Found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling preview server request: {ex.Message}");
            }
            finally
            {
                try
                {
                    context.Response.OutputStream.Close();
                }
                catch { }
            }
        }

        private string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }

        public void Stop()
        {
            _isRunning = false;
            if (_listener != null)
            {
                try
                {
                    _listener.Stop();
                    _listener.Close();
                }
                catch { }
                _listener = null;
            }
        }
    }
}
