using MelonLoader;
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

        public static class Colors
        {
            public static Color primary = new Color(0.34f, 0f, 0.65f);
            public static Color primary2 = new Color(0.34f, 0f, 0.65f, 0.8f);
            public static Color highlight = new Color(0.8f, 0.8f, 1f);
        }

        Transform ui;
        Transform screen;
        Transform hud;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            instance = this;
        }

        public override void VRChat_OnUiManagerInit()
        {
            ui = GameObject.Find("UserInterface").transform;
            if (ui == null) MelonLogger.LogWarning("Didn't find UserInterface");

            screen = ui.Find("MenuContent");
            if (screen == null) MelonLogger.LogWarning("Didn't find MenuContent");

            hud = ui.Find("UnscaledUI/HudContent/Hud");
            if (hud == null) MelonLogger.LogWarning("Didn't find Hud");

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
        }

        public void Apply()
        {
            SetColors(0);
            SetQuickMenu(0);
        }

        public void Store()
        {
            hasStored = true;
            SetColors(1);
            SetQuickMenu(1);
        }

        public void Restore()
        {
            SetColors(2);
            SetQuickMenu(2);
        }

        private void SetColors(int op)
        {
            // screen background
            Image background = screen.Find("Backdrop/Backdrop/Background")?.GetComponent<Image>();
            if (background != null) Utils.Move(op, ref background, ref Memory.screenBackground, Colors.primary2);

            // mic off
            Image voiceOff = hud.Find("VoiceDotParent/VoiceDotDisabled").GetComponent<Image>();
            if (voiceOff != null) Utils.Move(op, ref voiceOff, ref Memory.micOff, Colors.primary);

            // afk
            Image afkIcon = hud.Find("AFK/Icon").GetComponent<Image>();
            if (afkIcon != null) Utils.Move(op, ref afkIcon, ref Memory.afkIcon, Colors.primary);
        }

        private void SetQuickMenu(int op)
        {
            float size = Utils.EstimateBlockSize() * -1;

            RectTransform background = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements/_Background")?.GetComponent<RectTransform>();
            if (background != null) Utils.Move(op, ref background, ref Memory.backgroundSize, new Vector2(100 + size * 2, 100));

            RectTransform infobar = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements/_InfoBar")?.GetComponent<RectTransform>();
            if (infobar != null) Utils.Move(op, ref infobar, ref Memory.infobarSize, new Vector2(1680 + size * 2, 285.7f));
        }
    }
}
