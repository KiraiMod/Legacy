using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class ItemOrbit : ModuleBase
    {
        public float speed = 1;
        public float size = 1;

        public new ModuleInfo[] info = {
            new ModuleInfo("Item Orbit", "Orbit all items around target", ButtonType.Toggle, 8, Shared.PageIndex.toggles1, nameof(state)),
            new ModuleInfo("Item Orbit Speed", ButtonType.Slider, 6, Shared.PageIndex.sliders1, nameof(speed), 0, 4),
            new ModuleInfo("Item Orbit Size", ButtonType.Slider, 8, Shared.PageIndex.sliders1, nameof(size), 0, 4)
        };

        VRC_Pickup[] cached;

        public override void OnLevelWasLoaded()
        {
            if (state) Recache();
        }

        public override void OnStateChange(bool state)
        {
            if (state) Recache();
        }

        public override void OnUpdate()
        {
            if (!state) return;

            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            if (cached == null) Recache();

            GameObject puppet = new GameObject();

            if (Shared.modules.misc.bAnnoyance)
                puppet.transform.position = (Shared.TargetPlayer?.field_Private_VRCPlayerApi_0 ?? Networking.LocalPlayer).GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            else
                puppet.transform.position = (Shared.TargetPlayer?.transform.position ?? VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position) + new Vector3(0, 0.2f, 0);

            puppet.transform.Rotate(new Vector3(0, 360f * Time.time * speed, 0));

            foreach (VRC_Pickup pickup in cached)
            {
                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer) Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);

                pickup.transform.position = puppet.transform.position + puppet.transform.forward * size;

                puppet.transform.Rotate(new Vector3(0, 360 / cached.Length, 0));
            }

            Object.Destroy(puppet);
        }

        public override void OnPlayerLeft(Player player)
        {
            if (Shared.TargetPlayer == player)
                (KiraiLib.UI.elements[Utils.CreateID("Item Orbit", (int)Shared.PageIndex.toggles1)] as KiraiLib.UI.Toggle).SetState(false);
        }

        public void Recache()
        {
            cached = Object.FindObjectsOfType<VRC_Pickup>();
        }
    }
}
