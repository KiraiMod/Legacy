using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRC;
using VRC.Core;

[assembly: MelonInfo(typeof(KiraiMod.KiraiClone), "KiraiClone", null, "Kirai Chan#8315")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace KiraiMod
{
    public class KiraiClone : MelonMod
    {
        HttpClient client = new HttpClient();
        List<string> inProgress = new List<string>();

        private MethodInfo UBPU;

        public KiraiClone()
        {
            Stream stream;
            MemoryStream mem;

            if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "KiraiLib" || a.GetName().Name == "KiraiLibLoader"))
            {
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.KiraiLibLoader.dll");
                mem = new MemoryStream((int)stream.Length);
                stream.CopyTo(mem);

                Assembly.Load(mem.ToArray());

                new Action(() => KiraiLibLoader.Load())();
            }

            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.UBPU.exe");
            mem = new MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            UBPU = Assembly.Load(mem.ToArray())
                .GetTypes()
                .First(t => t.Name == "Program")
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(method => method.Name == "Main");

            if (!Directory.Exists("./Temp"))
                Directory.CreateDirectory("./Temp");
        }

        public override void OnApplicationStart()
        {
            MelonLogger.Log("Full recode by Kirai Chan et al");
            MelonLogger.Log("$40 for something slower than hotswap?");
            MelonLogger.Log("Good job Astro and Spacers.VIP but reuploader is flawed");
            MelonLogger.Log("20 required assemblies and a folder? No thanks");
            MelonLogger.Log("Shouldn't have banned me for association to Astro");
            MelonLogger.Log("wikipedia.org/wiki/Clean_room_design");
            MelonLogger.Log("Nothing from reuploader is stolen :D");

            KiraiLib.Callbacks.OnUIReload += () =>
            {
                VRChat_OnUiManagerInit();
            };
        }

        public override void VRChat_OnUiManagerInit()
        {
            KiraiLib.UI.Initialize();

            KiraiLib.UI.Button.Create(
                "uim/kirai-clone", 
                "Kirai\nClone", 
                "Reuploads the selected player's current avatar to your account",
                3, 1,
                KiraiLib.UI.UserInteractMenu.transform,
                new Action(() =>
                {
                    Player player = GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id);
                    if (player is null)
                    {
                        KiraiLib.Logger.Log("Failed to find the selected player in the instance");
                        return;
                    }

                    if (!player.prop_ApiAvatar_0.id.StartsWith("avtr_")) {
                        KiraiLib.Logger.Log("Avatar has a malformed id");
                        return;
                    }

                    if (inProgress.Contains(player.prop_ApiAvatar_0.id))
                    {
                        KiraiLib.Logger.Log("Avatar is already being reuploaded");
                        return;
                    }

                    Thread thread = new Thread(Reupload);
                    thread.IsBackground = true;
                    thread.Name = "KiraiClone-Helper";
                    thread.Start(player.prop_ApiAvatar_0);
                }));
        }

        public static Player GetPlayer(string id)
        {
            Il2CppSystem.Collections.Generic.List<Player> players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;

            for (int i = 0; i < players.Count; i++)
                if (players[i].field_Private_APIUser_0.id == id)
                    return players[i];

            return null;
        }

        private void Reupload(object obj)
        {
            Reupload(obj as ApiAvatar);
        }

        public async void Reupload(ApiAvatar avatar)
        {
            inProgress.Add(avatar.id);

            KiraiLib.Logger.Log($"KiraiCloning {avatar.name}");
            await Download(avatar);
            KiraiLib.Logger.Log("Downloaded");
            string id = await CreateID();
            KiraiLib.Logger.Log($"Created ID");
            await Uncompress(avatar);
            KiraiLib.Logger.Log("Decompressed");
            File.Delete($"./Temp/{avatar.id}");
            KiraiLib.Logger.Log("Deleted");
            Replace(avatar, id);
            KiraiLib.Logger.Log("Replaced ID");
            Recompress(avatar);
            KiraiLib.Logger.Log("Recompressed");
            File.Move($"./Temp/{avatar.id}.LZ4HC", $"./Temp/{avatar.id}.vrca");
            KiraiLib.Logger.Log("Moved");
            Upload(avatar);
            KiraiLib.Logger.Log("Reuploaded (NOT IMPLEMENTED)");
            MelonLogger.Log($"The generated id is: {id}");

            inProgress.Remove(avatar.id);
        }

        private async Task<string> CreateID()
        {
            string id = null;

            while (id is null)
            {
                string temp = $"avtr_{Guid.NewGuid()}";

                bool waiting = true;

                KiraiLib.Logger.Log($"Creating ID");
                API.Fetch<ApiAvatar>(temp, new Action<ApiContainer>(_ =>
                {
                    waiting = false;
                }), new Action<ApiContainer>(_ => {
                    waiting = false;
                    id = temp;
                }));

                while (waiting) await Task.Delay(10);
            }

            return id;
        }

        private async Task Download(ApiAvatar avatar)
        {
            byte[] bytes = await client.GetByteArrayAsync(avatar.assetUrl);
            File.WriteAllBytes($"./Temp/{avatar.id}", bytes);
        }

        private async Task Uncompress(ApiAvatar avatar)
        {
            UBPU.Invoke(null, new object[] { new string[] { $"./Temp/{avatar.id}" } });


            Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), $"./Temp/{avatar.id}_dump"));
            // todo fix
            while (!Directory.Exists($"./Temp/{avatar.id}_dump")) await Task.Delay(10);
        }

        private void Replace(ApiAvatar avatar, string id)
        {
            string[] files = Directory.EnumerateFiles($"./Temp/{avatar.id}_dump")
                .Select(n => new FileInfo(n))
                .OrderBy(f => f.CreationTime)
                .Select(f => f.Name)
                .ToArray();

            string res = null;
            if (files.Length != 1)
            {
                foreach (string file in files)
                    if (string.IsNullOrEmpty(Path.GetExtension(file)))
                        res = file;
            } else res = files[0];

            MelonLogger.Log(res);

            byte[] bytes = File.ReadAllBytes($"./Temp/{avatar.id}_dump/{res}");

            File.WriteAllBytes($"./Temp/{avatar.id}_dump/{res}",
                ReplaceBytes(bytes, Encoding.UTF8.GetBytes(avatar.id), Encoding.UTF8.GetBytes(id)));
        }

        private void Recompress(ApiAvatar avatar)
        {
            Directory.SetCurrentDirectory("Temp");
            UBPU.Invoke(null, new object[] { new string[] { $"./{avatar.id}.xml", "lz4hc" } });
            Directory.SetCurrentDirectory("../");
            return;
        }

        public static void Create(string name, string mimeType, string extension, Action<ApiContainer> successCallback, Action<ApiContainer> errorCallback)
        {
            new ApiFile
            {
                name = name,
                mimeType = mimeType,
                extension = extension
            }.Save(successCallback, errorCallback);
        }


        private void Upload(ApiAvatar avatar)
        {
            return;

            MelonLogger.Log("0");
            var a = new ApiFile
            {
                name = avatar.name,
                mimeType = "application/x-avatar",
                extension = ".vrca",
            };
            MelonLogger.Log("1");
            a.Save();
            MelonLogger.Log("2");


            MD5 md5 = MD5.Create();

            byte[] bytes = File.ReadAllBytes($"./Temp/{avatar.id}.vrca");

            string hash = "";
            foreach (byte b in md5.ComputeHash(bytes, 0, bytes.Length))
            {
                hash += b.ToString("X2");
            }
            MelonLogger.Log("3");


            //a.CreateNewVersion(ApiFile.Version.FileType.Full, hash, bytes.Length, sigMD5Base64, sigFileSize, _ => { }, _ => { });



            //application/x-world
            //application/x-avatar

            //MelonLogger.Log("0");
            //ApiFile.Create(avatar.name, "", ".vrca", new Action<ApiContainer>(container =>
            //{
            //    KiraiLib.Logger.Log("Recorded");

            //    ApiFile file = container.Model as ApiFile;

            //    MelonLogger.Log("1");
            //    file.StartSimpleUpload(ApiFile.Version.FileDescriptor.Type.file, new Action<ApiContainer>(resp =>
            //    {
            //        KiraiLib.Logger.Log("Uploading");

            //        MD5 md5 = MD5.Create();

            //        byte[] bytes = File.ReadAllBytes($"./Temp/{avatar.id}.vrca");

            //        string hash = "";
            //        foreach (byte b in md5.ComputeHash(bytes, 0, bytes.Length))
            //        {
            //            hash += b.ToString("X2");
            //        }

            //    MelonLogger.Log("2");
            //        ApiFile.PutSimpleFileToURL(
            //            (resp as ApiDictContainer).ResponseDictionary["url"].Cast<Il2CppSystem.String>(), 
            //            $"./Temp/{avatar.id}.vrca",
            //            "application/x-avatar", 
            //            hash, 
            //            new Action(() => { }), new Action<string>(_ => { }), new Action<long, long>((_, __) => { }));
            //    }), new Action<ApiContainer>(_ =>
            //    {
            //        KiraiLib.Logger.Log("Aborting");
            //    }));
            //}), new Action<ApiContainer>(container =>
            //{

            //    if (container.Code == 400)
            //    {
            //        KiraiLib.Logger.Log("Retrying");
            //        Upload(avatar);
            //    }
            //    else KiraiLib.Logger.Log("Aborting");
            //}));
        }

        public static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
            byte[] dst = null;
            byte[] temp = null;
            int index = FindBytes(src, search);
            while (index >= 0)
            {
                if (temp == null)
                    temp = src;
                else
                    temp = dst;

                dst = new byte[temp.Length - search.Length + repl.Length];

                Buffer.BlockCopy(temp, 0, dst, 0, index);
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                Buffer.BlockCopy(
                    temp,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    temp.Length - (index + search.Length));


                index = FindBytes(dst, search);
            }
            return dst;
        }
    }
}
