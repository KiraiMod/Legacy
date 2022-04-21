using MelonLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(KiraiMod.KiraiUI), "KiraiUI", null, "Kirai Chan#8315")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace KiraiMod
{
    public class KiraiUI : MelonMod
    {
        public static KiraiUI instance;

        public bool hasStored = false;
        public bool hasLoaded = false;
        public bool isApplied = false;

        public string version = "";

        public static class Config
        {
            public static Color primary = new Color(0.34f, 0f, 0.65f);
            public static Color primary2 = new Color(0.34f, 0f, 0.65f, 0.8f);
            public static Color highlight = new Color(0.8f, 0.8f, 1f);
            public static Color background = new Color(0, 0, 0);
            public static Color background2 = new Color(0, 0, 0, 0.9f);
            public static bool extended = false;
        }

        public static class CompatibilityModule
        {
            public static bool NoTheme;
            public static bool NoMovement;
            public static bool NoCompatibility;
        }

        Transform UIRoot;
        Transform SelectionMenu;
        Transform Screens;
        Transform HUD;
        Transform ActionMenu;
        Transform QuickMenuNewElements;
        Transform Fireball;
        Text EarlyAccessText;

        public override void OnApplicationStart()
        {
            instance = this;

            MelonMod mod = MelonHandler.Mods.FirstOrDefault(m => m.Assembly.GetName().Name == "KiraiMod");

            string location;

            if (mod is null) location = Location;
            else location = mod.Location;

            CRC32 crc = new CRC32();
            
            foreach(byte b in crc.ComputeHash(System.IO.File.ReadAllBytes(location)))
            {
                version += b.ToString("X");
            }
        }

        public override void VRChat_OnUiManagerInit()
        {
            hasLoaded = true;

            UIRoot = QuickMenu.prop_QuickMenu_0.transform.parent;

            System.Collections.Generic.List<Transform> check = new System.Collections.Generic.List<Transform>();

            #region Fetch
            Screens = UIRoot?.Find("MenuContent");
            HUD = UIRoot?.Find("UnscaledUI/HudContent/Hud");
            SelectionMenu = QuickMenu.prop_QuickMenu_0.transform.Find("ShortcutMenu");
            ActionMenu = UIRoot?.Find("ActionMenu");
            QuickMenuNewElements = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements");
            Fireball = GameObject.Find("_Application/CursorManager/BlueFireballMouse").transform;
            EarlyAccessText = SelectionMenu?.Find("EarlyAccessText")?.GetComponent<Text>();
            #endregion

            #region Compatibility
            if (!MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name == "KiraiMod"))
            {
                // we have no dom so we will do it ourselves -\_(._.)_/-
                MelonLogger.Log("KiraiMod not found. Maintaining full control.");

                Store();
                Apply();
            }
            else MelonLogger.Log("KiraiMod found! Forfeiting all control.");

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name == "FClient"))
            {
                MelonLogger.Log("FClient detected, not moving QuickMenu around.");
                CompatibilityModule.NoMovement = true;
            }
            #endregion

            if (EarlyAccessText != null) MelonCoroutines.Start(UpdateClock());
        }

        private System.Collections.IEnumerator UpdateClock()
        {
            for (;;)
            {
                if (isApplied)
                    EarlyAccessText.text = DateTime.Now.ToLongTimeString();
                yield return new WaitForSeconds(1);
            }
        }

        public void Apply()
        {
            if (!hasLoaded) return;
            isApplied = true;
            Execute(0);
        }

        public void Store()
        {
            if (!hasLoaded) return;
            hasStored = true;
            Execute(1);
        }

        public void Restore()
        {
            if (!hasLoaded) return;
            isApplied = false;
            Execute(2);
        }

        private void Execute(int op)
        {
            Image tmp1;
            PedalGraphic tmp2;
            Transform tmp3;
            Text tmp4;
            ParticleSystem tmp5;

            if (CompatibilityModule.NoCompatibility || !CompatibilityModule.NoTheme)
            {
                #region Screen Colors
                // screen background
                tmp1 = Screens.Find("Backdrop/Backdrop/Background").GetComponent<Image>();
                if (tmp1 != null) Utils.MoveImageColor(op, ref tmp1, ref Memory.screenBackground, Config.primary2);

                tmp3 = Screens.Find("Backdrop/Header/Tabs/ViewPort/Content");

                if (op != 1)
                    for (int i = 0; i < tmp3.childCount; i++)
                    {
                        if (i == tmp3.childCount - 1) tmp1 = tmp3.GetChild(i).GetComponentInChildren<Image>();
                        else tmp1 = tmp3.GetChild(i).GetComponent<Image>();

                        if (tmp1 != null) tmp1.color = op == 0 ? Color.clear : Color.white;
                    }

                #endregion
                #region HUD Colors
                // mic off
                tmp1 = HUD.Find("VoiceDotParent/VoiceDotDisabled").GetComponent<Image>();
                if (tmp1 != null) Utils.MoveImageColor(op, ref tmp1, ref Memory.micOff, Config.primary);

                // afk
                tmp1 = HUD.Find("AFK/Icon").GetComponent<Image>();
                if (tmp1 != null) Utils.MoveImageColor(op, ref tmp1, ref Memory.afkIcon, Config.primary);
                #endregion
                #region Cursor Colors
                tmp5 = Fireball.Find("Ball").GetComponent<ParticleSystem>();
                if (tmp5 != null) Utils.MoveParticleSystemColor(op, ref tmp5, ref Memory.fireballBall, Config.primary);

                tmp5 = Fireball.Find("Glow").GetComponent<ParticleSystem>();
                if (tmp5 != null) Utils.MoveParticleSystemColor(op, ref tmp5, ref Memory.fireballGlow, Config.primary);

                tmp5 = Fireball.Find("Trail").GetComponent<ParticleSystem>();
                if (tmp5 != null) Utils.MoveParticleSystemColor(op, ref tmp5, ref Memory.fireballTrail, Config.primary);
                #endregion
                #region Action Menu Colors
                // action menu main background left
                tmp2 = ActionMenu.Find("MenuL/ActionMenu/Main/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amMBgL, Config.background);

                // action menu main background right
                tmp2 = ActionMenu.Find("MenuR/ActionMenu/Main/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amMBgR, Config.background);

                // action menu main background left
                tmp2 = ActionMenu.Find("MenuL/ActionMenu/RadialPuppetMenu/Container/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amRPMBgL, Config.background);

                // action menu main background right
                tmp2 = ActionMenu.Find("MenuR/ActionMenu/RadialPuppetMenu/Container/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amRPMBgR, Config.background);
                #endregion
                #region QuickMenu Colors

                tmp1 = QuickMenuNewElements.Find("_Background/Panel")?.GetComponent<Image>();
                if (tmp1 != null)
                {
                    Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                    Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneBackground, Config.background2);
                }

                tmp1 = QuickMenuNewElements.Find("_InfoBar/Panel")?.GetComponent<Image>();
                if (tmp1 != null)
                {
                    Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                    Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneInfoBar, Config.background2);
                }

                tmp3 = QuickMenuNewElements.Find("_CONTEXT");
                if (tmp3 != null)
                {
                    tmp1 = tmp3.Find("QM_Context_ToolTip/Panel")?.GetComponent<Image>();
                    if (tmp1 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneToolTip, Config.background2);
                    }

                    tmp1 = tmp3.Find("QM_Context_User_Hover/Panel")?.GetComponent<Image>();
                    if (tmp1 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneUserHover, Config.background2);
                    }

                    tmp1 = tmp3.Find("QM_Context_User_Selected/Panel")?.GetComponent<Image>();
                    if (tmp1 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneUserSelected, Config.background2);
                    }

                    tmp1 = tmp3.Find("QM_Context_Invite/Panel")?.GetComponent<Image>();
                    if (tmp2 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneInvite, Config.background2);
                    }
                }
                #endregion
            }

            if (CompatibilityModule.NoCompatibility || !CompatibilityModule.NoMovement)
            {
                #region QuickMenu Movement
                float size = 420f;

                RectTransform background = QuickMenuNewElements?.Find("_Background")?.GetComponent<RectTransform>();
                if (background != null) Utils.MoveRectTransformSizeDelta(op, ref background, ref Memory.backgroundSize, new Vector2(100 + size * (Config.extended ? 4 : 2), 100));
                if (background != null) Utils.MoveRectTransformLocalPosition(op, ref background, ref Memory.backgroundPos, new Vector3(Config.extended ? -size : 0, 0, 0));

                RectTransform infobar = QuickMenuNewElements?.Find("_InfoBar")?.GetComponent<RectTransform>();
                if (infobar != null) Utils.MoveRectTransformSizeDelta(op, ref infobar, ref Memory.infobarSize, new Vector2(1680 + size * 2, 285.7f));
                #endregion
            }

            CanvasScaler scalar = QuickMenu.prop_QuickMenu_0.GetComponent<CanvasScaler>();
            if (scalar != null) Utils.MoveCanvasScalarRefPixPerUnit(op, ref scalar, ref Memory.qmRPPU, 0);

            tmp3 = SelectionMenu?.Find("BuildNumText");
            tmp4 = tmp3?.GetComponent<Text>();
            var ddt = tmp3?.GetComponent<VRC.UI.DebugDisplayText>();
            if (tmp4 != null && ddt != null)
            {
                if (op == 0)
                {
                    ddt.enabled = false;
                    tmp4.text = $"Version {VRCApplicationSetup.prop_VRCApplicationSetup_0.buildNumber}                                          {version}";
                }
                else if (op == 2) ddt.enabled = true;
            }

            if (op == 1) Memory.EarlyAccessText = EarlyAccessText.text;
            else if (op == 2) EarlyAccessText.text = Memory.EarlyAccessText;
        }

        public class CRC32
        {
            private readonly uint[] ChecksumTable;
            private readonly uint Polynomial = 0xEDB88320;

            public CRC32()
            {
                ChecksumTable = new uint[0x100];

                for (uint index = 0; index < 0x100; ++index)
                {
                    uint item = index;
                    for (int bit = 0; bit < 8; ++bit)
                        item = ((item & 1) != 0) ? (Polynomial ^ (item >> 1)) : (item >> 1);
                    ChecksumTable[index] = item;
                }
            }

            public byte[] ComputeHash(System.IO.Stream stream)
            {
                uint result = 0xFFFFFFFF;

                int current;
                while ((current = stream.ReadByte()) != -1)
                    result = ChecksumTable[(result & 0xFF) ^ (byte)current] ^ (result >> 8);

                byte[] hash = BitConverter.GetBytes(~result);
                Array.Reverse(hash);
                return hash;
            }

            public byte[] ComputeHash(byte[] data)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data))
                    return ComputeHash(stream);
            }
        }
    }
}
