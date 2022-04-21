using MelonLoader;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class ItemESP : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("ItemESP", "See all items in the world", ButtonType.Toggle, 7, 1, nameof(state))
        };

        private VRC_Pickup[] cache;
        private GameObject go;
        private LineRenderer lr;

        public override void OnStateChange(bool state)
        {
            if (state) Refresh();
            if (lr != null) lr.enabled = state;
        }

        public override void OnUpdate()
        {
            if (!state) return;
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            Vector3 src = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;

            if (src == null) return;

            if (cache != null)
            {
                lr.positionCount = cache.Length * 2;
                for (int i = 0; i < cache.Length; i++)
                {
                    if (cache[i] != null) lr.SetPosition(i * 2, src); //src
                    lr.SetPosition(i * 2 + 1, cache[i].transform.position); //dest
                }
            }
        }

        public override void OnLevelWasLoaded()
        {
            if (!state) return;

            cache = null;
            Refresh();
        }

        private void Refresh()
        {
            cache = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();

            if (go == null) go = new GameObject();
            if (lr == null)
            {
                lr = go.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                lr.startWidth = 0.002f;
                lr.endWidth = 0.002f;
                lr.useWorldSpace = false;
                lr.endColor = Utils.Colors.primary;
                lr.startColor = Utils.Colors.primary;
            }
        }
    }
}
