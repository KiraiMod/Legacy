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
    public class KiraiRPC : MelonMod
    {
        public static System.Action<string, string[]> callback = new System.Action<string, string[]>((a,b)=> {});

        public static class Config
        {
            public static string modName = "KiraiRPC";
        }
        
        private static VRC_EventHandler handler;

        private HarmonyInstance harmony;

        public override void OnApplicationStart()
        {
            harmony = HarmonyInstance.Create("KiraiRPC");
            MelonCoroutines.Start(Initialize());
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
            MelonCoroutines.Start(WaitForLevelToLoad());
        }

        private System.Collections.IEnumerator WaitForLevelToLoad()
        {
            int sleep = 0;

            while ((handler = Resources.FindObjectsOfTypeAll<VRC_EventHandler>().FirstOrDefault()) == null && sleep < 12)
            {
                sleep++;
                yield return new WaitForSeconds(5);
            }

            if (sleep >= 12) yield break;

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

        private static void OnRPC(ref Player __0, ref VrcEvent __1, ref VrcBroadcastType __2)
        {
            if (__0.field_Private_VRCPlayerApi_0.isLocal) return;

            if (__1.EventType == VrcEventType.ActivateCustomTrigger)
            {
                if (__1.ParameterString.Length < 1) return;
                if (__1.ParameterString[0] == 'k')
                {
                    if (__1.ParameterString.Length < 4)
                    {
                        MelonLogger.Log($"{__0.field_Private_APIUser_0.displayName} sent a malformed kRPC.");
                        return;
                    }

                    SendType protocol = (SendType)System.Enum.Parse(typeof(_SendType), __1.ParameterString[1].ToString());
                    string id = __1.ParameterString.Substring(2, 2);
                    string payload = __1.ParameterString.Substring(4);

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
            }
        }

        private static void OnGet(string id, string payload, Player player)
        {
            switch (id)
            {
                case "00":
                    SendRPC(SendType.Post, "00", 
                        player.field_Private_APIUser_0.displayName.Length.ToString().PadLeft(2, '0') + 
                        player.field_Private_APIUser_0.displayName + Config.modName);
                    break;
            }
        }

        private static void OnSet(string id, string payload, Player player)
        {

        }

        private static void OnPost(string id, string payload, Player player)
        {
            switch (id)
            {
                case "00":
                    if (!int.TryParse(payload.Substring(0, 2), out int length)) return;

                    MelonLogger.Log($"{payload.Substring(2, length)} is using ${payload.Substring(2 + length)}");
                    callback.Invoke("PlayerUsingMod", new string[] { payload.Substring(2, length), payload.Substring(2 + length)});
                    break;
            }
        }

        private static void OnBroadcast(string id, string payload, Player player)
        {
            switch (id)
            {
                case "00": // Who is using KiraiRPC
                    SendRPC(SendType.Broadcast, "01", Player.prop_Player_0.field_Private_APIUser_0.displayName); // i am using krpc
                    break;
                case "01": // I am using KiraiRPC!
                    MelonLogger.Log($"{player.field_Private_APIUser_0.displayName} is using the RPC system.");
                    if (payload == Player.prop_Player_0.field_Private_APIUser_0.displayName) // we are the person who requested
                        SendRPC(SendType.Get, "00", player.field_Private_APIUser_0.displayName); // ask them what they are using
                    break;
            }
        }

        private static void OnUpgrade(string id, string payload, Player player)
        {

        }

        public static void SendRPC(SendType protocol, string id)
        {
            SendRPC(protocol, id, "");
        }

        public static void SendRPC(SendType protocol, string id, string payload)
        {
            if (id.Length != 2)
            {
                MelonLogger.Log($"RPC {id} is invalid length");
                return;
            }

            SendRPC("k" + System.Enum.GetName(typeof(_SendType), protocol) + id + payload);
        }

        public static void SendRPC(string raw)
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
            VrcBroadcastType.AlwaysUnbuffered, VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject, 0f);;
        }

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
