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
            new ModuleInfo("Anti Menu", "Make other people unable to click their menus", ButtonType.Toggle, 2, Menu.PageIndex.toggles3, nameof(bAntiMenu)),
            new ModuleInfo("Annoyance Mode", "Orbit things around the targets head instead of their feet", ButtonType.Toggle, 9, Menu.PageIndex.toggles2, nameof(bAnnoyance)),
        };

        public override void OnStateChange(bool state)
        {
            if (!state) bAntiMenu = false;
        }

        public override void OnConfigLoaded()
        {
            state = true;
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
                Utils.HUDInput("Video URL", "Override", "https://www.youtube.com/watch?v=LhCYW9dKC5s", "", new Action<string>((value) =>
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
                Utils.HUDInput("Avatar ID", "Set Pedestals", "avtr_????????-????-????-????-????????????", "", new System.Action<string>((resp) =>
                {
                    Helper.SetPedestals(resp.Trim());
                }));
        }

        public void JoinWorldByID()
        {
            if (bUseClipboard)
                Helper.JoinWorldById(System.Windows.Forms.Clipboard.GetText().Trim());
            else
                Utils.HUDInput("World ID", "Join", "wrld_*:?????~*()~nonce()", "", new System.Action<string>((resp) =>
                {
                    Helper.JoinWorldById(resp.Trim());
                }));
        }
    }
}
