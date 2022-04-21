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
        public static System.Action<string, string, string[]> callbackChain = new System.Action<string, string, string[]>((target, type, data) => { });

        public static class Config
        {
            public static string primary = "KiraiRPC";
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

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                OnLevelWasLoaded(0);
            }
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
            SendRPC(SendType.Broadcast, "00");
        }

        private System.Collections.IEnumerator Initialize()
        {
            while (ReferenceEquals(NetworkManager.field_Internal_Static_NetworkManager_0, null)) yield return null;

            try
            {
                harmony.Patch(typeof(VRC_EventDispatcherRFC)
                    .GetMethod(nameof(VRC_EventDispatcherRFC.Method_Public_Void_Player_VrcEvent_VrcBroadcastType_Int32_Single_0), BindingFlags.Public | BindingFlags.Instance),
                    new HarmonyMethod(typeof(KiraiRPC).GetMethod(nameof(OnRPC), BindingFlags.NonPublic | BindingFlags.Static)));

                MelonLogger.Log("Hooking RPCs... Passed");
            }
            catch { MelonLogger.LogWarning("Hooking RPCs... Failed"); }
        }

        public static System.Action<SendType, string, string> GetCallback(string name)
        {
            return new System.Action<SendType, string, string>((type, id, payload) =>
            {
                SendRPC(type, id, name, payload);
            });
        }

        private static void OnRPC(ref Player __0, ref VrcEvent __1, ref VrcBroadcastType __2)
        {
            if (__0?.field_Private_VRCPlayerApi_0?.isLocal ?? true) return;

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

                    string sprotocol = __1.ParameterString[1].ToString();

                    if (!System.Enum.TryParse(sprotocol, out _SendType _protocol))
                    {
                        MelonLogger.Log($"{__0.field_Private_APIUser_0.displayName} sent a malformed kRPC (invalid type).");
                        return;
                    }

                    SendType protocol = (SendType)_protocol;

                    string sid = __1.ParameterString.Substring(2, 2);
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
                        switch (protocol)
                        {
                            case SendType.Get:
                                OnGet(id, payload, __0);
                                break;

                            case SendType.Set:
                                OnSet(id, payload, __0);
                                break;

                            case SendType.Post:
                                OnPost(id, payload, __0);
                                break;

                            case SendType.Broadcast:
                                OnBroadcast(id, payload, __0);
                                break;

                            case SendType.Upgrade:
                                OnUpgrade(id, payload, __0);
                                break;
                        }
                    }

                    if (callbackChain != null) callbackChain.Invoke(len == 0 ? "KiraiRPC" : target, sprotocol + sid, new string[] { __0.field_Private_APIUser_0.displayName, payload });
                }
            }
        }

        private static void OnGet(int id, string payload, Player player)
        {
            switch (id)
            {
                case 0:
                case 1:
                    if (payload == Player.prop_Player_0.field_Private_APIUser_0.displayName)
                    {
                        MelonLogger.Log($"{player.field_Private_APIUser_0.displayName} is using the RPC system.");

                        SendRPC(SendType.Post, "00", // tell them what we are using
                            player.field_Private_APIUser_0.displayName.Length.ToString().PadLeft(2, '0') +
                            player.field_Private_APIUser_0.displayName + Config.primary);
                        if (id == 0) SendRPC(SendType.Get, "01", player.field_Private_APIUser_0.displayName); // ask them what they are using
                    }
                    break;

            }
        }

        private static void OnSet(int id, string payload, Player player)
        {

        }

        private static void OnPost(int id, string payload, Player player)
        {
            switch (id)
            {
                case 0:
                    if (!int.TryParse(payload.Substring(0, 2), out int length)) return;

                    if (payload.Substring(2, length) == Player.prop_Player_0.field_Private_APIUser_0.displayName) // we are the intended recipient
                        callbackChain.Invoke("", "PlayerUsingMod", new string[] { player.field_Private_APIUser_0.displayName, payload.Substring(2 + length) });
                    

                    break;
            }
        }

        private static void OnBroadcast(int id, string payload, Player player)
        {
            switch (id)
            {
                case 0: // Who is using KiraiRPC
                    SendRPC(SendType.Get, "00", player.field_Private_APIUser_0.displayName); // ask them what they are using
                    break;
            }
        }

        private static void OnUpgrade(int id, string payload, Player player)
        {

        }

        private static void SendRPC(SendType protocol, string id)
        {
            SendRPC(protocol, id, "");
        }

        private static bool SendRPC(SendType protocol, string id, string payload)
        {
            if (id.Length != 2) return false;

            SendRPC("k" + System.Enum.GetName(typeof(_SendType), protocol) + id + "0" + payload);

            return true;
        }

        private static bool SendRPC(SendType protocol, string id, string sender, string payload)
        {
            if (id.Length != 2) return false;

            SendRPC("k" + System.Enum.GetName(typeof(_SendType), protocol) + id + sender.Length.ToString("X") + sender + payload);

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

        /// <summary>
        /// Get       = A,
        /// Set       = B,
        /// Post      = C,
        /// Broadcast = D,
        /// Upgrade   = E
        /// </summary>
        public enum SendType
        {
            Get,
            Set,
            Post,
            Broadcast,
            Upgrade
        }

        enum _SendType
        {
            A,
            B,
            C,
            D,
            E
        }

        // A - Get
        // B - Set
        // C - Post
        // D - Broadcast
        // E - Upgrade (ignore)

        // Broadcast 00: Anybody here?
        // Broadcast 01: I'm here.
        // Get 00: What mod are you using?
        // Set 00: I am using ModName
        // Get 01: What is your supported version?
        // Set 01: My supported version is x.x.x
    }
}
