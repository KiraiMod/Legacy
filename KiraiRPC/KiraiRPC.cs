using Harmony;
using MelonLoader;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using static VRC.SDKBase.VRC_EventHandler;

[assembly: MelonInfo(typeof(KiraiMod.KiraiRPC), "KiraiRPC", null, "Kirai Chan#8315")]
[assembly: MelonGame("VRChat", "VRChat")]

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
        public static bool isSDK3;

        private static VRC_EventHandler handler;
        private object token;
        private HarmonyInstance harmony;

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

            public RPCData(string target, int id, string sender, string payload)
            {
                this.target = target;
                this.id = id;
                this.sender = sender;
                this.payload = payload;
            }
        }

        public enum RPCEventIDs
        {
            OnInit = 0x000,
        }

        public override void OnApplicationStart()
        {
            harmony = HarmonyInstance.Create("KiraiRPC");
            MelonCoroutines.Start(Initialize());
#if DEBUG
            MelonLogger.Log(System.ConsoleColor.Cyan, $"{new string('v', 26)}\n");
            MelonLogger.Log(System.ConsoleColor.Cyan, "Running a debug build");
            MelonLogger.Log(System.ConsoleColor.Cyan, "Upload logs to #crash-logs\n");
            MelonLogger.Log(System.ConsoleColor.Cyan, $"{new string('^', 26)}");
#endif
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (level != -1) return;
            handler = null;

            if (token != null) MelonCoroutines.Stop(token);

            token = MelonCoroutines.Start(WaitForLevelToLoad());
        }

        private System.Collections.IEnumerator WaitForLevelToLoad()
        {
            int sleep = 0;

            isSDK3 = Object.FindObjectOfType<VRCSDK2.VRC_SceneDescriptor>() is null;

            while ((VRCPlayer.field_Internal_Static_VRCPlayer_0 == null ||
                (handler = Object.FindObjectOfType<VRC_EventHandler>()) == null) &&
                sleep < 60)
            {
                sleep++;
                yield return new WaitForSeconds(1);
            }

            if (sleep >= 60)
            {
                yield break;
            }
            token = null;

#if DEBUG
            MelonLogger.Log($"Found Event listener after {sleep} seconds");
#endif

            SendRPC(0xD00);
        }

        private System.Collections.IEnumerator Initialize()
        {
            try
            {
                harmony.Patch(typeof(VRC_EventDispatcherRFC)
                    .GetMethod(nameof(VRC_EventDispatcherRFC.Method_Public_Void_Player_VrcEvent_VrcBroadcastType_Int32_Single_0), BindingFlags.Public | BindingFlags.Instance),
                    new HarmonyMethod(typeof(KiraiRPC).GetMethod(nameof(OnRPC), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("OnRPC", true);
            }
            catch { LogWithPadding("OnRPC", false); }

            yield break;
        }

        public static System.Action<int, string> GetSendRPC(string name)
        {
            return new System.Action<int, string>((id, payload) =>
            {
                SendRPC(id, name, payload);
            });
        }

        private static void OnRPC(ref Player __0, ref VrcEvent __1)
        {
            if (__0?.field_Private_APIUser_0 is null || __1 is null) return;

            if (__1.EventType == VrcEventType.SendRPC && __1.ParameterString == "UdonSyncRunProgramAsRPC")
            {
                ProcessRPC(__0, System.Text.Encoding.UTF8.GetString(__1.ParameterBytes).Substring(6));
            }

            if (__1.EventType == VrcEventType.ActivateCustomTrigger)
            {
                ProcessRPC(__0, __1.ParameterString);
            }
        }

        private static void ProcessRPC(Player player, string rpc)
        {
            if (rpc.Length < 1) return;
            if (rpc[0] == 'k')
            {
#if DEBUG
                MelonLogger.Log($"Recieved {rpc}");
#endif

                if (rpc.Length < 5)
                {
                    MelonLogger.Log($"{player.field_Private_APIUser_0.displayName} sent a malformed kRPC (invalid size).");
                    return;
                }

                string sid = rpc.Substring(1, 3);
                if (!int.TryParse(sid, System.Globalization.NumberStyles.HexNumber, null, out int id))
                {
                    MelonLogger.Log($"{player.field_Private_APIUser_0.displayName} sent a malformed kRPC (invalid id).");
                    return;
                }

                string slen = rpc.Substring(4, 1);
                if (!int.TryParse(slen, System.Globalization.NumberStyles.HexNumber, null, out int len))
                {
                    MelonLogger.Log($"{player.field_Private_APIUser_0.displayName} sent a malformed kRPC (invalid length).");
                    return;
                }

                string target = rpc.SafeSubstring(5, len);
                string payload = rpc.SafeSubstring(5 + len);

                if (len == 0) // intended for us
                {
                    switch (id)
                    {
                        case 0xD00:
                            if (callbackChain != null)
                                callbackChain.Invoke(new RPCData("KiraiRPC", (int)RPCEventIDs.OnInit, player.field_Private_APIUser_0.displayName, ""));
                            break;
                    }
                }

                if (callbackChain != null)
                    callbackChain.Invoke(new RPCData(len == 0 ? "KiraiRPC" : target, id, player.field_Private_APIUser_0.displayName, payload));
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

            if (isSDK3)
            {
                handler.TriggerEvent(
                new VrcEvent
                {
                    EventType = VrcEventType.SendRPC,
                    Name = "SendRPC",
                    ParameterObject = handler.gameObject,
                    ParameterInt = Player.prop_Player_0.field_Private_VRCPlayerApi_0.playerId,
                    ParameterFloat = 0f,
                    ParameterString = "UdonSyncRunProgramAsRPC",
                    ParameterBoolOp = VrcBooleanOp.Unused,
                    ParameterBytes = Networking.EncodeParameters(new Il2CppSystem.Object[] {
                        raw
                    })
                },
                VrcBroadcastType.AlwaysUnbuffered, VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject, 0f);
            }
            else
            {
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

        private static void LogWithPadding(string src, bool passed)
        {
            MelonLogger.Log($"Hooking {src}...".PadRight(73, ' ') + (passed ? "Passed" : "Failed"));
        }
    }
}
