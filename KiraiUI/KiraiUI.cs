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

        }

        internal static class Memory
        {
            public static Color screenBackground;
            public static Color micOff;
            public static Color afkIcon;
        }

        public void Apply()
        {
            SetColors(0);
        }

        public void Store()
        {
            hasStored = true;
            SetColors(1);
        }

        public void Restore()
        {
            SetColors(2);
        }

        private void SetColors(int op)
        {
            // screen background
            Image background = screen.Find("Backdrop/Backdrop/Background")?.GetComponent<Image>();
            if (background != null) Move(op, ref background, ref Memory.screenBackground, Colors.primary2);

            // mic off
            Image voiceOff = hud.Find("VoiceDotParent/VoiceDotDisabled").GetComponent<Image>();
            if (voiceOff != null) Move(op, ref voiceOff, ref Memory.micOff, Colors.primary);

            // afk
            Image afkIcon = hud.Find("AFK/Icon").GetComponent<Image>();
            if (afkIcon != null) Move(op, ref afkIcon, ref Memory.afkIcon, Colors.primary);
        }

        private void Move(int op, ref Image c, ref Color s, Color i)
        {
            if      (op == 0) c.color = i;
            else if (op == 1) s = c.color;
            else              c.color = s;
        }
    }
}
