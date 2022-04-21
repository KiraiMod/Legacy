using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool loading = true;
        private Action<int, string> SendRPC;
        private Dictionary<string, RSA> encryptors = new Dictionary<string, RSA>();
        private RSA ourRSA;

        private Menu menu;
        private Menu.Button button;

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

                                                HUDMessage($"{data.sender}: {str}");
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                    });
                }).Invoke();

                MelonCoroutines.Start(WaitForNetworkManager());
            }
            else
            {
                loading = false;
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

        public override void OnUpdate()
        {
            if (loading)
            {
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    UnityEngine.Object.Destroy(button.self);
                    button = null;
                }

                if (Input.GetKeyDown(KeyCode.Insert))
                {
                    if (button != null)
                    {
                        UnityEngine.Object.Destroy(button.self);
                        button = null;
                    }

                    VRChat_OnUiManagerInit();
                }
            }
        }

        public override void VRChat_OnUiManagerInit()
        {
            menu = new Menu();

            button = new Menu.Button("Message", "Send a message to this user", -1, -3, menu.um.transform, new Action(() =>
            {
                string name = QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName;
                if (encryptors.TryGetValue(name, out RSA rsa)) {
                    HUDInput("Message", "Send", "Enter your message...", "", new Action<string>((val) =>
                    {
                        if (val.Length <= 128)
                            SendRPC(0x002, name.Length.ToString("X").PadLeft(2, '0') + name + Convert.ToBase64String(rsa.EncryptValue(System.Text.Encoding.UTF8.GetBytes(val))));
                        else HUDMessage("Messages can only be 128 characters long.");
                    }));
                } 
                else
                {
                    HUDMessage($"{QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName} is not using KiraiComms");
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

        public static void HUDMessage(string message)
        {
            if (VRCUiManager.prop_VRCUiManager_0 == null) return;

            VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_0(message);
        }

        public static void HUDInput(string title, string text, string placeholder, string initial, System.Action<string> OnAccept)
        {
            VRCUiPopupManager
                .field_Private_Static_VRCUiPopupManager_0
                .Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_PDM_1
                (
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
                );
        }
    }
}
