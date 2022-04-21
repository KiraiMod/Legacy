using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod
{
    public class KiraiTools : MelonMod
    {
        private AssetBundle resources;

        public bool active = false;

        private GameObject wand;

        public override void OnApplicationStart()
        {
            System.IO.Stream stream = Assembly.GetManifestResourceStream("KiraiTools.kiraitools.assetbundle");
            System.IO.MemoryStream mem = new System.IO.MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            resources = AssetBundle.LoadFromMemory(mem.ToArray(), 0);
            resources.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }

        public override void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.Q)) Toggle();
            }
        }

        private void Toggle()
        {
            if (active) Disable();
            else Enable();

            active ^= true;
        }

        private void Enable()
        {
            if (wand == null)
            {
                wand = Object.Instantiate(resources.LoadAsset_Internal("assets/wireframe wand.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>());
            }

            wand.transform.position = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightHand);
            wand.active = true;
        }

        private void Disable()
        {
            wand.active = false;
        }
    }
}
