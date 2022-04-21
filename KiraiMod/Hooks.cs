using Harmony;
using MelonLoader;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using VRC;
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
            while (NetworkManager.field_Internal_Static_NetworkManager_0 is null) yield return null;

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_ObjectPublicHa1UnT1Unique_1_Player_0
                    .field_Private_HashSet_1_UnityAction_1_T_0
                    .Add(new Action<Player>(player => OnPlayerJoined(player)));
                LogWithPadding("OnPlayerJoined", true);
            }
            catch { LogWithPadding("OnPlayerJoined", false); }

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

            try
            {
                Shared.harmony.Patch(typeof(VRC_EventDispatcherRFC)
                    .GetMethod(nameof(VRC_EventDispatcherRFC.Method_Public_Void_Player_VrcEvent_VrcBroadcastType_Int32_Single_0), BindingFlags.Public | BindingFlags.Instance),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnRPC), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("RPCs", true);
            }
            catch { LogWithPadding("RPCs", false); }

            //try
            //{
            //    Shared.harmony.Patch(typeof(ObjectPublicIPhotonPeerListenerObStBoStObCoDiBo2ObUnique)
            //        .GetMethod(nameof(ObjectPublicIPhotonPeerListenerObStBoStObCoDiBo2ObUnique
            //        .Method_Public_Virtual_New_Boolean_Byte_Object_ObjectPublicObByObInByObObUnique_SendOptions_0),
            //        BindingFlags.Public | BindingFlags.Instance),
            //        new HarmonyMethod(typeof(Hooks).GetMethod(nameof(SendOperationPrefix), BindingFlags.NonPublic | BindingFlags.Static)));

            //    MelonLogger.Log("Hooking SendOperationPrefix... Passed");
            //}
            //catch { MelonLogger.LogWarning("Hooking SendOperationPrefix... Failed"); }

            //try
            //{
            //    Shared.harmony.Patch(typeof(ObjectPublicIPhotonPeerListenerObStBoStObCoDiBo2ObUnique)
            //        .GetMethod(nameof(ObjectPublicIPhotonPeerListenerObStBoStObCoDiBo2ObUnique.OnEvent), BindingFlags.Public | BindingFlags.Instance),
            //        new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnEvent), BindingFlags.NonPublic | BindingFlags.Static)));

            //    MelonLogger.Log("Hooking OnEvent... Passed");
            //}
            //catch { MelonLogger.LogWarning("Hooking OnEvent... Failed"); }

            try
            {
                Shared.harmony.Patch(typeof(VRCAvatarManager)
                    .GetMethod(nameof(VRCAvatarManager.Method_Private_Boolean_GameObject_PDM_0), BindingFlags.Instance | BindingFlags.Public),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnAvatarInitialized), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("OnAvatarInitialized", true);
            }
            catch { LogWithPadding("OnAvatarInitialized", false); }

            if (Shared.modules.aliases.state)
            {
                try
                {
                    Shared.harmony.Patch(typeof(Text).GetProperty(nameof(Text.text)).GetGetMethod(),
                        null, new HarmonyMethod(typeof(Modules.Aliases).GetMethod(nameof(Modules.Aliases.ProcessString), BindingFlags.Public | BindingFlags.Static)));

                    LogWithPadding("UnityEngine.UI.Text Getter", true);
                }
                catch { LogWithPadding("UnityEngine.UI.Text Getter", false); }

                try
                {
                    Shared.harmony.Patch(typeof(TMPro.TMP_Text).GetProperty(nameof(TMPro.TMP_Text.text)).GetSetMethod(),
                        new HarmonyMethod(typeof(Modules.Aliases).GetMethod(nameof(Modules.Aliases.ProcessStringPrefix), BindingFlags.Public | BindingFlags.Static)));

                    LogWithPadding("TMPro.TMP_Text Setter", true);
                }
                catch { LogWithPadding("TMPro.TMP_Text Setter", false); }
            }
            else
            {
                MelonLogger.Log("Not hooking UnityEngine.UI.Text because Aliases is off.");
                MelonLogger.Log("Not hooking TMPro.TMP_Text because Aliases is off.");
            }

            try
            {
                Shared.harmony.Patch(typeof(QuickMenu)
                    .GetMethod(nameof(QuickMenu.Method_Public_Void_0), BindingFlags.Public | BindingFlags.Instance),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnMenuOpened), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("OnMenuOpened", true);
            }
            catch { LogWithPadding("OnMenuOpened", false); }

            try
            {
                Shared.harmony.Patch(typeof(QuickMenu)
                    .GetMethod(nameof(QuickMenu.Method_Public_Void_4), BindingFlags.Public | BindingFlags.Instance),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnMenuClosed), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("OnMenuClosed", true);
            }
            catch { LogWithPadding("OnMenuClosed", false); }
        }

#if DEBUG
        private static void Test1()
        {
            MelonLogger.Log("Test1 called");
        }

        private static void Test2()
        {
            MelonLogger.Log("Test2 called");
        }

        private static void Test3()
        {
            MelonLogger.Log("Test3 called");
        }

        private static void Test4()
        {
            MelonLogger.Log("Test4 called");
        }
#endif

        private void OnPlayerJoined(Player player)
        {
            MelonLogger.Log(player.field_Private_VRCPlayerApi_0.displayName + " joined");
            Shared.modules.OnPlayerJoined(player);

            if (player.IsMod()) Utils.HUDMessage("A moderator is in your lobby.");
        }

        private void OnPlayerLeft(Player player)
        {
            MelonLogger.Log(player.field_Private_VRCPlayerApi_0.displayName + " left");

            if (Shared.targetPlayer == player)
                Shared.targetPlayer = null;
            
            Shared.modules.OnPlayerLeft(player);
        }

        private static void OnAvatarInitialized(GameObject __0, ref VRCAvatarManager __instance)
        {
            if (__instance?.field_Private_VRCPlayer_0?.field_Private_Player_0?.field_Private_APIUser_0 == null || __0 == null) return;

            Shared.modules.OnAvatarInitialized(__0, __instance);
        }

        private static void OnRPC(ref Player __0, ref VrcEvent __1, ref VrcBroadcastType __2)
        {
            if (Shared.Options.bWorldTriggers && __2 == VrcBroadcastType.Local) __2 = VrcBroadcastType.AlwaysUnbuffered;
        }

        //private static void SendOperationPrefix(ref byte __0, ref Il2CppSystem.Object __1, ref ObjectPublicObByObInByObObUnique __2, ref ExitGames.Client.Photon.SendOptions __3)
        //{
        //}

        //private static void OnEvent(ref ExitGames.Client.Photon.EventData __0)
        //{
        //    if (__0 == null) return;

        //    if (__0.Code == 33)
        //    {

        //    }
        //}

        private static void OnMenuOpened()
        {
#if DEBUG
            MelonLogger.Log("Menu Opened");
#endif
            if (Shared.modules.playerlist.state)
            {
                Shared.modules.playerlist.parent.active = true;
            }
        }

        private static bool OnMenuClosed()
        {
            bool continueExecuting = !Config.General.bPersistantQuickMenu ||
                VRCUiManager.prop_VRCUiManager_0.prop_Boolean_0 ||
                Input.GetKey(KeyCode.Escape) ||
                Input.GetButton("Oculus_CrossPlatform_Button2") ||
                Input.GetButton("Oculus_CrossPlatform_Button4");

            if (continueExecuting) { 
                if (Shared.modules.playerlist.state)
                    Shared.modules.playerlist.parent.active = false;
#if DEBUG
                MelonLogger.Log("Menu Closing");
#endif
            } 

            return continueExecuting;
        }

        private static void LogWithPadding(string src, bool passed)
        {
            MelonLogger.Log($"Hooking {src}...".PadRight(73, ' ') + (passed ? "Passed" : "Failed"));
        }
    }
}
