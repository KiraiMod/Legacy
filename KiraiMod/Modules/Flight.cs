using UnityEngine;
using UnityEngine.XR;
using VRC.Animation;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class Flight : ModuleBase
    {
        private Transform camera;

        public Vector3 oGravity;

        public float speed = 8;
        public bool directional = false;

        public new ModuleInfo[] info = {
            new ModuleInfo("Flight", "Allows you to fly around with no gravity", ButtonType.Toggle, 1, Menu.PageIndex.options1, nameof(state)),
            new ModuleInfo("3D Flight", "Fly in the direction you are looking", ButtonType.Toggle, 4, Menu.PageIndex.options2, nameof(directional)),
            new ModuleInfo("Flight Speed", ButtonType.Slider, 2, Menu.PageIndex.sliders1, nameof(speed), 0, 32)
        };

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
                motion.Method_Public_Void_1();
            }
        }

        public override void OnUpdate()
        {
            if (!state && !Shared.modules.noclip.state) return;

            if (Networking.LocalPlayer == null) return;

            float x, y, z;

            if (XRDevice.isPresent)
            {
                x = Input.GetAxis("Horizontal");
                y = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
                z = Input.GetAxis("Vertical");
            } else
            {
                x = y = z = 0;

                if (Input.GetKey(KeyCode.W)) z++;
                if (Input.GetKey(KeyCode.S)) z--;

                if (Input.GetKey(KeyCode.D)) x++;
                if (Input.GetKey(KeyCode.A)) x--;

                if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) y++;
                if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) y--;
            }

            if (camera == null)
            {
                camera = VRCVrCamera.field_Private_Static_VRCVrCamera_0.GetComponentInChildren<Camera>().transform;
            }

            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += (directional
                ? camera.right
                : VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.right) * Shared.modules.flight.speed * Time.deltaTime * x;

            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += (directional
                ? camera.forward
                : VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.forward)
                * Shared.modules.flight.speed * Time.deltaTime * z;

            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += (directional
                ? camera.up
                : VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.up) * Shared.modules.flight.speed * Time.deltaTime * y;


            if (!Shared.modules.noclip.state && Physics.gravity.y != 0)
            {
                Shared.modules.flight.oGravity = Physics.gravity;
                Physics.gravity = new Vector3(0, 0, 0);
            }

            Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
        }
    }
}