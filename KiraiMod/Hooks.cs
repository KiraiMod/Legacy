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

        private static IEnumerator Initialize()
        {
            try
            {
                Shared.harmony.Patch(typeof(Cursor).GetProperty(nameof(Cursor.lockState)).GetSetMethod(),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(FuckOffPayToCheatThisIsMyFeatureNotYours), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("Cursor.LockState Setter", true);
            }
            catch { LogWithPadding("Cursor.LockState Setter", false); }

            try
            {
                Shared.harmony.Patch(typeof(Cursor).GetProperty(nameof(Cursor.visible)).GetSetMethod(),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(FocusVisible), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("Cursor.Visible Setter", true);
            }
            catch { LogWithPadding("Cursor.Visible Setter", false); }

            try
            {
                Shared.harmony.Patch(typeof(System.Diagnostics.Process)
                    .GetMethods().FirstOrDefault(m => m.Name == nameof(System.Diagnostics.Process.GetProcesses) && m.GetParameters().Length == 0),
                    null, new HarmonyMethod(typeof(Hooks).GetMethod(nameof(GetProcesses), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("Process.GetProcesses", true);
            }
            catch { LogWithPadding("Process.GetProcesses", false); }

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

            var QMMethodsByCC = typeof(QuickMenu).GetMethods()
                .Where(m => m.Name.Contains("Method_Public_Void_") && m.Name.Length == 20)
                .OrderByDescending(m => (Attribute.GetCustomAttribute(m, typeof(UnhollowerBaseLib.Attributes.CallerCountAttribute)) as UnhollowerBaseLib.Attributes.CallerCountAttribute).Count);

            var QMMethodsByXR = QMMethodsByCC.Skip(2).OrderByDescending(m => UnhollowerRuntimeLib.XrefScans.XrefScanner.XrefScan(m).Count());

            try
            {
                Shared.harmony.Patch(QMMethodsByXR.ElementAt(0),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnMenuOpened), BindingFlags.NonPublic | BindingFlags.Static)));

                //Shared.harmony.Patch(QMMethodsByXR.ElementAt(1),
                //    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnMenuOpened), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("OnMenuOpened", true);
            }
            catch { LogWithPadding("OnMenuOpened", false); }

            try
            {
                Shared.harmony.Patch(QMMethodsByCC.ElementAt(1),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnMenuClosed), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("OnMenuClosed", true);
            }
            catch { LogWithPadding("OnMenuClosed", false); }

            try
            {
                Shared.harmony.Patch(typeof(PortalInternal)
                    .GetMethod(nameof(PortalInternal.ConfigurePortal), BindingFlags.Public | BindingFlags.Instance), null,
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(OnPortalConfigured), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("OnPortalConfigured", true);
            }
            catch { LogWithPadding("OnPortalConfigured", false); }

            try
            {
                Shared.harmony.Patch(typeof(HighlightsFX)
                    .GetMethod(nameof(HighlightsFX.Method_Public_Void_Renderer_Boolean_0), BindingFlags.Public | BindingFlags.Instance),
                    new HarmonyMethod(typeof(Hooks).GetMethod(nameof(HighlightRenderer), BindingFlags.NonPublic | BindingFlags.Static)));

                LogWithPadding("HighlightRenderer", true);
            }
            catch { LogWithPadding("HighlightRenderer", false); }

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
        }

        private static void GetProcesses(ref System.Diagnostics.Process[] __result)
        {
            __result = new System.Diagnostics.Process[0];
    }

        private static bool FuckOffPayToCheatThisIsMyFeatureNotYours(bool __0) 
        {
            return !__0 || Application.isFocused;
        }

        private static bool FocusVisible(bool __0)
        {
            return __0 || Application.isFocused;
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

        private static void Test5()
        {
            MelonLogger.Log("Test5 called");
        }
#endif

        private static void OnPlayerJoined(Player player)
        {
            MelonLogger.Log(player.field_Private_VRCPlayerApi_0.displayName + " joined");
            Shared.modules.OnPlayerJoined(player);

            if (player.IsMod()) KiraiLib.Logger.Log("A moderator is in your lobby.");
        }

        private static void OnPlayerLeft(Player player)
        {
            MelonLogger.Log(player.field_Private_VRCPlayerApi_0.displayName + " left");

            Shared.modules.OnPlayerLeft(player);
            
            if (Shared.TargetPlayer == player)
                Shared.TargetPlayer = null;
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
            bool continueExecuting = !Shared.modules.misc.bPersistantQuickMenu ||
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

        private static void OnPortalConfigured(GameObject __instance)
        {
            foreach (PortalInternal portal in UnityEngine.Object.FindObjectsOfType<PortalInternal>())
            {
                if (portal.name == __instance.name)
                {
                    if (Mathf.Abs(portal.transform.position.x) > 10000 ||
                        Mathf.Abs(portal.transform.position.y) > 10000 ||
                        Mathf.Abs(portal.transform.position.z) > 10000)
                    {
                        KiraiLib.Logger.Log($"Blocked invalid portal from {VRC.SDKBase.Networking.GetOwner(portal.gameObject).displayName}");
                        portal.gameObject.active = false;
                    }
                    else if (!Shared.unloaded)
                    {
                        VRC.SDKBase.Networking.SetOwner(VRC.SDKBase.Networking.LocalPlayer, portal.gameObject);
                        MelonCoroutines.Start(Helper.ReversePortal(portal));
                    }

                    break;
                }
            }
        }

        private static bool HighlightRenderer(Renderer __0, bool __1)
        {
            if (__0 is null) return false;

            return !Shared.modules.esp.state || __1 || __0.name != "SelectRegion";
        }

        private static void LogWithPadding(string src, bool passed)
        {
            MelonLogger.Log($"Hooking {src}...".PadRight(73, ' ') + (passed ? "Passed" : "Failed"));
        }
    }
}
