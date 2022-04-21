using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class XUtils : ModuleBase
    {
        public RaycastHit hit;
        public bool state2;

        private GameObject puppet;
        private LineRenderer lr;

        public override void OnStateChange(bool state)
        {
            if (lr != null) lr.enabled = state;
        }

        public override void OnUpdate()
        {
            if (!state) return;

            VRCPlayerApi.TrackingData tt = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);

            if (puppet == null) puppet = new GameObject();
            if (lr == null)
            {
                lr = puppet.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                lr.startWidth = 0.02f;
                lr.endWidth = 0.02f;
                lr.useWorldSpace = false;
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, Vector3.forward * 1000);
                lr.endColor = Utils.Colors.primary;
                lr.startColor = Utils.Colors.primary;
            }

            puppet.transform.position = tt.position;
            puppet.transform.rotation = tt.rotation;

            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.75f)
            {
                if (Physics.Raycast(puppet.transform.position, puppet.transform.forward, out hit, 1000.0f, -1))
                {
                    SetState(false);
                    state2 = true;
                    HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(hit.collider.gameObject.GetComponent<Renderer>(), true);
                    Shared.menu.selected = 5;
                }
            }
        }
    }
}
