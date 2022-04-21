using MelonLoader;
using UnityEngine;

namespace KiraiMod.Modules
{
    public class Headlight : ModuleBase
    {
        private Light light;

        public new ModuleInfo[] info = {
            new ModuleInfo("Headlight", "Illuminate the world from your viewpoint", ButtonType.Toggle, 3, Menu.PageIndex.options2, nameof(state))
        };

        public override void OnStateChange(bool state)
        {
            Refresh();
        }

        public override void OnConfigLoaded()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (light == null) MelonCoroutines.Start(Initialize(state));
            else light.enabled = state;
        }

        public System.Collections.IEnumerator Initialize(bool state)
        {
            while (VRCVrCamera.field_Private_Static_VRCVrCamera_0 == null) yield return null;

            light = VRCVrCamera.field_Private_Static_VRCVrCamera_0.GetComponentInChildren<Camera>().gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.enabled = state;
        }
    }
}
