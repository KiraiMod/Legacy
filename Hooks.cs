using Harmony;
using MelonLoader;
using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using static KiraiMod.Modules.ModLog;
using static VRC.SDKBase.VRC_EventHandler;

namespace KiraiMod
{
    public class Hooks
    {
        public Hooks()
        {
            MelonCoroutines.Start(Initialize());
        }

        private IEnumerator Initialize()
        {
            while (ReferenceEquals(NetworkManager.field_Internal_Static_NetworkManager_0, null)) yield return null;

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_ObjectPublicHa1UnT1Unique_1_Player_0
                    .field_Private_HashSet_1_UnityAction_1_T_0
                    .Add(new Action<Player>(player => OnPlayerJoined(player)));
                MelonModLogger.Log("Hooking OnPlayerJoined... Passed");
            }
            catch { MelonModLogger.LogWarning("Hooking OnPlayerJoined... Failed"); }

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_ObjectPublicHa1UnT1Unique_1_Player_1
                    .field_Private_HashSet_1_UnityAction_1_T_0
                    .Add(new Action<Player>(player => OnPlayerLeft(player)));
                MelonModLogger.Log("Hooking OnPlayerLeft... Passed");
            }
            catch { MelonModLogger.LogWarning("Hooking OnPlayerLeft... Failed"); }

            try
            {
                Shared.harmony.Patch(typeof(VRC_EventDispatcherRFC)
                    .GetMethod(nameof(VRC_EventDispatcherRFC.Method_Public_Void_Player_VrcEvent_VrcBroadcastType_Int32_Single_0), BindingFlags.Public | BindingFlags.Instance), 
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnRPC), BindingFlags.NonPublic | BindingFlags.Static)));

                MelonModLogger.Log("Hooking RPCs... Passed");
            }
            catch { MelonModLogger.LogWarning("Hooking RPCs... Failed"); }

            try
            {
                Shared.harmony.Patch(typeof(VRCAvatarManager)
                    .GetMethod(nameof(VRCAvatarManager.Method_Private_Boolean_GameObject_String_Single_0), BindingFlags.Instance | BindingFlags.Public), 
                    null, new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnAvatarInitialized), BindingFlags.NonPublic | BindingFlags.Static)));

                MelonModLogger.Log("Hooking OnAvatarInitialized... Passed");
            } catch { MelonModLogger.Log("Hooking OnAvatarInitialized... Failed"); }

            if (Shared.modules.aliases.state)
                try
                {
                    Shared.harmony.Patch(typeof(Text).GetProperty("text").GetGetMethod(),
                        null, new HarmonyMethod(typeof(Modules.Aliases).GetMethod(nameof(Modules.Aliases.ProcessString), BindingFlags.Public | BindingFlags.Static)));

                    MelonModLogger.Log("Hooking UnityEngine.UI.Text Getter ... Passed");
                }
                catch { MelonModLogger.Log("Hooking UnityEngine.UI.Text Getter ... Failed"); }
            else MelonModLogger.Log("Not hooking UnityEngine.UI.Text because Aliases is off.");
        }

        private void OnPlayerJoined(Player player)
        {
            MelonModLogger.Log(player.field_Private_VRCPlayerApi_0.displayName + " joined");
            Shared.modules.OnPlayerJoined(player);

            if (player.IsMod()) Utils.HUDMessage("A moderator is in your lobby.");
        }

        private void OnPlayerLeft(Player player)
        {
            MelonModLogger.Log(player.field_Private_VRCPlayerApi_0.displayName + " left");
            Shared.modules.OnPlayerLeft(player);
        }

        private static void OnAvatarInitialized(GameObject __0, ref VRCAvatarManager __instance)
        {
            if (__instance?.field_Private_VRCPlayer_0?.field_Private_Player_0?.field_Private_APIUser_0 == null) return;

            Shared.modules.OnAvatarInitialized(__0, __instance);
        }

        private static void OnRPC(ref Player __0, ref VrcEvent __1, ref VrcBroadcastType __2, ref int __3, ref float __4)
        {
            if (Shared.Options.bWorldTriggers && __2 == VrcBroadcastType.Local) __2 = VrcBroadcastType.AlwaysUnbuffered;

            switch (__1.EventType)
            {
                case VrcEventType.SendRPC:
                    if (__1.ParameterObject.name == "ModerationManager")
                    {
                        string param = "";
                        foreach (byte b in __1.ParameterBytes)
                        {
                            param += (char)b;
                        }
                        string[] strs = param.Split('\0');
                        string uid = Regex.Replace(strs[2], "[^a-zA-Z0-9_.-]+", "", RegexOptions.Compiled);

                        if (__0.IsLocal() || Utils.GetPlayer(uid).IsLocal()) MelonCoroutines.Start(Shared.modules.mute.DelayedRefresh());
                        else Shared.modules.mute.Refresh();

                        switch (__1.ParameterString)
                        {
                            case "BlockStateChangeRPC":
                                Shared.modules.modlog.Notify(__0.field_Private_APIUser_0, Utils.GetPlayer(uid).field_Private_APIUser_0,
                                    strs.Length == 3 ? ModerationAction.Blocked : ModerationAction.Unblocked);
                                break;

                            case "MuteChangeRPC":
                                Shared.modules.modlog.Notify(__0.field_Private_APIUser_0, Utils.GetPlayer(uid).field_Private_APIUser_0,
                                    strs.Length == 3 ? ModerationAction.Muted : ModerationAction.Unmuted);
                                break;

                            case "ShowUserAvatarChangedRPC":
                                Shared.modules.modlog.Notify(__0.field_Private_APIUser_0, Utils.GetPlayer(uid).field_Private_APIUser_0,
                                    strs.Length == 3 ? ModerationAction.Shown : ModerationAction.Hide);
                                break;

                            case "ResetShowUserAvatarToDefaultRPC":
                                Shared.modules.modlog.Notify(__0.field_Private_APIUser_0, Utils.GetPlayer(uid).field_Private_APIUser_0,
                                    ModerationAction.Reset);
                                break;

                            default:
                                MelonModLogger.Log("Unknown moderation message " + __1.ParameterString);
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
