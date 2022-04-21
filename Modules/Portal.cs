using UnityEngine;

namespace KiraiMod.Modules
{
    public class Portal : ModuleBase
    {
        public float distance = 0;
        public bool infinite = false;

        public System.Collections.IEnumerator AutoPortal()
        {
            for (;;)
            {
                if (Shared.targetPlayer != null && state) Helper.PortalPlayer(Shared.targetPlayer, Shared.modules.portal.distance, Shared.modules.portal.infinite);
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}
