using Harmony;
using MelonLoader;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using static VRC.SDKBase.VRC_EventHandler;

namespace KiraiMod
{
    internal static class Extensions
    {
        public static string SafeSubstring(this string value, int startIndex, int length = int.MaxValue)
        {
            return new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());
        }
    }

    public class KiraiRPC : MelonMod
    {
        public static System.Action<RPCData> callbackChain = new System.Action<RPCData>((data) => { });

        public static class Config
        {
            public static string primary = "KiraiRPC";
        }

        public class RPCData
        {
            public string target;
            public int id;
            public string sender;
            public string payload;

            public string to_be_deprecated_custom_please_dont_use;
            public bool to_be_deprecated_isCustom_please_dont_use;

            public RPCData(string target, int id, string sender, string payload)
            {
                this.target = target;
                this.id = id;
                this.sender = sender;
                this.payload = payload;

                to_be_deprecated_isCustom_please_dont_use = false;
            }

            public RPCData(string target, string custom, string sender, string payload)
            {
                this.target = target;
                this.sender = sender;
                this.payload = payload;

                to_be_deprecated_custom_please_dont_use = custom;
                to_be_deprecated_isCustom_please_dont_use = true;
            }
        }

        public enum RPCEventIDs
        {
            OnInit = 0x000,
        }

        private static VRC_EventHandler handler;

        private HarmonyInstance harmony;

        public override void OnApplicationStart()
        {
            harmony = HarmonyInstance.Create("KiraiRPC");
            MelonCoroutines.Start(Initialize());
#if DEBUG
            MelonLogger.Log(System.ConsoleColor.Cyan, $"{new string('v', 26)}\n");
            MelonLogger.Log(System.ConsoleColor.Cyan, "Running a debug build");
            MelonLogger.Log(System.ConsoleColor.Cyan, "Upload logs to #crash-logs\n");
            MelonLogger.Log(System.ConsoleColor.Cyan, $"{new string('^', 26)}\n");
#endif
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (level != -1) return;
            handler = null;
            MelonCoroutines.Start(WaitForLevelToLoad());
        }

        private System.Collections.IEnumerator WaitForLevelToLoad()
        {
            int sleep = 0;           

            while ((VRCPlayer.field_Internal_Static_VRCPlayer_0 == null || 
                (handler = Object.FindObjectsOfType<VRC_EventHandler>().FirstOrDefault()) == null) && 
                sleep < 60)
            {
                sleep++;
                yield return new WaitForSeconds(1);
            }

            if (sleep >= 60)
            {
                MelonLogger.Log("Abandoning RPC system, world is probably SDK3.");
                yield break;
            }
#if DEBUG
            MelonLogger.Log($"Found Event listener after {sleep} seconds");
#endif
            SendRPC(0xD00);
        }

        private System.Collections.IEnumerator Initialize()
        {
            while (NetworkManager.field_Internal_Static_NetworkManager_0 is null) yield return null;

            try
            {
                harmony.Patch(typeof(VRC_EventDispatcherRFC)
                    .GetMethod(nameof(VRC_EventDispatcherRFC.Method_Public_Void_Player_VrcEvent_VrcBroadcastType_Int32_Single_0), BindingFlags.Public | BindingFlags.Instance),
                    new HarmonyMethod(typeof(KiraiRPC).GetMethod(nameof(OnRPC), BindingFlags.NonPublic | BindingFlags.Static)));

                MelonLogger.Log("Hooking RPCs... Passed");
            }
            catch { MelonLogger.LogWarning("Hooking RPCs... Failed"); }
        }

        public static System.Action<int, string> GetSendRPC(string name)
        {
            return new System.Action<int, string>((id, payload) =>
            {
                SendRPC(id, name, payload);
            });
        }

        private static void OnRPC(ref Player __0, ref VrcEvent __1, ref VrcBroadcastType __2)
        {
            //if (__0?.field_Private_VRCPlayerApi_0?.isLocal ?? true) return;

            if (__1?.EventType == VrcEventType.ActivateCustomTrigger)
            {
                if (__1.ParameterString.Length < 1) return;
                if (__1.ParameterString[0] == 'k')
                {
#if DEBUG
                    MelonLogger.Log($"Recieved {__1.ParameterString}");
#endif

                    if (__1.ParameterString.Length < 5)
                    {
                        MelonLogger.Log($"{__0.field_Private_APIUser_0.displayName} sent a malformed kRPC (invalid size).");
                        return;
                    }

                    string sid = __1.ParameterString.Substring(1, 3);
                    if (!int.TryParse(sid, System.Globalization.NumberStyles.HexNumber, null, out int id)) {
                        MelonLogger.Log($"{__0.field_Private_APIUser_0.displayName} sent a malformed kRPC (invalid id).");
                        return;
                    }

                    string slen = __1.ParameterString.Substring(4, 1);
                    if (!int.TryParse(slen, System.Globalization.NumberStyles.HexNumber, null, out int len))
                    {
                        MelonLogger.Log($"{__0.field_Private_APIUser_0.displayName} sent a malformed kRPC (invalid length).");
                        return;
                    }

                    string target = __1.ParameterString.SafeSubstring(5, len);
                    string payload = __1.ParameterString.SafeSubstring(5 + len);

                    if (len == 0) // intended for us
                    {
                        switch (id)
                        {
                            case 0xD00:
                                if (callbackChain != null)
                                    callbackChain.Invoke(new RPCData("KiraiRPC", (int)RPCEventIDs.OnInit, __0.field_Private_APIUser_0.displayName, ""));

                                SendRPC(0xA00, __0.field_Private_APIUser_0.displayName);
                                break;
                            case 0xA00:
                            case 0xA01:
                                if (payload == Player.prop_Player_0.field_Private_APIUser_0.displayName)
                                {
                                    MelonLogger.Log($"{__0.field_Private_APIUser_0.displayName} is using the RPC system.");

                                    SendRPC(0xC00, // tell them what we are using
                                        __0.field_Private_APIUser_0.displayName.Length.ToString().PadLeft(2, '0') +
                                        __0.field_Private_APIUser_0.displayName + Config.primary);
                                    if (id == 0) SendRPC(0xA01, __0.field_Private_APIUser_0.displayName); // ask them what they are using
                                }
                                break;
                            case 0xC00:
                                if (!int.TryParse(payload.Substring(0, 2), out int length)) return;

                                if (payload.Substring(2, length) == Player.prop_Player_0.field_Private_APIUser_0.displayName && callbackChain != null) // we are the intended recipient
                                    callbackChain.Invoke(new RPCData("KiraiRPC", "PlayerUsingMod", __0.field_Private_APIUser_0.displayName, payload.Substring(2 + length)));
                                
                                break;
                        }
                    }

                    if (callbackChain != null) 
                        callbackChain.Invoke(new RPCData(len == 0 ? "KiraiRPC" : target, id,__0.field_Private_APIUser_0.displayName, payload));
                }
            }
        }

        private static void SendRPC(int id)
        {
            SendRPC(id, "");
        }

        private static bool SendRPC(int id, string payload)
        {
            string sid = id.ToString("X");
            if (sid.Length > 3) return false;

            SendRPC("k" + sid.PadLeft(3, '0') + "0" + payload);

            return true;
        }

        private static bool SendRPC(int id, string sender, string payload)
        {
            string sid = id.ToString("X");
            if (sid.Length > 3) return false;

            SendRPC("k" + sid.PadLeft(3, '0') + sender.Length.ToString("X") + sender + payload);

            return true;
        }

        private static bool SendRPC(string protocol, string id, string sender, string payload)
        {
            if (protocol.Length != 1) return false;
            if (id.Length != 2) return false;

            SendRPC("k" + protocol + id + sender.Length + sender + payload);

            return true;
        }

        public static void SendRPC(string raw)
        {
#if DEBUG
            MelonLogger.Log($"Sending {raw}");
#endif

            if (handler == null)
            {
                MelonLogger.Log("Canceling RPC because handler is null.");
                return;
            }

            handler.TriggerEvent(
            new VrcEvent
            {
                EventType = VrcEventType.ActivateCustomTrigger,
                Name = "",
                ParameterObject = handler.gameObject,
                ParameterInt = 0,
                ParameterFloat = 0f,
                ParameterString = raw,
                ParameterBoolOp = VrcBooleanOp.Unused,
                ParameterBytes = new Il2CppStructArray<byte>(0L)
            },
            VrcBroadcastType.AlwaysUnbuffered, VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject, 0f);
        }
    }
}
