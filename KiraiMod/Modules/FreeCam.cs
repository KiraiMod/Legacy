//#define FREECAM_LEGACY

using UnityEngine;
using VRC;
using VRC.Networking;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class FreeCam : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("FreeCam", "Move your camera without moving your player", ButtonType.Toggle, 11, Shared.PageIndex.toggles1, nameof(state))
        };

#if FREECAM_LEGACY
        private GamelikeInputController playerController;
        private Transform camera;
        private bool oFlight = false;
#else
        private FlatBufferNetworkSerializer serializer;
        private Vector3 oPos;
        private Quaternion oRot;
#endif
        private bool oNoclip = false;

        public override void OnStateChange(bool state)
        {
#if FREECAM_LEGACY
            FetchPlayerController();

            GameObject forward = Player.prop_Player_0.transform.Find("ForwardDirection").gameObject;

            Transform head = VRCVrCamera.field_Private_Static_VRCVrCamera_0.transform.parent;
            
            playerController.enabled = !state;
            forward.active = !state;
        
            if (!state)
            {
                head.localPosition = Vector3.zero;
                head.localRotation = Quaternion.identity;
            }

#else
            if (state)
            {
                oPos = Player.prop_Player_0.transform.position;
                oRot = Player.prop_Player_0.transform.rotation;
            } 
            else
            {
                Player.prop_Player_0.transform.position = oPos;
                Player.prop_Player_0.transform.rotation = oRot;
            }

            if (serializer is null)
                serializer = Player.prop_Player_0.GetComponent<FlatBufferNetworkSerializer>();
            serializer.enabled = !state;

#endif
            {
            if (KiraiLib.UI.elements.TryGetValue(Utils.CreateID("noclip", (int)Shared.PageIndex.toggles1), out KiraiLib.UI.UIElement element))
                {
                    KiraiLib.UI.Toggle toggle = element as KiraiLib.UI.Toggle;

                    if (state)
                    {
                        oNoclip = toggle.state;
                        toggle.SetState(
#if FREECAM_LEGACY
                            false
#else
                            true
#endif
                            );
                    }
                    else
                    {
                        toggle.SetState(oNoclip);
                        oNoclip = false;
                    }
                }
            }

#if FREECAM_LEGACY
            {
                if (KiraiLib.UI.elements.TryGetValue(Utils.CreateID("flight", (int)Shared.modules.flight.info[0].page), out KiraiLib.UI.UIElement element))
                {
                    KiraiLib.UI.Toggle toggle = element as KiraiLib.UI.Toggle;

                    if (state)
                    {
                        oFlight = toggle.state;
                        toggle.SetState(false);
                    }
                    else
                    {
                        toggle.SetState(oFlight);
                        oFlight = false;
                    }
                }
            }
#endif
        }

#if FREECAM_LEGACY
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

            if (camera is null)
                camera = VRCVrCamera.field_Private_Static_VRCVrCamera_0.GetComponentInChildren<Camera>().transform;

            Transform head = VRCVrCamera.field_Private_Static_VRCVrCamera_0.transform.parent;

            head.position += camera.right * Shared.modules.flight.speed * Time.deltaTime * x * (Input.GetKey(KeyCode.LeftShift) ? 8 : 1);
            head.position += camera.forward * Shared.modules.flight.speed * Time.deltaTime * z * (Input.GetKey(KeyCode.LeftShift) ? 8 : 1);
            head.position += camera.up * Shared.modules.flight.speed * Time.deltaTime * y * (Input.GetKey(KeyCode.LeftShift) ? 8 : 1);
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
#else
        public override void OnLevelWasLoaded() => serializer = null;
#endif
    }
}
