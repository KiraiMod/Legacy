using System;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class Misc : ModuleBase
    {
        public bool bUseClipboard;
        public bool bAntiMenu;
        public bool bAnnoyance;
        public bool bPersistantQuickMenu;

        public bool BindsNumpad;
        public bool BindsTab;
        public bool BindsAlt;
        public bool HighStep;

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Force\nPickups", "Enabled theft on all pickups", ButtonType.Button, 0, Menu.PageIndex.buttons1, nameof(ForcePickups)),
            new ModuleInfo("Fast\nPickups", "Thrown pickups are very fast", ButtonType.Button, 1, Menu.PageIndex.buttons1, nameof(FastPickups)),
            new ModuleInfo("Nuke\nVideoSync", "Overrides all video players to a custom URL", ButtonType.Button, 5, Menu.PageIndex.buttons1, nameof(NukeVideoSync)),
            new ModuleInfo("Bring\nPickups", "Brings all pickups in the scene", ButtonType.Button, 6, Menu.PageIndex.buttons1, nameof(BringPickups)),
            new ModuleInfo("Drop\nTarget", "Forget the current target", ButtonType.Button, 7, Menu.PageIndex.buttons1, nameof(DropTarget)),
            new ModuleInfo("Change\nPedestals", "Change all pedestals to an avatar ID", ButtonType.Button, 3, Menu.PageIndex.buttons2, nameof(ChangePedestals)),
            new ModuleInfo("Join World\nvia ID", "Join a world using a full instance id", ButtonType.Button, 6, Menu.PageIndex.buttons2, nameof(JoinWorldByID)),
            new ModuleInfo("Clipboard", "Use the clipboard instead of a popup input", ButtonType.Toggle, 10, Menu.PageIndex.toggles2, nameof(bUseClipboard)),
            new ModuleInfo("Anti Menu", "Make other people unable to click their menus", ButtonType.Toggle, 3, Menu.PageIndex.toggles3, nameof(bAntiMenu)),
            new ModuleInfo("Annoyance Mode", "Orbit things around the targets head instead of their feet", ButtonType.Toggle, 9, Menu.PageIndex.toggles2, nameof(bAnnoyance)),
            new ModuleInfo("Persistant QuickMenu", "Keep the Quick Menu open when moving around", ButtonType.Toggle, 2, Menu.PageIndex.toggles3, nameof(bPersistantQuickMenu)),
            new ModuleInfo("Numpad Binds", "Use Numlock + Keypad to activate binds", ButtonType.Toggle, 1, Menu.PageIndex.toggles3, nameof(BindsNumpad)),
            new ModuleInfo("Tab Binds", "Use Tab + Alphabetical to activate binds", ButtonType.Toggle, 5, Menu.PageIndex.toggles3, nameof(BindsTab)),
            new ModuleInfo("Alt Binds", "Use Alt + Numerical to activate binds", ButtonType.Toggle, 9, Menu.PageIndex.toggles3, nameof(BindsAlt)),
            new ModuleInfo("High Step", "Step up objects larger than you normally can", ButtonType.Toggle, 0, Menu.PageIndex.toggles2, nameof(HighStep))
        };

        public override void OnStateChange(bool state)
        {
            if (!state) bAntiMenu = false;
        }

        public override void OnConfigLoaded()
        {
            state = true;
            MelonLoader.MelonCoroutines.Start(WaitForMenu());
        }

        public override void OnLevelWasLoaded()
        {
            if (HighStep) VRC.Player.prop_Player_0.GetComponent<CharacterController>().stepOffset = 1.6f;
        }

        public System.Collections.IEnumerator WaitForMenu()
        {
            while (Shared.menu is null) yield return new WaitForSeconds(1);

            ReenableDefault();
        }

        public void ForcePickups()
        {
            VRC_Pickup[] pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
            for (int i = 0; i < pickups.Length; i++)
            {
                pickups[i].DisallowTheft = false;
            }
        }

        public void FastPickups()
        {
            VRC_Pickup[] pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
            for (int i = 0; i < pickups.Length; i++)
            {
                pickups[i].ThrowVelocityBoostScale = 5f;
            }
        }

        public void NukeVideoSync()
        {
            if (bUseClipboard)
                Helper.OverrideVideoPlayers(Utils.GetClipboard().Trim());
            else
                KiraiLib.HUDInput("Video URL", "Override", "https://www.youtube.com/watch?v=LhCYW9dKC5s", new Action<string>((value) =>
                {
                    Helper.OverrideVideoPlayers(value.Trim());
                }));
        }

        public void BringPickups()
        {
            Helper.BringPickups();
        }

        public void DropTarget()
        {
            Shared.TargetPlayer = null;
        }

        public void ChangePedestals()
        {
            if (bUseClipboard)
                Helper.SetPedestals(System.Windows.Forms.Clipboard.GetText().Trim());
            else
                KiraiLib.HUDInput("Avatar ID", "Set Pedestals", "avtr_8-4-4-4-12", new Action<string>((resp) =>
                {
                    Helper.SetPedestals(resp.Trim());
                }));
        }

        public void JoinWorldByID()
        {
            if (bUseClipboard)
                Helper.JoinWorldById(System.Windows.Forms.Clipboard.GetText().Trim());
            else
                KiraiLib.HUDInput("World ID", "Join", "wrld_*:?????~*()~nonce()", new Action<string>((resp) =>
                {
                    Helper.JoinWorldById(resp.Trim());
                }));
        }

        public void OnStateChangeBindsNumpad(bool state)
        {
            if (state)
            {
                DisableOthers(0);
            }
            else ReenableDefault();
        }

        public void OnStateChangeBindsTab(bool state)
        {
            if (state)
            {
                DisableOthers(1);
            }
            else ReenableDefault();
        }

        public void OnStateChangeBindsAlt(bool state)
        {
            if (state)
            {
                DisableOthers(2);
            }
            else ReenableDefault();
        }

        private void DisableOthers(int target)
        {
            if (target > 0)
                Shared.menu.Set(Utils.CreateID("Numpad Binds", (int)Menu.PageIndex.toggles3), false);

            if (target != 1)
                Shared.menu.Set(Utils.CreateID("Tab Binds", (int)Menu.PageIndex.toggles3), false);

            if (target < 2)
                Shared.menu.Set(Utils.CreateID("Alt Binds", (int)Menu.PageIndex.toggles3), false);
        }

        private void ReenableDefault()
        {
            if (BindsNumpad || BindsTab || BindsAlt) return;

            BindsNumpad = true;
            Shared.menu.Set(Utils.CreateID("Numpad Binds", (int)Menu.PageIndex.toggles3), true);
        }

        public void OnStateChangeHighStep(bool state)
        {
            VRC.Player.prop_Player_0.GetComponent<CharacterController>().stepOffset = state ? 1.6f : 0.5f;
        }
    }
}
