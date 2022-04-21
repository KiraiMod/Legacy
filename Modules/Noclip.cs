using UnityEngine;

namespace KiraiMod.Modules
{
    public class Noclip : ModuleBase
    {
        public override void OnStateChange(bool state)
        {
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            Collider collider = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponent<Collider>();

            if (collider == null) return;

            collider.enabled = !state;
        }
    }
}