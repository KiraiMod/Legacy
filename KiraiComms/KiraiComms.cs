using KiraiLibs;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using VRC;

[assembly: MelonInfo(typeof(KiraiMod.KiraiComms), "KiraiComms", "0.2.0", "Kirai Chan#8315 & Brass")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace KiraiMod
{
    public class KiraiComms : MelonMod
    {
        private static bool bOSLPush;

        static KiraiComms()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
#if DEBUG
                "KiraiMod.Lib.KiraiLib.dll"
#else
                "KiraiMod.Lib.KiraiLibLoader.dll"
#endif
                );

            MemoryStream mem = new MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            Assembly.Load(mem.ToArray());

#if !DEBUG
            new Action(() => KiraiLibLoader.Load())();
#endif
        }

        private static Dictionary<string, RSA> encryptors = new Dictionary<string, RSA>();
        private static RSA ourRSA;
        private static bool halt = false;

        public override void OnApplicationStart()
        {
            KiraiLib.Libraries.LoadLibrary("KiraiRPC").ContinueWith((res) => Offloader.Initialize(res.Result));

            ourRSA = RSA.Create();

            KiraiLib.Callbacks.OnUIReload += () => VRChat_OnUiManagerInit();

            MelonCoroutines.Start(WaitForNetworkManager());
        }

        private System.Collections.IEnumerator WaitForNetworkManager()
        {
            while (NetworkManager.field_Internal_Static_NetworkManager_0 is null) yield return null;

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_VRCEventDelegate_1_Player_1
                    .field_Private_HashSet_1_UnityAction_1_T_0
                    .Add(new Action<Player>(player => OnPlayerLeft(player)));
                LogWithPadding("OnPlayerLeft", true);
            }
            catch { LogWithPadding("OnPlayerLeft", false); }
        }

        public override void VRChat_OnUiManagerInit()
        {
            if (halt) return;

            KiraiLib.UI.Initialize();

            KiraiLib.UI.Button.Create("uim/message", "Message", "Send a message to this user", -1, -3, KiraiLib.UI.UserInteractMenu.transform, new Action(() =>
            {
                string name = QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName;
                if (encryptors.TryGetValue(name, out RSA rsa)) {
                    KiraiLib.HUDInput(
                        $"Message <color={NameToPlayer(name).field_Private_APIUser_0.GetTrustColor().ToHex()}>{name}</color>",
                        "Send",
                        "Enter your message...",
                        new Action<string>(
                            (val) =>
                    {
                        if (val.Length <= 128)
                            Offloader.SendRPC("KiraiComms", 0x002, name, Convert.ToBase64String(rsa.EncryptValue(System.Text.Encoding.UTF8.GetBytes(val))));
                        else KiraiLib.Logger.Log("Messages can only be 128 characters long.");
                    }));
                }
                else KiraiLib.Logger.Log($"{QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName} is not using KiraiComms");
            }));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (bOSLPush) KiraiLib.SDK.Events.OnSceneLoad(buildIndex, sceneName);
        }

        private void OnPlayerLeft(Player player)
        {
            encryptors.Remove(player.field_Private_APIUser_0.displayName);
        }

        private static void LogWithPadding(string src, bool passed)
        {
            MelonLogger.Msg($"Hooking {src}...".PadRight(71, ' ') + (passed ? "Passed" : "Failed"));
        }

        private static Player NameToPlayer(string name)
        {
            foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.field_Private_APIUser_0.displayName == name)
                    return player;
            }

            return null;
        }

        internal static class Offloader
        {
            internal static void Initialize(short res)
            {
                if (res > 0)
                {
                    KiraiRPC.Callback += new Action<KiraiRPC.RPCData>((data) =>
                    {
                        if (data.target == "KiraiRPC")
                        {
                            if (data.id == (int)KiraiRPC.RPCEventIDs.OnInit)
                            {
                                RSAParameters rsadata = ourRSA.ExportParameters(false);
                                string exponent = Convert.ToBase64String(rsadata.Exponent);
                                string modulus = Convert.ToBase64String(rsadata.Modulus);

                                KiraiRPC.SendRPC("KiraiComms", 0x000, data.sender, exponent, modulus);
                            }
                        }
                        else if (data.target == "KiraiComms")
                        {
                            switch (data.id)
                            {
                                case 0x000:
                                case 0x001:
                                    {
                                        if (data.parameters[0] == VRC.Core.APIUser.CurrentUser.displayName)
                                        {
                                            // we recieved their rsa public
                                            RSAParameters rsadata = new RSAParameters
                                            {
                                                Exponent = Convert.FromBase64String(data.parameters[1]),
                                                Modulus = Convert.FromBase64String(data.parameters[2])
                                            };

                                            RSA rsa = RSA.Create();
                                            rsa.ImportParameters(rsadata);

                                            encryptors[data.sender] = rsa;

                                            if (data.id == 0x000 && data.sender != VRC.Core.APIUser.CurrentUser.displayName)
                                            {
                                                // send our rsa public
                                                RSAParameters rsadata2 = ourRSA.ExportParameters(false);
                                                KiraiRPC.SendRPC("KiraiComms", 0x001, data.sender, Convert.ToBase64String(rsadata2.Exponent), Convert.ToBase64String(rsadata2.Modulus));
                                            }
                                        }
                                    }
                                    break;
                                case 0x002:
                                    {
                                        if (data.parameters[0] == VRC.Core.APIUser.CurrentUser.displayName)
                                        {
                                            string str = System.Text.Encoding.UTF8.GetString(ourRSA.DecryptValue(Convert.FromBase64String(data.parameters[1]))).Replace("\0", "");

                                            KiraiLib.Logger.Display($"<color={NameToPlayer(data.sender).field_Private_APIUser_0.GetTrustColor().ToHex()}>{data.sender}</color>: {str}", 10);
                                        }
                                        break;
                                    }
                            }
                        }
                    });
                }

                if (res == 1) bOSLPush = true;
            }

            internal static void SendRPC(string name, int id, params string[] payload) => KiraiRPC.SendRPC(name, id, payload); 
        }
    }
}
