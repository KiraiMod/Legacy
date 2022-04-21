using MelonLoader;
using System.Reflection;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class FreeCam : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("FreeCam", "Move your camera without moving your player", ButtonType.Toggle, 11, Menu.PageIndex.toggles1, nameof(state))
        };

        private GamelikeInputController playerController;
        private Transform camera;

        public override void OnStateChange(bool state)
        {
            FetchPlayerController();

            GameObject forward = Player.prop_Player_0.transform.Find("ForwardDirection").gameObject;

            Transform head = VRCVrCamera.field_Private_Static_VRCVrCamera_0.transform.parent;
            
            if (state)
            {
                playerController.enabled = false;
                forward.active = false;
            }
            else
            {
                head.localPosition = Vector3.zero;
                head.localRotation = Quaternion.identity;

                playerController.enabled = true;
                forward.active = true;
            }
        }

        public override void OnUpdate()
        {
            if (!state) return;

            if (Networking.LocalPlayer == null) return;

            float x, y, z;

            if (Networking.LocalPlayer?.IsUserInVR() ?? false)
            {
                x = Input.GetAxis("Horizontal");
                y = Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");
                z = Input.GetAxis("Vertical");
            }
            else
            {
                x = y = z = 0;

                if (Input.GetKey(KeyCode.W)) z++;
                if (Input.GetKey(KeyCode.S)) z--;

                if (Input.GetKey(KeyCode.D)) x++;
                if (Input.GetKey(KeyCode.A)) x--;

                if (Input.GetKey(KeyCode.E)) y++;
                if (Input.GetKey(KeyCode.Q)) y--;
            }

            if (camera == null)
            {
                camera = VRCVrCamera.field_Private_Static_VRCVrCamera_0.GetComponentInChildren<Camera>().transform;
            }

            Transform head = VRCVrCamera.field_Private_Static_VRCVrCamera_0.transform.parent;

            head.position += camera.right * Shared.modules.flight.speed * Time.deltaTime * x;
            head.position += camera.forward * Shared.modules.flight.speed * Time.deltaTime * z;
            head.position += camera.up * Shared.modules.flight.speed * Time.deltaTime * y;
            head.rotation *= Quaternion.Euler(0, 
                Networking.LocalPlayer.IsUserInVR() ?
                (Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickHorizontal") * 2) :
                (Input.GetAxis("Mouse X") * 0.2f), 0);
        }

        private GamelikeInputController FetchPlayerController()
        {
            if (playerController == null)
                playerController = Player.prop_Player_0?.GetComponent<GamelikeInputController>();

            return playerController;
        }
    }
}
