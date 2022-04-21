using MelonLoader;
using System;
using UnityEngine;

namespace KiraiMod.Modules
{
    public class Portal : ModuleBase
    {
        public float distance = 0;
        public bool infinite = false;
        public bool AntiMenu = false;

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Auto Portal", "Drops portals on target every second", ButtonType.Toggle, 9, Shared.PageIndex.toggles1, nameof(state)),
            new ModuleInfo("Infinite Portals", "Dropped portals will not be deleted", ButtonType.Toggle, 7, Shared.PageIndex.toggles2, nameof(infinite)),
            new ModuleInfo("Portal", "Portals the targeted player", ButtonType.Button, 2, Shared.PageIndex.buttons1, nameof(PortalTarget)),
            new ModuleInfo("Delete\nPortals", "Delete all non-static portals", ButtonType.Button, 3, Shared.PageIndex.buttons1, nameof(DeletePortals)),
            new ModuleInfo("Portal Distance", ButtonType.Slider, 5, Shared.PageIndex.sliders1, nameof(distance), 1, 8),
            new ModuleInfo("Menu Passthrough", "Make other player's menus unclickable", ButtonType.Toggle, 3, Shared.PageIndex.toggles3, nameof(AntiMenu))
        };

        public override void OnStateChange(bool state)
        {
            if (!state) AntiMenu = false;
        }

        public void OnStateChangeAntiMenu(bool state)
        {
            if (!state) DeletePortals();
        }

        public System.Collections.IEnumerator AutoPortal()
        {
            for (;;)
            {
                try
                {
                    if (AntiMenu) Helper.PortalPosition(new Vector3(5.755256E+11f, 5.755256E+11f, 5.755256E+11f), Quaternion.identity);
                    else if (state && Shared.TargetPlayer != null)
                    {
                        if (Shared.modules.misc.bAnnoyance)
                        {
                            var player = VRC.PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[
                                    UnityEngine.Random.Range(0, VRC.PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count)];

                            Helper.PortalPlayer(
                                player,
                                Shared.modules.portal.distance,
                                Shared.modules.portal.infinite);

                            KiraiLib.Logger.Display($"Placing portal on <color={player.field_Private_APIUser_0.GetTrustColor().ToHex()}>{player.field_Private_APIUser_0.displayName}</color>", 1);
                        }
                        else Helper.PortalPlayer(Shared.TargetPlayer, Shared.modules.portal.distance, Shared.modules.portal.infinite);
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error(e.Message);
                }

                yield return new WaitForSeconds(5.0f);
            }
        }

        public void PortalTarget() => Helper.PortalPlayer(Shared.TargetPlayer ?? VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Private_Player_0, Shared.modules.portal.distance, Shared.modules.portal.infinite);
        public void DeletePortals() => Helper.DeletePortals();
    }
}
