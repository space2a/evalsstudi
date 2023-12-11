using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;

using JO2024.Actions;
using JO2024.CustomPages;

using Org.BouncyCastle.Asn1.Ocsp;

namespace JO2024.WebServers
{
    internal class WebServer
    {
        public readonly ushort WebPort;

        public Dictionary<string, REQUESTANSWER> REQUESTANSWERs = new Dictionary<string, REQUESTANSWER>();

        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private Queue<HttpListenerContext> _queue;

        private long inUse = 0;
        private long tradesTotal = 0;
        private long bytesSent = 0;


        public WebServer(int maxThreads, ushort webPort)
        {
            WebPort = webPort;

            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
        }


        public bool Start()
        {
            try
            {
                _listener.Prefixes.Add(String.Format(@"http://*:" + WebPort + "/", WebPort + "/"));

                _listener.Start();
                _listenerThread.Start();

                for (int i = 0; i < _workers.Length; i++)
                {
                    _workers[i] = new Thread(Worker);
                    _workers[i].Start();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                if (tradesTotal == 0)
                    Console.WriteLine("An error occured while starting the server, please make sure to run as administrator");
                else
                    Console.WriteLine("errors =>" + ex.ToString());
                Console.ResetColor();
                return false;
            }

        }

        public bool Stop()
        {
            try
            {
                _stop.Set();
                _listenerThread.Join();
                foreach (Thread worker in _workers)
                    worker.Join();
                _listener.Stop();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(ContextReady, null);

                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
                    return;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(_listener.EndGetContext(ar));
                    _ready.Set();
                }
            }
            catch { return; }
        }

        private void Worker()
        {
            WaitHandle[] wait = new[] { _ready, _stop };
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try { ClientWorkerAsync(context); }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }

        public void ClientWorkerAsync(HttpListenerContext context)
        {
            inUse++;
            HttpListenerContext ctx = context;

            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;

            string text;
            using (var reader = new StreamReader(req.InputStream,
                                                 req.ContentEncoding))
            {
                text = reader.ReadToEnd();
            }

            if (REQUESTANSWERs.TryGetValue(req.RawUrl.Remove(0, 1), out REQUESTANSWER pa))
            {
                var request = CreateRequest(text);
                Console.WriteLine("WS => " + text);
                pa.RequestFunction(new WEBCLIENT() { Ip = req.RemoteEndPoint.Address.ToString(), Context = ctx }, request);
                
                if (pa.REQUESTTYPE == REQUESTANSWER.RequesType.Post || String.IsNullOrWhiteSpace(request.Answer)) { resp.Close(); return; }

                var answer = Encoding.UTF8.GetBytes(request.Answer);
                resp.OutputStream.Write(answer, 0, answer.Length);
                resp.Close();
                return;
            }

            Console.WriteLine(text + " => " + req.RawUrl);

            byte[] data = new byte[0];
            resp.AddHeader("Server", "JO2024");

            if (req.RawUrl == "/")
                data = Home.BuildHomePage();
            else if (req.RawUrl.ToLower() == "/admin")
                data = Admin.BuildAdminPage(ctx);
            else if (req.RawUrl.ToLower() == "/monespace")
                data = MonEspace.BuildMonEspacePage(ctx);

            else if (Directory.Exists("webcontent/" + req.RawUrl.Replace("/", "")))
            {
                if (File.Exists("webcontent" + req.RawUrl + "/index.html"))
                    data = File.ReadAllBytes("webcontent" + req.RawUrl + "/index.html");
            }
            else if (File.Exists("webcontent" + req.RawUrl))
                data = File.ReadAllBytes("webcontent" + req.RawUrl);
            else
                data = File.ReadAllBytes("webcontent/errors/404.html");

            resp.ContentEncoding = Encoding.Unicode;
            resp.ContentLength64 = data.LongLength;

            bytesSent += data.Length;
            inUse--;
            tradesTotal++;
            resp.OutputStream.WriteAsync(data, 0, data.Length);
        }

        public REQUEST CreateRequest(string post)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(post);

            var parameters = new Dictionary<string, string>();
            try
            {
                foreach (string key in nameValueCollection)
                {
                    parameters.Add(key, nameValueCollection[key]);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Impossible de traiter la requette.. : " + ex.ToString());
            }

            return new REQUEST() { Parameters = parameters, RawData = post };
        }


        public string GetMIMEType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension.Length > 0 &&
                MIMETypesDictionary.ContainsKey(extension.Remove(0, 1)))
                return MIMETypesDictionary[extension.Remove(0, 1)];

            return "application/octet-stream";
        }

        private static readonly Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string>
        {
          {"ai", "application/postscript"},
          {"aif", "audio/x-aiff"},
          {"aifc", "audio/x-aiff"},
          {"aiff", "audio/x-aiff"},
          {"asc", "text/plain"},
          {"atom", "application/atom+xml"},
          {"au", "audio/basic"},
          {"avi", "video/x-msvideo"},
          {"bcpio", "application/x-bcpio"},
          {"bin", "application/octet-stream"},
          {"bmp", "image/bmp"},
          {"cdf", "application/x-netcdf"},
          {"cgm", "image/cgm"},
          {"class", "application/octet-stream"},
          {"cpio", "application/x-cpio"},
          {"cpt", "application/mac-compactpro"},
          {"csh", "application/x-csh"},
          {"css", "text/css"},
          {"dcr", "application/x-director"},
          {"dif", "video/x-dv"},
          {"dir", "application/x-director"},
          {"djv", "image/vnd.djvu"},
          {"djvu", "image/vnd.djvu"},
          {"dll", "application/octet-stream"},
          {"dmg", "application/octet-stream"},
          {"dms", "application/octet-stream"},
          {"doc", "application/msword"},
          {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
          {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
          {"docm","application/vnd.ms-word.document.macroEnabled.12"},
          {"dotm","application/vnd.ms-word.template.macroEnabled.12"},
          {"dtd", "application/xml-dtd"},
          {"dv", "video/x-dv"},
          {"dvi", "application/x-dvi"},
          {"dxr", "application/x-director"},
          {"eps", "application/postscript"},
          {"etx", "text/x-setext"},
          {"exe", "application/octet-stream"},
          {"ez", "application/andrew-inset"},
          {"gif", "image/gif"},
          {"gram", "application/srgs"},
          {"grxml", "application/srgs+xml"},
          {"gtar", "application/x-gtar"},
          {"hdf", "application/x-hdf"},
          {"hqx", "application/mac-binhex40"},
          {"htm", "text/html"},
          {"html", "text/html"},
          {"ice", "x-conference/x-cooltalk"},
          {"ico", "image/x-icon"},
          {"ics", "text/calendar"},
          {"ief", "image/ief"},
          {"ifb", "text/calendar"},
          {"iges", "model/iges"},
          {"igs", "model/iges"},
          {"jnlp", "application/x-java-jnlp-file"},
          {"jp2", "image/jp2"},
          {"jpe", "image/jpeg"},
          {"jpeg", "image/jpeg"},
          {"jpg", "image/jpeg"},
          {"js", "application/x-javascript"},
          {"kar", "audio/midi"},
          {"latex", "application/x-latex"},
          {"lha", "application/octet-stream"},
          {"lzh", "application/octet-stream"},
          {"m3u", "audio/x-mpegurl"},
          {"m4a", "audio/mp4a-latm"},
          {"m4b", "audio/mp4a-latm"},
          {"m4p", "audio/mp4a-latm"},
          {"m4u", "video/vnd.mpegurl"},
          {"m4v", "video/x-m4v"},
          {"mac", "image/x-macpaint"},
          {"man", "application/x-troff-man"},
          {"mathml", "application/mathml+xml"},
          {"me", "application/x-troff-me"},
          {"mesh", "model/mesh"},
          {"mid", "audio/midi"},
          {"midi", "audio/midi"},
          {"mif", "application/vnd.mif"},
          {"mov", "video/quicktime"},
          {"movie", "video/x-sgi-movie"},
          {"mp2", "audio/mpeg"},
          {"mp3", "audio/mpeg"},
          {"mp4", "video/mp4"},
          {"mpe", "video/mpeg"},
          {"mpeg", "video/mpeg"},
          {"mpg", "video/mpeg"},
          {"mpga", "audio/mpeg"},
          {"ms", "application/x-troff-ms"},
          {"msh", "model/mesh"},
          {"mxu", "video/vnd.mpegurl"},
          {"nc", "application/x-netcdf"},
          {"oda", "application/oda"},
          {"ogg", "application/ogg"},
          {"pbm", "image/x-portable-bitmap"},
          {"pct", "image/pict"},
          {"pdb", "chemical/x-pdb"},
          {"pdf", "application/pdf"},
          {"pgm", "image/x-portable-graymap"},
          {"pgn", "application/x-chess-pgn"},
          {"pic", "image/pict"},
          {"pict", "image/pict"},
          {"png", "image/png"},
          {"pnm", "image/x-portable-anymap"},
          {"pnt", "image/x-macpaint"},
          {"pntg", "image/x-macpaint"},
          {"ppm", "image/x-portable-pixmap"},
          {"ppt", "application/vnd.ms-powerpoint"},
          {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
          {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
          {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
          {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
          {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
          {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
          {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
          {"ps", "application/postscript"},
          {"qt", "video/quicktime"},
          {"qti", "image/x-quicktime"},
          {"qtif", "image/x-quicktime"},
          {"ra", "audio/x-pn-realaudio"},
          {"ram", "audio/x-pn-realaudio"},
          {"ras", "image/x-cmu-raster"},
          {"rdf", "application/rdf+xml"},
          {"rgb", "image/x-rgb"},
          {"rm", "application/vnd.rn-realmedia"},
          {"roff", "application/x-troff"},
          {"rtf", "text/rtf"},
          {"rtx", "text/richtext"},
          {"sgm", "text/sgml"},
          {"sgml", "text/sgml"},
          {"sh", "application/x-sh"},
          {"shar", "application/x-shar"},
          {"silo", "model/mesh"},
          {"sit", "application/x-stuffit"},
          {"skd", "application/x-koan"},
          {"skm", "application/x-koan"},
          {"skp", "application/x-koan"},
          {"skt", "application/x-koan"},
          {"smi", "application/smil"},
          {"smil", "application/smil"},
          {"snd", "audio/basic"},
          {"so", "application/octet-stream"},
          {"spl", "application/x-futuresplash"},
          {"src", "application/x-wais-source"},
          {"sv4cpio", "application/x-sv4cpio"},
          {"sv4crc", "application/x-sv4crc"},
          {"svg", "image/svg+xml"},
          {"swf", "application/x-shockwave-flash"},
          {"t", "application/x-troff"},
          {"tar", "application/x-tar"},
          {"tcl", "application/x-tcl"},
          {"tex", "application/x-tex"},
          {"texi", "application/x-texinfo"},
          {"texinfo", "application/x-texinfo"},
          {"tif", "image/tiff"},
          {"tiff", "image/tiff"},
          {"tr", "application/x-troff"},
          {"tsv", "text/tab-separated-values"},
          {"txt", "text/plain"},
          {"ustar", "application/x-ustar"},
          {"vcd", "application/x-cdlink"},
          {"vrml", "model/vrml"},
          {"vxml", "application/voicexml+xml"},
          {"wav", "audio/x-wav"},
          {"wbmp", "image/vnd.wap.wbmp"},
          {"wbmxl", "application/vnd.wap.wbxml"},
          {"wml", "text/vnd.wap.wml"},
          {"wmlc", "application/vnd.wap.wmlc"},
          {"wmls", "text/vnd.wap.wmlscript"},
          {"wmlsc", "application/vnd.wap.wmlscriptc"},
          {"wrl", "model/vrml"},
          {"xbm", "image/x-xbitmap"},
          {"xht", "application/xhtml+xml"},
          {"xhtml", "application/xhtml+xml"},
          {"xls", "application/vnd.ms-excel"},
          {"xml", "application/xml"},
          {"xpm", "image/x-xpixmap"},
          {"xsl", "application/xml"},
          {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
          {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
          {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
          {"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
          {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
          {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
          {"xslt", "application/xslt+xml"},
          {"xul", "application/vnd.mozilla.xul+xml"},
          {"xwd", "image/x-xwindowdump"},
          {"xyz", "chemical/x-xyz"},
          {"zip", "application/zip"}
        };
    }

    public struct REQUESTANSWER
    {
        public string PostActionName { get; set; }
        public Action<WEBCLIENT, REQUEST> RequestFunction { get; set; }

        public RequesType REQUESTTYPE;

        public enum RequesType : byte
        {
            Get,
            Post
        }
    }

    public class REQUEST
    {
        public string RawData;
        public Dictionary<string, string> Parameters;

        public string Answer;
    }

    public struct WEBCLIENT
    {
        public string Ip;
        public HttpListenerContext Context;

        public Cookie GetCookie(string cookieName)
        {
            var cookies = Context.Request.Cookies;

            // Rechercher le cookie "session"
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies)
                {
                    if (cookie.Name == cookieName)
                    {
                        return cookie;
                    }
                }
            }

            return null;
        }

        public void RedirectHome()
        {
            Context.Response.Redirect("/");
        }

        public void Redirect(string filePath)
        {
            var answer = Encoding.UTF8.GetBytes(File.ReadAllText("webcontent/" + filePath));
            Context.Response.OutputStream.Write(answer, 0, answer.Length);
            Context.Response.Close();
        }

        public void Redirect(byte[] content)
        {
            Context.Response.OutputStream.Write(content, 0, content.Length);
            Context.Response.Close();
        }
    }
}
