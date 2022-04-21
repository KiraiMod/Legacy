using UnityEngine;

namespace KiraiMod.Modules
{
    public class Portal : ModuleBase
    {
        public float distance = 0;
        public bool infinite = false;

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Auto Portal", "Drops portals on target every second", ButtonType.Toggle, 9, Menu.PageIndex.toggles1, nameof(state)),
            new ModuleInfo("Infinite Portals", "Dropped portals will not be deleted", ButtonType.Toggle, 7, Menu.PageIndex.toggles2, nameof(infinite)),
            new ModuleInfo("Portal", "Portals the targeted player", ButtonType.Button, 2, Menu.PageIndex.buttons1, nameof(PortalTarget)),
            new ModuleInfo("Delete\nPortals", "Delete all non-static portals", ButtonType.Button, 3, Menu.PageIndex.buttons1, nameof(DeletePortals)),
            new ModuleInfo("Portal Distance", ButtonType.Slider, 5, Menu.PageIndex.sliders1, nameof(distance), 1, 8)
        };

        public System.Collections.IEnumerator AutoPortal()
        {
            for (;;)
            {
                if (Shared.targetPlayer != null && state) Helper.PortalPlayer(Shared.targetPlayer, Shared.modules.portal.distance, Shared.modules.portal.infinite);
                yield return new WaitForSeconds(5.0f);
            }
        }

        public void PortalTarget()
        {
            Helper.PortalPlayer(Shared.targetPlayer ?? VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Private_Player_0, Shared.modules.portal.distance, Shared.modules.portal.infinite);
        }

        public void DeletePortals()
        {
            Helper.DeletePortals();
        }
    }
}
