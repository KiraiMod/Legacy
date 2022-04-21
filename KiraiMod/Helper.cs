using MelonLoader;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;
using VRC.Udon;
using VRC.UI;
using VRCSDK2;
using VRC_AvatarPedestal = VRC.SDKBase.VRC_AvatarPedestal;
using VRC_EventHandler = VRC.SDKBase.VRC_EventHandler;
using VRC_Pickup = VRC.SDKBase.VRC_Pickup;
using VRC_PortalMarker = VRC.SDKBase.VRC_PortalMarker;

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

            PortalPosition(player.transform.position + player.transform.forward * distance, player.transform.rotation, blank);
        }

        public static void PortalPosition(Vector3 position, Quaternion rotation, bool blank = false)
        {
            GameObject portal = Networking.Instantiate(VRC_EventHandler.VrcBroadcastType.Always, "Portals/PortalInternalDynamic", position, rotation);

            if (portal == null) return;

            Networking.RPC(RPC.Destination.AllBufferOne, portal, "ConfigurePortal", new Il2CppSystem.Object[] {
                "wrld_5b89c79e-c340-4510-be1b-476e9fcdedcc",
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

        public static void CrashSelected()
        {
            if (Shared.targetPlayer is null)
            {
                Utils.HUDMessage("No player is targeted");
                return;
            }

            if (Object.FindObjectOfType<VRCSDK2.VRC_SceneDescriptor>() != null)
            {
                VRC_ObjectSync sync = Resources.FindObjectsOfTypeAll<VRC_ObjectSync>().FirstOrDefault(o =>
                    o.GetComponents<Collider>().Concat(o.GetComponentsInChildren<Collider>()).Any(c => !c.isTrigger && ((1016111 >> c.gameObject.layer) & 1) == 1));
                if (sync != null) MelonCoroutines.Start(OutOfRangeCrash(sync.transform, Shared.targetPlayer));
                else Utils.HUDMessage("World is invalid.");
            }
            else
            {
                UdonBehaviour sync = Resources.FindObjectsOfTypeAll<UdonBehaviour>().FirstOrDefault(o => 
                    o.SynchronizePosition && o.GetComponents<Collider>().Concat(o.GetComponentsInChildren<Collider>()).Any(c => !c.isTrigger && ((1016111 >> c.gameObject.layer) & 1) == 1));
                if (sync != null) MelonCoroutines.Start(OutOfRangeCrash(sync.transform, Shared.targetPlayer));
                else
                {
                    VRCStation[] stations = Resources.FindObjectsOfTypeAll<VRCStation>();
                    if (stations.Length > 0) MelonCoroutines.Start(RunawayChairSpamCrash(stations));
                    else Utils.HUDMessage("World is invalid.");
                }
            }
        }

        public static System.Collections.IEnumerator OutOfRangeCrash(Transform pickup, Player player)
        {
            Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
            for (int i = 0; i < 90; i++)
            {
                pickup.position = player.transform.position;
                pickup.rotation = Quaternion.Euler(0, 0, 0);
                yield return null;
            }

            pickup.position = new Vector3(pickup.position.x, Vector3.positiveInfinity.y, pickup.rotation.z);
        }

        public static System.Collections.IEnumerator RunawayChairSpamCrash(VRCStation[] stations)
        {
            for (int i = 0; i < 4; i++)
            {
                MelonCoroutines.Start(ChairSpamCrash(stations));
                yield return null;
                yield return null;
            }
        }

        public static System.Collections.IEnumerator ChairSpamCrash(VRCStation[] stations)
        {
            foreach (VRCStation station in stations)
            {
                if (station?.gameObject is null) continue;
                if (Shared.targetPlayer is null) yield break;
                Networking.SetOwner(Shared.targetPlayer.field_Private_VRCPlayerApi_0, station.gameObject);
                station.GetComponent<UdonBehaviour>().SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_interact");
                yield return null;
            }
        }

        public static void SetPedestals(string id)
        {
            if (!id.StartsWith("avtr_"))
                Utils.HUDMessage("Invalid avatar ID");
            else
                foreach (var pedestal in Object.FindObjectsOfType<VRC_AvatarPedestal>())
                {
                    Networking.SetOwner(Networking.LocalPlayer, pedestal.gameObject);
                    Networking.RPC(RPC.Destination.All, pedestal.gameObject, "SwitchAvatar", new Il2CppSystem.Object[] { id });
                }
        }

        public static void BroadcastCustomEvent(string name)
        {
            foreach (VRC.Udon.UdonBehaviour ub in Shared.modules.udon.behaviours)
                ub.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, name);
        }
    }
}
