using MelonLoader;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using VRC.Udon;
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

        public static GameObject PortalPosition(Vector3 position, Quaternion rotation, bool blank = false)
        {
            GameObject portal = Networking.Instantiate(VRC_EventHandler.VrcBroadcastType.Always, "Portals/PortalInternalDynamic", position, rotation);

            if (portal == null) return null;

            Networking.RPC(RPC.Destination.AllBufferOne, portal, nameof(PortalInternal.ConfigurePortal), new Il2CppSystem.Object[] {
                "wrld_5b89c79e-c340-4510-be1b-476e9fcdedcc",
                (Shared.modules.misc.bAnnoyance && Shared.TargetPlayer != null)
                ? $"KOS\n{Shared.TargetPlayer.field_Private_APIUser_0.displayName}\0"
                : "KOS",
                new Il2CppSystem.Int32
                {
                    m_value = -666
                }.BoxIl2CppObject()
            });

            if (blank) MelonCoroutines.Start(Utils.DestroyDelayed(1.0f, portal.GetComponent<PortalInternal>()));

            return portal;
        }

        public static void SetTimer(PortalInternal portal, float time)
        {
            if (portal is null) return;

            portal.field_Private_Single_1 = time * -1 + 30;

            //Networking.RPC(RPC.Destination.AllBufferOne, portal, nameof(PortalInternal.SetTimerRPC), new Il2CppSystem.Object[] {
            //    new Il2CppSystem.Single
            //    {
            //        m_value = time
            //    }.BoxIl2CppObject(),
            //    Player.prop_Player_0
            //});
        }

        public static System.Collections.IEnumerator ReversePortal(PortalInternal portal)
        {
            for (int i = 0; i < 116; i++)
            {
                if (portal == null || Networking.GetOwner(portal.gameObject) != Networking.LocalPlayer) yield break;
                SetTimer(portal, i / 4 + 2);
                yield return new WaitForSecondsRealtime(0.25f);
            }
            SetTimer(portal, 0);
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
                pickup.transform.position = (Shared.TargetPlayer?.transform.position ?? VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position) + new Vector3(0, 0.1f, 0);
            }
        }

        public static void DropTarget()
        {
            Shared.TargetPlayer = null;
        }

        public static void CrashSelected()
        {
            if (Shared.TargetPlayer is null)
            {
                KiraiLib.Logger.Log("No player is targeted");
                return;
            }

            if (Object.FindObjectOfType<VRCSDK2.VRC_SceneDescriptor>() != null)
            {
                VRC_ObjectSync sync = Resources.FindObjectsOfTypeAll<VRC_ObjectSync>().FirstOrDefault(o =>
                    o.GetComponents<Collider>().Concat(o.GetComponentsInChildren<Collider>()).Any(c => !c.isTrigger && ((1016111 >> c.gameObject.layer) & 1) == 1));
                if (sync != null) MelonCoroutines.Start(OutOfRangeCrash(sync.transform, Shared.TargetPlayer));
                else KiraiLib.Logger.Log("World is invalid.");
            }
            else
            {
                UdonBehaviour sync = Resources.FindObjectsOfTypeAll<UdonBehaviour>().FirstOrDefault(o => 
                    o.SynchronizePosition && o.GetComponents<Collider>().Concat(o.GetComponentsInChildren<Collider>()).Any(c => !c.isTrigger && ((1016111 >> c.gameObject.layer) & 1) == 1));
                if (sync != null) MelonCoroutines.Start(OutOfRangeCrash(sync.transform, Shared.TargetPlayer));
                else KiraiLib.Logger.Log("World is invalid.");
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

        public static void SetPedestals(string id)
        {
            if (!id.StartsWith("avtr_"))
                KiraiLib.Logger.Log("Invalid avatar ID");
            else
                foreach (var pedestal in Object.FindObjectsOfType<VRC_AvatarPedestal>())
                {
                    Networking.SetOwner(Networking.LocalPlayer, pedestal.gameObject);
                    Networking.RPC(RPC.Destination.All, pedestal.gameObject, "SwitchAvatar", new Il2CppSystem.Object[] { id });
                }
        }

        public static void BroadcastCustomEvent(string name)
        {
            foreach (UdonBehaviour ub in Shared.modules.udon.behaviours)
                ub.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, name);
        }

        public static void OverrideVideoPlayers(string url)
        {
            if (url == "") url = "https://www.youtube.com/watch?v=LhCYW9dKC5s";
            if (url.Contains("youtube.com/watch?v="))
            {
                // legacy players (win7)
                foreach (SyncVideoPlayer svp in Object.FindObjectsOfType<SyncVideoPlayer>())
                {
                    if (svp == null) continue;
                    Networking.LocalPlayer.TakeOwnership(svp.gameObject);
                    svp.field_Private_VRC_SyncVideoPlayer_0.Stop();
                    svp.field_Private_VRC_SyncVideoPlayer_0.Clear();
                    svp.field_Private_VRC_SyncVideoPlayer_0.AddURL(url);
                    svp.field_Private_VRC_SyncVideoPlayer_0.Next();
                    svp.field_Private_VRC_SyncVideoPlayer_0.Play();
                }

                // modern players (win10)
                foreach (SyncVideoStream svs in Object.FindObjectsOfType<SyncVideoStream>())
                {
                    if (svs == null) continue;
                    Networking.LocalPlayer.TakeOwnership(svs.gameObject);
                    svs.field_Private_VRC_SyncVideoStream_0.Stop();
                    svs.field_Private_VRC_SyncVideoStream_0.Clear();
                    svs.field_Private_VRC_SyncVideoStream_0.AddURL(url);
                    svs.field_Private_VRC_SyncVideoStream_0.Next();
                    svs.field_Private_VRC_SyncVideoStream_0.Play();
                }
            } else
            {
                KiraiLib.Logger.Log("Invalid video URL");
            }
        }

        public static bool JoinWorldById(string id)
        {
            if (!Networking.GoToRoom(id))
            {
                string[] split = id.Split(':');

                if (split.Length != 2) return false;

                new PortalInternal().Method_Private_Void_String_String_PDM_0(split[0], split[1]);
            }

            return true;
        }
    }
}
