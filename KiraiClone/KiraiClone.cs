﻿using MelonLoader;
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

        ApiFileUtils utils;

        private MethodInfo UBPU;

        public KiraiClone()
        {
            Stream stream;
            MemoryStream mem;

            #region Load KiraiLibLoader
            if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "KiraiLibLoader"))
            {
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Lib.KiraiLibLoader.dll");
                mem = new MemoryStream((int)stream.Length);
                stream.CopyTo(mem);

                Assembly.Load(mem.ToArray());

                new Action(() => KiraiLibLoader.Load())();
            }
            #endregion
            #region Load UBPU
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Lib.UBPU.exe");
            mem = new MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            UBPU = Assembly.Load(mem.ToArray())
                .GetTypes()
                .First(t => t.Name == "Program")
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(method => method.Name == "Main");
            #endregion
            #region Load All Extra Dependencies
            foreach (string resource in new string[] {
                "KiraiMod.Lib.AsyncEnumerable.dll",
                "KiraiMod.Lib.Blake2Sharp.dll",
                "KiraiMod.Lib.DotZLib.dll",
                "KiraiMod.Lib.librsync.net.dll",
                "KiraiMod.Lib.LZ4.dll",
                "KiraiMod.Lib.Microsoft.Bcl.AsyncInterfaces.dll",
                "KiraiMod.Lib.Mono.Cecil.dll",
                "KiraiMod.Lib.Mono.Cecil.Mdb.dll",
                "KiraiMod.Lib.Mono.Cecil.Pdb.dll",
                "KiraiMod.Lib.Mono.Cecil.Rocks.dll",
                "KiraiMod.Lib.MonoMod.RuntimeDetour.dll",
                "KiraiMod.Lib.MonoMod.Utils.dll",
                "KiraiMod.Lib.SevenZip.dll",
                "KiraiMod.Lib.System.Threading.Tasks.Extensions.dll",
                "KiraiMod.Lib.websocket-sharp.dll",
                })
            {
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                mem = new MemoryStream((int)stream.Length);
                stream.CopyTo(mem);

                Assembly.Load(mem.ToArray());
            }
            #endregion

            if (Directory.Exists("./Temp"))
                Directory.Delete("./Temp", true);
            Directory.CreateDirectory("./Temp");
        }

        public override void OnApplicationStart()
        {
            MelonLogger.Log("Full recode by Kirai Chan et al");
            MelonLogger.Log("$40 for something slower than hotswap?");
            MelonLogger.Log("Good job Astro and Spacers.VIP but reuploader is flawed");
            MelonLogger.Log("20 required assemblies? No thanks");
            MelonLogger.Log("Shouldn't have banned me for association to Astro");
            MelonLogger.Log("wikipedia.org/wiki/Clean_room_design");
            MelonLogger.Log("Nothing from reuploader is stolen :D");

            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<ApiFileUtils>();
            var go = new UnityEngine.GameObject();
            utils = go.AddComponent<ApiFileUtils>();
            UnityEngine.Object.DontDestroyOnLoad(go);

            KiraiLib.Callbacks.OnUIReload += () =>
            {
                VRChat_OnUiManagerInit();
            };
        }

        private static bool NoOp()
        {
            return false;
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
            if (avatar is null)
            {
                KiraiLib.Logger.Log("Avatar is null");
                return;
            }

            inProgress.Add(avatar.id);

            var imageUrl = avatar.imageUrl;
            var assetUrl = avatar.assetUrl;
            var id = avatar.id;
            var name = avatar.name;
            var description = avatar.description;

            KiraiLib.Logger.Log($"KiraiCloning {name}");
            await Download(imageUrl, id);
            KiraiLib.Logger.Log("Downloaded VRCA");
            string newID = await CreateID();
            KiraiLib.Logger.Log($"Created ID");
            await Uncompress(id);
            KiraiLib.Logger.Log("Decompressed Original");
            File.Delete($"./Temp/{id}");
            KiraiLib.Logger.Log("Deleted Original");
            Replace(id, newID);
            KiraiLib.Logger.Log("Replaced ID");
            await Task.Delay(500);
            Recompress(id);
            KiraiLib.Logger.Log("Recompressed Modified");
            File.Move($"./Temp/{id}.LZ4HC", $"./Temp/{id}.vrca");
            KiraiLib.Logger.Log("Renamed Modified");
            await Task.Delay(500);

            ApiFileUtils.UploadFileAsync($"./Temp/{id}.vrca", null, name, (ApiFile vrca, string _) =>
            {
                KiraiLib.Logger.Log("Uploaded VRCA");
                string imagePath = DownloadImage(imageUrl, id).Result;
                KiraiLib.Logger.Log("Downloaded Image");

                ApiFileUtils.UploadFileAsync(imagePath, null, vrca.GetFileURL(), (ApiFile image, string __) =>
                    {
                        KiraiLib.Logger.Log("Uploaded Image");

                        var avtr = new ApiAvatar
                        {
                            id = id,
                            authorName = APIUser.CurrentUser.username,
                            authorId = APIUser.CurrentUser.id,
                            name = name,
                            imageUrl = image.GetFileURL(),
                            assetUrl = vrca.GetFileURL(),
                            description = description,
                            releaseStatus = "private"
                        };

                        var Last = new Action(() =>
                        {
                            Directory.Delete($"./Temp/{id}_dump", true);
                            File.Delete($"./Temp/{id}.vrca");
                            File.Delete($"./Temp/{id}.xml");
                            File.Delete(imagePath);

                            KiraiLib.Logger.Log($"Cleaned up");

                            inProgress.Remove(id);
                        });

                        avtr.Post(
                            UnhollowerRuntimeLib.DelegateSupport.ConvertDelegate<Il2CppSystem.Action<ApiContainer>>(new Action<ApiContainer>((___) =>
                            {
                                KiraiLib.Logger.Log($"Successfully KiraiCloned {name}");

                                Last();
                            })),
                            UnhollowerRuntimeLib.DelegateSupport.ConvertDelegate<Il2CppSystem.Action<ApiContainer>>(new Action<ApiContainer>((___) =>
                            {
                                KiraiLib.Logger.Log($"Failed to KiraiClone {name}");

                                Last();
                            })));
                    },
                    (ApiFile a, string s1) => { },
                    (ApiFile a, string s1, string s2, float p) => { },
                    (VRC.Core.ApiFile Assets) => false);
            },
            (ApiFile a, string s) => { },
            (ApiFile a, string s, string s2, float f) => { },
            (ApiFile a) => { return false; });
        }

        private async Task<string> DownloadImage(string imageUrl, string id)
        {
            HttpResponseMessage httpResponseMessage = await new HttpClient().GetAsync(imageUrl);
            byte[] array = await httpResponseMessage.Content.ReadAsByteArrayAsync();

            string text = $"./Temp/{id}.{httpResponseMessage.Content.Headers.GetValues("Content-Type").First().Split('/')[1]}";

            File.WriteAllBytes(text, array);
            return text;
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

        private async Task Download(string assetUrl, string id)
        {
            byte[] bytes = await client.GetByteArrayAsync(assetUrl);
            File.WriteAllBytes($"./Temp/{id}", bytes);
        }

        private async Task Uncompress(string id)
        {
            UBPU.Invoke(null, new object[] { new string[] { $"./Temp/{id}" } });

            while (!Directory.Exists($"./Temp/{id}_dump")) await Task.Delay(10);
        }

        private void Replace(string id, string newID)
        {
            string[] files = Directory.EnumerateFiles($"./Temp/{id}_dump")
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

            byte[] bytes = File.ReadAllBytes($"./Temp/{id}_dump/{res}");

            File.WriteAllBytes($"./Temp/{id}_dump/{res}",
                ReplaceBytes(bytes, Encoding.UTF8.GetBytes(id), Encoding.UTF8.GetBytes(newID)));
        }

        private void Recompress(string id)
        {
            Directory.SetCurrentDirectory("Temp");
            UBPU.Invoke(null, new object[] { new string[] { $"./{id}.xml", "lz4hc" } });
            Directory.SetCurrentDirectory("../");
            return;
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
