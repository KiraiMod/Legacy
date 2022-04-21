using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class Flight : ModuleBase
    {
        public Vector3 oGravity;
        public float speed = 8;

        public override void OnStateChange(bool state)
        {
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            VRCMotionState motion = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponent<VRCMotionState>();

            if (motion == null) return;

            if (state)
            {
                oGravity = Physics.gravity;
                Physics.gravity = new Vector3(0, 0, 0);
            }
            else
            {
                Physics.gravity = oGravity;
                motion.Method_Public_Void_0();
            }
        }

        public override void OnUpdate()
        {
            if (!state && !Shared.modules.noclip.state) return;

            if (Networking.LocalPlayer == null) return;
            if (Networking.LocalPlayer.IsUserInVR())
            {
                VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.right * Shared.modules.flight.speed * Time.deltaTime * Input.GetAxis("Horizontal");
                VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.up * Shared.modules.flight.speed * Time.deltaTime * Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
                VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.forward * Shared.modules.flight.speed * Time.deltaTime * Input.GetAxis("Vertical");
            }
            else
            {
                if (Input.GetKey(KeyCode.Q)) VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject.transform.position -= new Vector3(0f, Shared.modules.flight.speed * Time.deltaTime, 0f);
                if (Input.GetKey(KeyCode.E)) VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject.transform.position += new Vector3(0f, Shared.modules.flight.speed * Time.deltaTime, 0f);

                if (Input.GetKey(KeyCode.W)) VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.forward * Shared.modules.flight.speed * Time.deltaTime;
                if (Input.GetKey(KeyCode.A)) VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.right * -1 * Shared.modules.flight.speed * Time.deltaTime;
                if (Input.GetKey(KeyCode.S)) VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.forward * -1 * Shared.modules.flight.speed * Time.deltaTime;
                if (Input.GetKey(KeyCode.D)) VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.right * Shared.modules.flight.speed * Time.deltaTime;
            }

            if (!Shared.modules.noclip.state && Physics.gravity.y != 0)
            {
                Shared.modules.flight.oGravity = Physics.gravity;
                Physics.gravity = new Vector3(0, 0, 0);
            }

            Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
        }
    }
}