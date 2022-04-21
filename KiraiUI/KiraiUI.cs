using MelonLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod
{
    public class KiraiUI : MelonMod
    {
        public static KiraiUI instance;

        public bool hasStored = false;
        public bool disableQM = false;
        public bool hasLoaded = false;
        public bool isApplied = false;

        public static class Config
        {
            public static Color primary = new Color(0.34f, 0f, 0.65f);
            public static Color primary2 = new Color(0.34f, 0f, 0.65f, 0.8f);
            public static Color highlight = new Color(0.8f, 0.8f, 1f);
            public static Color background = new Color(0, 0, 0);
            public static bool extended = false;
        }

        Transform ui;
        Transform screen;
        Transform hud;
        Transform am;
        Text earlyAccess;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            instance = this;
        }

        public override void VRChat_OnUiManagerInit()
        {
            hasLoaded = true;

            ui = GameObject.Find("UserInterface").transform;
            if (ui == null) MelonLogger.LogWarning("Didn't find UserInterface");

            screen = ui.Find("MenuContent");
            if (screen == null) MelonLogger.LogWarning("Didn't find MenuContent");

            hud = ui.Find("UnscaledUI/HudContent/Hud");
            if (hud == null) MelonLogger.LogWarning("Didn't find Hud");

            earlyAccess = ui.Find("QuickMenu/ShortcutMenu/EarlyAccessText")?.GetComponent<Text>();
            if (earlyAccess == null) MelonLogger.Log("Didn't find EarlyAccessText");

            am = ui.Find("ActionMenu");
            if (am is null) MelonLogger.Log("Didn't find ActionMenu");

            if (!MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiMod")))
            {
                // we have no dom so we will do it ourselves -\_(._.)_/-
                MelonLogger.Log("KiraiMod not found. Maintaining full control.");
                Store();
                Apply();
            }
            else
            {
                MelonLogger.Log("KiraiMod found! Forfeiting all control.");
            }

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("FClient")))
            {
                MelonLogger.Log("FClient detected, not moving QuickMenu around.");
                disableQM = true;
            }

            if (!disableQM && earlyAccess != null) MelonCoroutines.Start(UpdateClock());
        }

        private System.Collections.IEnumerator UpdateClock()
        {
            for (;;)
            {
                earlyAccess.text = DateTime.Now.ToLongTimeString();
                yield return new WaitForSeconds(1);
            }
        }

        public void Apply()
        {
            if (!hasLoaded) return;
            isApplied = true;
            SetColors(0);
            if (!disableQM) SetQuickMenu(0);
        }

        public void Store()
        {
            if (!hasLoaded) return;
            hasStored = true;
            SetColors(1);
            if (!disableQM) SetQuickMenu(1);
        }

        public void Restore()
        {
            if (!hasLoaded) return;
            isApplied = false;
            SetColors(2);
            if (!disableQM) SetQuickMenu(2);
        }

        private void SetColors(int op)
        {
            // screen background
            Image background = screen.Find("Backdrop/Backdrop/Background")?.GetComponent<Image>();
            if (background != null) Utils.Move(op, ref background, ref Memory.screenBackground, Config.primary2);

            // mic off
            Image voiceOff = hud.Find("VoiceDotParent/VoiceDotDisabled").GetComponent<Image>();
            if (voiceOff != null) Utils.Move(op, ref voiceOff, ref Memory.micOff, Config.primary);

            // afk
            Image afkIcon = hud.Find("AFK/Icon").GetComponent<Image>();
            if (afkIcon != null) Utils.Move(op, ref afkIcon, ref Memory.afkIcon, Config.primary);

            // action menu main background left
            PedalGraphic amMBgL = am.Find("MenuL/ActionMenu/Main/Background")?.GetComponent<PedalGraphic>();
            if (amMBgL != null) Utils.Move(op, ref amMBgL, ref Memory.amMBgL, Config.background);

            // action menu main background right
            PedalGraphic amMBgR = am.Find("MenuR/ActionMenu/Main/Background")?.GetComponent<PedalGraphic>();
            if (amMBgR != null) Utils.Move(op, ref amMBgR, ref Memory.amMBgR, Config.background);

            // action menu main background left
            PedalGraphic amRPMBgL = am.Find("MenuL/ActionMenu/RadialPuppetMenu/Container/Background")?.GetComponent<PedalGraphic>();
            if (amRPMBgL != null) Utils.Move(op, ref amRPMBgL, ref Memory.amRPMBgL, Config.background);

            // action menu main background right
            PedalGraphic amRPMBgR = am.Find("MenuR/ActionMenu/RadialPuppetMenu/Container/Background")?.GetComponent<PedalGraphic>();
            if (amRPMBgR != null) Utils.Move(op, ref amRPMBgR, ref Memory.amRPMBgR, Config.background);
        }

        private void SetQuickMenu(int op)
        {
            float size = -Utils.EstimateBlockSize();

            RectTransform background = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements/_Background")?.GetComponent<RectTransform>();
            if (background != null) Utils.Move(op, ref background, ref Memory.backgroundSize, new Vector2(100 + size * (Config.extended ? 4 : 2 ), 100));
            if (background != null) Utils.Move(op, ref background, ref Memory.backgroundPos, new Vector3(Config.extended ? -size : 0, 0, 0));

            RectTransform infobar = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements/_InfoBar")?.GetComponent<RectTransform>();
            if (infobar != null) Utils.Move(op, ref infobar, ref Memory.infobarSize, new Vector2(1680 + size * 2, 285.7f));
        }
    }
}
