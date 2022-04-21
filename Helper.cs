using MelonLoader;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;
using VRC.UI;

namespace KiraiMod
{
    class Helper
    {
        public static void DeletePortals()
        {
            PortalTrigger[] portals = Resources.FindObjectsOfTypeAll<PortalTrigger>();
            for (int i = 0; i < portals.Length; i++)
            {
                if (!portals[i].gameObject.activeInHierarchy || portals[i].gameObject.GetComponentInParent<VRC_PortalMarker>() != null) continue;

                Object.Destroy(portals[i].gameObject);
            }
        }

        public static void PortalPlayer(Player player, float distance, bool blank = false)
        {
            if (player == null) return;

            GameObject portal = Networking.Instantiate(VRC_EventHandler.VrcBroadcastType.Always, "Portals/PortalInternalDynamic", player.transform.position + player.transform.forward * distance, player.transform.rotation);

            if (portal == null) return;

            Networking.RPC(RPC.Destination.AllBufferOne, portal, "ConfigurePortal", new Il2CppSystem.Object[] {
                "wrld_8365f7c2-9771-4bf3-8bcc-901fbe9a903d",
                "KOS",
                new Il2CppSystem.Int32
                {
                    m_value = -666
                }.BoxIl2CppObject()
            });

            if (blank) MelonCoroutines.Start(Utils.DestroyDelayed(1.0f, portal.GetComponent<PortalInternal>()));
        }

        public static void Teleport(Vector3 pos)
        {
            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position = pos;
        }

        public static void BringPickups()
        {
            foreach (VRC_Pickup pickup in Object.FindObjectsOfType<VRC_Pickup>())
            {
                Networking.LocalPlayer.TakeOwnership(pickup.gameObject);
                pickup.transform.localPosition = new Vector3(0, 0, 0);
                pickup.transform.position = (Shared.targetPlayer?.transform.position ?? VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position) + new Vector3(0, 0.1f, 0);
            }
        }

        public static void DropTarget()
        {
            Shared.targetPlayer = null;
        }
    }
}
