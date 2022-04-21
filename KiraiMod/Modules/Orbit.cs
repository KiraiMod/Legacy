using UnityEngine;

namespace KiraiMod.Modules
{
    public class Orbit : ModuleBase
    {
        public float distance = 1;
        public float speed = 2;

        private readonly Vector3 hideSelfOffset = new Vector3(0, 4, 0);

        public new ModuleInfo[] info = {
            new ModuleInfo("Orbit", "Orbits the selected player", ButtonType.Toggle, 7, Shared.PageIndex.toggles1, nameof(state)),
            new ModuleInfo("Orbit Speed", ButtonType.Slider, 3, Shared.PageIndex.sliders1, nameof(speed), 0, 8),
            new ModuleInfo("Orbit Distance", ButtonType.Slider, 4, Shared.PageIndex.sliders1, nameof(distance), 0, 4)
        };

        public override void OnUpdate()
        {
            if (!state) return;

            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            if (x < -0.1f || x > 0.1f || y < -0.1f || y > 0.1f)
            {
                (KiraiLib.UI.elements[Utils.CreateID("Orbit", (int)Shared.PageIndex.toggles1)] as KiraiLib.UI.Toggle)?.SetState(false);

                SetState(false);
                return;
            }

            if (Shared.TargetPlayer == null) return;

            GameObject puppet = new GameObject();

            Vector3 pos = Shared.modules.misc.bAnnoyance
                ? Shared.TargetPlayer.field_Private_VRCPlayerApi_0.GetBonePosition(HumanBodyBones.Head)
                : Shared.TargetPlayer.transform.position;

            if (Shared.modules.hideself.state)
                pos -= hideSelfOffset;

            puppet.transform.position = pos;
            puppet.transform.Rotate(new Vector3(0, 1, 0), Time.time * speed * 90);

            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position = puppet.transform.position + (puppet.transform.forward * distance);
            Object.Destroy(puppet);
        }
    }
}
