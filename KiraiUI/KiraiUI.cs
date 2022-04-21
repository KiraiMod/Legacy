using MelonLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(KiraiMod.KiraiUI), "KiraiUI", "1", "Kirai Chan#8315")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("SmallUserVolume")]

namespace KiraiMod
{
    public class KiraiUI : MelonMod
    {
        public static KiraiUI instance;

        public bool hasStored = false;
        public bool hasLoaded = false;
        public bool isApplied = false;

        public static class Config
        {
            public static Color PrimaryColor = new Color(0.34f, 0f, 0.65f);
            public static Color FullMenuBackgroundColor = new Color(0.34f, 0f, 0.65f, 0.8f);
            public static Color ActionMenuBackgroundColor = new Color(0, 0, 0);
            public static Color QuickMenuBackgroundColor = new Color(0, 0, 0, 0.9f);
            public static bool extended = false;
            public static int PulseSkipCooldown = 3;
            public static int PulseLow = -100;
            public static int PulseHigh = 50;
        }

        public static class CompatibilityModule
        {
            public static bool NoColor;
            public static bool NoTheme;
            public static bool NoPulse = true;
            public static bool NoMovement;
            public static bool NoCompatibility;
        }

        Transform UIRoot;
        Transform Screens;
        Transform HUD;
        Transform ActionMenu;
        Transform QuickMenuNewElements;
        Transform Fireball;
        Text EarlyAccessText;
        CanvasScaler Canvas;

        public override void OnApplicationStart()
        {
            instance = this;
        }

        private int SkipCount = 0;
        private bool PulseFlip = false;

        public override void OnUpdate()
        {
            if (CompatibilityModule.NoCompatibility || !CompatibilityModule.NoPulse)
            {
                if (!isApplied || Canvas is null) return;

                SkipCount++;

                if (SkipCount == Config.PulseSkipCooldown)
                {
                    SkipCount = 0;

                    if (Canvas.referencePixelsPerUnit >= Config.PulseHigh || 
                        Canvas.referencePixelsPerUnit <= Config.PulseLow)
                        PulseFlip ^= true;

                    if (PulseFlip) Canvas.referencePixelsPerUnit++;
                    else Canvas.referencePixelsPerUnit--;
                }
            }
        }

        public override void VRChat_OnUiManagerInit()
        {
            hasLoaded = true;

            UIRoot = QuickMenu.prop_QuickMenu_0.transform.parent;

            #region Fetch
            Screens = UIRoot?.Find("MenuContent");
            HUD = UIRoot?.Find("UnscaledUI/HudContent/Hud");
            ActionMenu = UIRoot?.Find("ActionMenu");
            QuickMenuNewElements = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements");
            Fireball = GameObject.Find("_Application/CursorManager/BlueFireballMouse").transform;
            EarlyAccessText = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements/_InfoBar/EarlyAccessText")?.GetComponent<Text>();
            Canvas = QuickMenu.prop_QuickMenu_0.GetComponent<CanvasScaler>();
            #endregion

            #region Compatibility
            if (!MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name == "KiraiMod"))
            {
                // we have no dom so we will do it ourselves -\_(._.)_/-
                MelonLogger.Msg("KiraiMod not found. Maintaining full control.");

                Store();
                Apply();
            }
            else MelonLogger.Msg("KiraiMod found! Forfeiting all control.");

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name == "FClient"))
            {
                MelonLogger.Msg("FClient detected, not moving QuickMenu around.");
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

            if (CompatibilityModule.NoCompatibility || !CompatibilityModule.NoColor)
            {
                #region Screen Colors
                // screen background
                tmp1 = Screens.Find("Backdrop/Backdrop/Background").GetComponent<Image>();
                if (tmp1 != null) Utils.MoveImageColor(op, ref tmp1, ref Memory.screenBackground, Config.FullMenuBackgroundColor);
                #endregion
                #region HUD Colors
                // mic off
                tmp1 = HUD.Find("VoiceDotParent/VoiceDotDisabled").GetComponent<Image>();
                if (tmp1 != null) Utils.MoveImageColor(op, ref tmp1, ref Memory.micOff, Config.PrimaryColor);

                // afk
                tmp1 = HUD.Find("AFK/Icon").GetComponent<Image>();
                if (tmp1 != null) Utils.MoveImageColor(op, ref tmp1, ref Memory.afkIcon, Config.PrimaryColor);
                #endregion
                #region Cursor Colors
                tmp5 = Fireball.Find("Ball").GetComponent<ParticleSystem>();
                if (tmp5 != null) Utils.MoveParticleSystemColor(op, ref tmp5, ref Memory.fireballBall, Config.PrimaryColor);
                if (tmp5 != null) Utils.MoveParticleSystemGravity(op, ref tmp5, ref Memory.ballGravity, -0.01f);
                if (tmp5 != null) Utils.MoveParticleSystemLifetime(op, ref tmp5, ref Memory.ballLifetime, 0.5f);
                if (tmp5 != null) Utils.MoveParticleSystemEmission(op, ref tmp5, ref Memory.ballEmission, 100);
                if (tmp5 != null) Utils.MoveParticleSystemSimulationSpace(op, ref tmp5, ref Memory.ballSimSpace, ParticleSystemSimulationSpace.World);

                tmp5 = Fireball.Find("Glow").GetComponent<ParticleSystem>();
                if (tmp5 != null) Utils.MoveParticleSystemColor(op, ref tmp5, ref Memory.fireballGlow, Config.PrimaryColor);

                tmp5 = Fireball.Find("Trail").GetComponent<ParticleSystem>();
                if (tmp5 != null) Utils.MoveParticleSystemColor(op, ref tmp5, ref Memory.fireballTrail, Config.PrimaryColor);
                #endregion
                #region Action Menu Colors
                // action menu main background left
                tmp2 = ActionMenu.Find("MenuL/ActionMenu/Main/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amMBgL, Config.ActionMenuBackgroundColor);

                // action menu main background right
                tmp2 = ActionMenu.Find("MenuR/ActionMenu/Main/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amMBgR, Config.ActionMenuBackgroundColor);

                // action menu main background left
                tmp2 = ActionMenu.Find("MenuL/ActionMenu/RadialPuppetMenu/Container/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amRPMBgL, Config.ActionMenuBackgroundColor);

                // action menu main background right
                tmp2 = ActionMenu.Find("MenuR/ActionMenu/RadialPuppetMenu/Container/Background")?.GetComponent<PedalGraphic>();
                if (tmp2 != null) Utils.MovePedalGraphicColor(op, ref tmp2, ref Memory.amRPMBgR, Config.ActionMenuBackgroundColor);
                #endregion
                #region QuickMenu Colors

                tmp1 = QuickMenuNewElements.Find("_Background/Panel")?.GetComponent<Image>();
                if (tmp1 != null)
                {
                    Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                    Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneBackground, Config.QuickMenuBackgroundColor);
                }

                tmp1 = QuickMenuNewElements.Find("_InfoBar/Panel")?.GetComponent<Image>();
                if (tmp1 != null)
                {
                    Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                    Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneInfoBar, Config.QuickMenuBackgroundColor);
                }

                tmp3 = QuickMenuNewElements.Find("_CONTEXT");
                if (tmp3 != null)
                {
                    tmp1 = tmp3.Find("QM_Context_ToolTip/Panel")?.GetComponent<Image>();
                    if (tmp1 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneToolTip, Config.QuickMenuBackgroundColor);
                    }

                    tmp1 = tmp3.Find("QM_Context_User_Hover/Panel")?.GetComponent<Image>();
                    if (tmp1 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneUserHover, Config.QuickMenuBackgroundColor);
                    }

                    tmp1 = tmp3.Find("QM_Context_User_Selected/Panel")?.GetComponent<Image>();
                    if (tmp1 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneUserSelected, Config.QuickMenuBackgroundColor);
                    }

                    tmp1 = tmp3.Find("QM_Context_Invite/Panel")?.GetComponent<Image>();
                    if (tmp2 != null)
                    {
                        Utils.MoveImageSprite(op, ref tmp1, ref Memory.panel, null);
                        Utils.MoveImageColor(op, ref tmp1, ref Memory.qmneInvite, Config.QuickMenuBackgroundColor);
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

            if (CompatibilityModule.NoCompatibility || !CompatibilityModule.NoTheme)
            {
                if (Canvas != null) Utils.MoveCanvasScalarRefPixPerUnit(op, ref Canvas, ref Memory.qmRPPU, 0);

                tmp3 = Screens.Find("Backdrop/Header/Tabs/ViewPort/Content");

                if (op != 1)
                    for (int i = 0; i < tmp3.childCount; i++)
                    {
                        if (i == tmp3.childCount - 1) tmp1 = tmp3.GetChild(i).GetComponentInChildren<Image>();
                        else tmp1 = tmp3.GetChild(i).GetComponent<Image>();

                        if (tmp1 != null) tmp1.color = op == 0 ? UnityEngine.Color.clear : UnityEngine.Color.white;
                    }
            }


            tmp3 = QuickMenu.prop_QuickMenu_0.transform.Find("QuickMenu_NewElements/_InfoBar/BuildNumText");
            tmp4 = tmp3?.GetComponent<Text>();
            var ddt = tmp3?.GetComponent<VRC.UI.DebugDisplayText>();
            if (tmp4 != null && ddt != null)
            {
                if (op == 0)
                {
                    ddt.enabled = false;
                    tmp4.text = $"Version {VRCApplicationSetup.prop_VRCApplicationSetup_0.field_Public_Int32_0}";
                }
                else if (op == 2) ddt.enabled = true;
            }

            if (op == 1) Memory.EarlyAccessText = EarlyAccessText.text;
            else if (op == 2) EarlyAccessText.text = Memory.EarlyAccessText;
        }
    }
}
