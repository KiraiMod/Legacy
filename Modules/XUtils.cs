using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class XUtils : ModuleBase
    {
        public Collider hit;

        public override void OnUpdate()
        {
            if (!state) return;

            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.75f)
            {
                VRCPlayerApi.TrackingData tt = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                GameObject puppet = new GameObject();
                puppet.transform.position = tt.position;
                puppet.transform.rotation = tt.rotation;
                RaycastHit rcHit;
                if (Physics.Raycast(puppet.transform.position, puppet.transform.forward, out rcHit, 1000.0f, -1))
                {
                    SetState(false);
                    hit = rcHit.collider;
                    HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(hit.gameObject.GetComponent<Renderer>(), true);
                    Shared.menu.selected = 4;
                }
                Object.Destroy(puppet);
            }
        }
    }
}
