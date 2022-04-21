using UnityEngine;

namespace KiraiMod.Modules
{
    public class Noclip : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("Noclip", "Disable player collisions and fly around", ButtonType.Toggle, 2, Shared.PageIndex.toggles1, nameof(state))
        };

        public override void OnStateChange(bool state) => Execute(state);
        public override void OnLevelWasLoaded()
        {
            if (!state) return;

            MelonLoader.MelonCoroutines.Start(DelayedInit());
        }

        private System.Collections.IEnumerator DelayedInit()
        {
            while (VRCPlayer.field_Internal_Static_VRCPlayer_0 is null) yield return null;

            Execute(true);
        }

        private void Execute(bool state)
        {
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            Collider collider = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponent<Collider>();

            if (collider == null) return;

            collider.enabled = !state;
        }
    }
}