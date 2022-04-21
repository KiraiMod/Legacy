using MelonLoader;
using System;
using System.Net;
using System.Reflection;
using System.Threading;

namespace KiraiMod
{
    public class IPC
    {
        private HttpListener listener;
        private Thread thread;

        public IPC()
        {
            MelonModLogger.Log("[IPC] Starting");

            listener = new HttpListener();
            listener.Prefixes.Add("http://*:53065/");

            thread = new Thread(() => Handler());
            thread.IsBackground = true;
            thread.Name = "KiraiIPC";

            try { listener.Start(); }
            catch { MelonModLogger.LogError("Failed to start IPC"); }

            thread.Start();
        }

        ~IPC()
        {
            MelonModLogger.Log("[IPC] Stoping");

            listener.Stop();
            thread.Abort();
        }

        public void Handler()
        {
            for (;;)
            {
                HttpListenerContext ctx = listener.GetContext();
                if (!ctx.Request.IsLocal)
                {
                    MelonModLogger.Log("[IPC] Non local attempted to access IPC.");
                    ctx.Response.StatusCode = 403;
                    byte[] resp = System.Text.Encoding.UTF8.GetBytes("Forbidden. This incident has be reported.");
                    ctx.Response.OutputStream.Write(resp, 0, resp.Length);
                    ctx.Response.Close();
                    continue;
                }

                if (ctx.Request.HttpMethod == "GET") GET(ctx);
                else if (ctx.Request.HttpMethod == "POST") POST(ctx);
            }
        }

        public void GET(HttpListenerContext ctx)
        {
            MelonModLogger.Log("Sending Web UI");
            ctx.Response.ContentType = "text/html";

            try
            {
                System.IO.Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.WebUI.html");
                resource.CopyTo(ctx.Response.OutputStream);
            } catch { MelonModLogger.LogError("Failed to get Web UI"); }

            ctx.Response.Close();
        }

        public void POST(HttpListenerContext ctx)
        {
            byte[] raw = new byte[65536];
            int len = ctx.Request.InputStream.Read(raw, 0, 65536);

            if (len >= raw.Length)
            {
                ctx.Response.StatusCode = 413;
                ctx.Response.Close();
                return;
            }

            byte[] data = new byte[len];
            Array.Copy(raw, 0, data, 0, len);

            MelonLoader.TinyJSON.Variant parsed = MelonLoader.TinyJSON.JSON.Load(System.Text.Encoding.UTF8.GetString(data));
            if (parsed == null)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.Close();
                return;
            }

            string[] ipc = parsed.Make<string[]>();
            if (ipc == null || ipc.Length < 1)
            {
                ctx.Response.Close();
                return;
            }

            PropertyInfo prop = typeof(Registry).GetProperty(ipc[0], BindingFlags.Public | BindingFlags.Static);
            if (prop == null)
            {
                MelonModLogger.Log("[IPC] Unknown property");
                ctx.Response.StatusCode = 404;
                ctx.Response.Close();
                return;
            }

            if (ipc.Length == 1)
            {
                ctx.Response.Close(System.Text.Encoding.UTF8.GetBytes(prop.GetValue(null).ToString()), false);
                return;
            }
            else ctx.Response.Close();

            prop.SetValue(null, prop.PropertyType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { ipc[1] }));
        }

        private static class Registry
        {
            public static bool bSpeed    { get { return Shared.modules.speed .state; } set { Shared.modules.speed .SetState(value); } }
            public static bool bFlight   { get { return Shared.modules.flight.state; } set { Shared.modules.flight.SetState(value); } }
            public static bool bNoclip   { get { return Shared.modules.noclip.state; } set { Shared.modules.noclip.SetState(value); } }
            public static bool bESP      { get { return Shared.modules.esp   .state; } set { Shared.modules.esp   .SetState(value); } }
            public static bool bMuteSelf { get { return Shared.modules.mute  .state; } set { Shared.modules.mute  .SetState(value); } }
            public static bool bOrbit    { get { return Shared.modules.orbit .state; } set { Shared.modules.orbit .SetState(value); } }
            public static bool bInvis    { get { return Shared.modules.invis .state; } set { Shared.modules.invis .SetState(value); } }

            public static bool bWorldTriggers { get { return Shared.Options.bWorldTriggers; } set { Shared.Options.bWorldTriggers = value; } }
        }
    }
}
