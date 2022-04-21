using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using VRC;

[assembly: MelonInfo(typeof(KiraiMod.KiraiComms), "KiraiComms", null, "Kirai Chan#8315 & Brass")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace KiraiMod
{
    public class KiraiComms : MelonMod
    {
        static KiraiComms()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.KiraiLibLoader.dll");
            MemoryStream mem = new MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            Assembly.Load(mem.ToArray());

            new Action(() => KiraiLibLoader.Load())();
        }

        private Action<int, string> SendRPC;
        private Dictionary<string, RSA> encryptors = new Dictionary<string, RSA>();
        private RSA ourRSA;
        private bool halt = false;

        public override void OnApplicationStart()
        {
            if (MelonHandler.Mods.Any(m => m.Assembly.GetName().Name == "KiraiRPC"))
            {
                ourRSA = RSA.Create();

                new Action(() =>
                {
                    SendRPC = KiraiRPC.GetSendRPC("KiraiComms");
                    KiraiRPC.callbackChain += new Action<KiraiRPC.RPCData>((data) =>
                    {
                        if (data.target == "KiraiRPC")
                        {
                            if (data.id == (int)KiraiRPC.RPCEventIDs.OnInit)
                            {
                                RSAParameters rsadata = ourRSA.ExportParameters(false);
                                string exponent = Convert.ToBase64String(rsadata.Exponent);
                                string modulus = Convert.ToBase64String(rsadata.Modulus);
                                SendRPC(0x000, data.sender.Length.ToString("X").PadLeft(2, '0') + data.sender + exponent.Length.ToString("X") + exponent + modulus);
                            }
                        }
                        else if (data.target == "KiraiComms")
                        {
                            switch (data.id)
                            {
                                case 0x000:
                                case 0x001:
                                    {
                                        if (int.TryParse(data.payload.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out int len))
                                        {
                                            if (data.payload.Substring(2, len) == VRC.Core.APIUser.CurrentUser.displayName)
                                            {
                                                if (int.TryParse(data.payload.Substring(2 + len, 1), System.Globalization.NumberStyles.HexNumber, null, out int len2))
                                                {
                                                    // we recieved their rsa public
                                                    RSAParameters rsadata = new RSAParameters();
                                                    rsadata.Exponent = Convert.FromBase64String(data.payload.Substring(3 + len, len2));
                                                    rsadata.Modulus = Convert.FromBase64String(data.payload.Substring(3 + len + len2));

                                                    RSA rsa = RSA.Create();
                                                    rsa.ImportParameters(rsadata);

                                                    encryptors[data.sender] = rsa;

                                                    if (data.id == 0x000 && data.sender != VRC.Core.APIUser.CurrentUser.displayName)
                                                    {
                                                        // send our rsa public
                                                        RSAParameters rsadata2 = ourRSA.ExportParameters(false);
                                                        string exponent = Convert.ToBase64String(rsadata2.Exponent);
                                                        string modulus = Convert.ToBase64String(rsadata2.Modulus);
                                                        SendRPC(0x001, data.sender.Length.ToString("X").PadLeft(2, '0') + data.sender + exponent.Length.ToString("X") + exponent + modulus);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 0x002:
                                    {
                                        if (int.TryParse(data.payload.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out int len))
                                        {
                                            if (data.payload.Substring(2, len) == VRC.Core.APIUser.CurrentUser.displayName)
                                            {
                                                string str = System.Text.Encoding.UTF8.GetString(ourRSA.DecryptValue(Convert.FromBase64String(data.payload.Substring(2 + len)))).Replace("\0", "");

                                                KiraiLib.Logger.Log($"{data.sender}: {str}");
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                    });
                }).Invoke();

                KiraiLib.Callbacks.OnUIReload += () =>
                {
                    VRChat_OnUiManagerInit();
                };

                MelonCoroutines.Start(WaitForNetworkManager());
            }
            else
            {
                halt = true;
                MelonLogger.LogError("Didn't find KiraiRPC, stopping...");
            }
        }

        private System.Collections.IEnumerator WaitForNetworkManager()
        {
            while (NetworkManager.field_Internal_Static_NetworkManager_0 is null) yield return null;

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_ObjectPublicHa1UnT1Unique_1_Player_1
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
                    HUDInput("Message", "Send", "Enter your message...", "", new Action<string>((val) =>
                    {
                        if (val.Length <= 128)
                            SendRPC(0x002, name.Length.ToString("X").PadLeft(2, '0') + name + Convert.ToBase64String(rsa.EncryptValue(System.Text.Encoding.UTF8.GetBytes(val))));
                        else KiraiLib.Logger.Log("Messages can only be 128 characters long.");
                    }));
                } 
                else
                {
                    KiraiLib.Logger.Log($"{QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName} is not using KiraiComms");
                }
            }));
        }

        private void OnPlayerLeft(Player player)
        {
            encryptors.Remove(player.field_Private_APIUser_0.displayName);
        }

        private static void LogWithPadding(string src, bool passed)
        {
            MelonLogger.Log($"Hooking {src}...".PadRight(71, ' ') + (passed ? "Passed" : "Failed"));
        }

        public static void HUDInput(string title, string text, string placeholder, string initial, System.Action<string> OnAccept)
        {
            typeof(VRCUiPopupManager)
                .GetMethods()
                .Where(m => m.Name.Contains("Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_PDM_"))
                .First(m => UnhollowerRuntimeLib.XrefScans.XrefScanner.XrefScan(m).Where(x => x.Type == UnhollowerRuntimeLib.XrefScans.XrefType.Global).Count() == 0)
                .Invoke(VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0, new object[] {
                    title,
                    initial,
                    InputField.InputType.Standard,
                    false,
                    text,
                    UnhollowerRuntimeLib
                        .DelegateSupport
                        .ConvertDelegate<
                            Il2CppSystem.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>
                        >(
                            new System.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>(
                                (a, b, c) =>
                                {
                                    OnAccept(a);
                                }
                            )
                        ),
                    null,
                    placeholder,
                    true,
                    null
                });
        }
    }
}
