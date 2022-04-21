using Il2CppSystem;
using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class ItemOrbit : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("Item Orbit", "Orbit all items around target", ButtonType.Toggle, 8, 0, nameof(state))
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
            puppet.transform.position = (Shared.targetPlayer?.transform.position ?? VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position) + new Vector3(0, 0.2f, 0);

            puppet.transform.Rotate(new Vector3(0, 360f * Time.time, 0));

            foreach (VRC_Pickup pickup in cached)
            {
                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer) Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);

                pickup.transform.position = puppet.transform.position + puppet.transform.forward;

                puppet.transform.Rotate(new Vector3(0, 360 / cached.Length, 0));
            }

            UnityEngine.Object.Destroy(puppet);
        }

        public void Recache()
        {
            cached = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
        }
    }
}
