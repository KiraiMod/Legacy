using MelonLoader;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace KiraiMod
{
    public class IPC
    {
        private HttpListener listener;
        private Thread thread;

        private static MethodInfo method = null;
        private static bool execute = false;
        private static bool unloadForwarder = false;

        public IPC()
        {
            MelonLogger.Log("[IPC] Starting");

            listener = new HttpListener();
            listener.Prefixes.Add("http://*:53065/");

            thread = new Thread(() => Handler());
            thread.IsBackground = true;
            thread.Name = "KiraiIPC";

            try
            {
                listener.Start();
                thread.Start();
                MelonCoroutines.Start(Forwarder());
            }
            catch (Exception e) { 
                MelonLogger.Log("[IPC] Failed to start"); 
                MelonLogger.LogError(e.ToString()); 
            }
        }

        ~IPC()
        {
            MelonLogger.Log("[IPC] Stoping");

            try
            {
                thread.Abort();
                listener.Stop();
            }
            catch (Exception e)
            {
                MelonLogger.Log("[IPC] Failed to stop");
                MelonLogger.LogError(e.ToString());
            }
        }

        public System.Collections.IEnumerator Forwarder()
        {
            for (;;)
            {
                if (unloadForwarder) yield break;

                if (execute)
                {
                    try { method.Invoke(null, null); }
                    catch { MelonLogger.Log("[IPC] Forwarder failed to execute method."); }

                    execute = false;
                }

                yield return new UnityEngine.WaitForSeconds(1);
            }
        }

        public void Handler()
        {
            for (;;)
            {
                try
                {
                    HttpListenerContext ctx = listener.GetContext();
                    if (!ctx.Request.IsLocal)
                    {
                        MelonLogger.Log("[IPC] Non local attempted to access IPC.");
                        ctx.Response.StatusCode = 403;
                        byte[] resp = System.Text.Encoding.UTF8.GetBytes("Forbidden. This incident has be reported.");
                        ctx.Response.OutputStream.Write(resp, 0, resp.Length);
                        ctx.Response.Close();
                        continue;
                    }

                    if (ctx.Request.HttpMethod == "GET") GET(ctx);
                    else if (ctx.Request.HttpMethod == "POST") POST(ctx);
                }
                catch (Exception e)
                {
                    MelonLogger.Log($"[IPC] {e.Message}");
                    break;
                }
            }
        }

        public void GET(HttpListenerContext ctx)
        {
            MelonLogger.Log("Sending Web UI");
            ctx.Response.ContentType = "text/html";

            try
            {
                System.IO.Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.WebUI.html");
                resource.CopyTo(ctx.Response.OutputStream);
            } catch { MelonLogger.LogError("Failed to get Web UI"); }

            ctx.Response.Close();
        }

        public void POST(HttpListenerContext ctx)
        {
            byte[] raw = new byte[65536];
            int len = ctx.Request.InputStream.Read(raw, 0, 65536);

            if (len < raw.Length)
            {
                byte[] data = new byte[len];
                Array.Copy(raw, 0, data, 0, len);

                MelonLoader.TinyJSON.Variant parsed = MelonLoader.TinyJSON.JSON.Load(System.Text.Encoding.UTF8.GetString(data));
                if (parsed != null)
                {
                    string[] ipc = parsed.Make<string[]>();
                    byte[] resp = new byte[0];
                    if (ipc != null) ctx.Response.StatusCode = HandleIPC(ctx, ipc, out resp);
                    ctx.Response.Close(resp, false);
                    return;
                }
                else ctx.Response.StatusCode = 400;
            }
            else ctx.Response.StatusCode = 413;
            ctx.Response.Close();
        }

        private int HandleIPC(HttpListenerContext ctx, string[] args, out byte[] resp)
        {
            resp = new byte[0];
            if (args.Length < 1) return 400;

            string[] trimmed = args.Skip(1).ToArray();

            switch (args[0].ToLower())
            {
                case "rpc":
                    return HandleRPC(trimmed, out resp);
                case "get":
                    return HandleGET(trimmed, out resp);
                case "set":
                    return HandleSET(trimmed, out resp);
                default:
                    return 400;
            }
        }

        private int HandleRPC(string[] args, out byte[] resp)
        {
            resp = new byte[0];
            if (args.Length != 1) return 400;

            method = typeof(Registry).GetMethod(args[0], BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                MelonLogger.Log("[IPC] Unknown method");
                return 404;
            }

            if (!execute)
            {
                execute = true;
            } else MelonLogger.LogWarning("[IPC] Forwarder is busy!");

            try
            {
                method.Invoke(null, null);
            }
            catch { MelonLogger.Log("[IPC] Unknown exception within reflection invocation."); }
            
            return 200;
        }

        private int HandleGET(string[] args, out byte[] resp)
        {
            resp = new byte[0];
            if (args.Length != 1) return 400;

            PropertyInfo prop = typeof(Registry).GetProperty(args[0], BindingFlags.Public | BindingFlags.Static);
            if (prop == null)
            {
                MelonLogger.Log("[IPC] Unknown property");
                return 404;
            }

            resp = System.Text.Encoding.UTF8.GetBytes(prop.GetValue(null).ToString());
            return 200;
        }

        private int HandleSET(string[] args, out byte[] resp)
        {
            resp = new byte[0];
            if (args.Length != 2) return 400;

            PropertyInfo prop = typeof(Registry).GetProperty(args[0], BindingFlags.Public | BindingFlags.Static);
            if (prop == null)
            {
                MelonLogger.Log("[IPC] Unknown property");
                return 404;
            }

            object parsed;
            try { parsed = prop.PropertyType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { args[1] }); }
            catch { return 400; }

            prop.SetValue(null, parsed);
            return 200;
        }

        private static class Registry
        {
            public static bool bSpeed     { get { return Shared.modules.speed    .state; } set { Shared.modules.speed    .SetState(value); } }
            public static bool bFlight    { get { return Shared.modules.flight   .state; } set { Shared.modules.flight   .SetState(value); } }
            public static bool bNoclip    { get { return Shared.modules.noclip   .state; } set { Shared.modules.noclip   .SetState(value); } }
            public static bool bESP       { get { return Shared.modules.esp      .state; } set { Shared.modules.esp      .SetState(value); } }
            //public static bool bMuteSelf  { get { return Shared.modules.mute     .state; } set { Shared.modules.mute     .SetState(value); } }
            public static bool bOrbit     { get { return Shared.modules.orbit    .state; } set { Shared.modules.orbit    .SetState(value); } }
            public static bool bItemOrbit { get { return Shared.modules.itemOrbit.state; } set { Shared.modules.itemOrbit.SetState(value); } }

            public static bool bWorldTriggers { get { return Shared.Options.bWorldTriggers; } set { Shared.Options.bWorldTriggers = value; } }

            public static void pBringPickups() { Helper.BringPickups(); }
            public static void pDropTarget()   { Helper  .DropTarget(); }
        }
    }
}
