using UnityEngine;

namespace KiraiMod.Modules
{
    public class Orbit : ModuleBase
    {
        public float distance = 1;
        public float speed = 2;

        public new ModuleInfo[] info = {
            new ModuleInfo("Orbit", "Orbits the selected player", ButtonType.Toggle, 7, Menu.PageIndex.toggles1, nameof(state)),
            new ModuleInfo("Orbit Speed", ButtonType.Slider, 3, Menu.PageIndex.sliders1, nameof(speed), 0, 8),
            new ModuleInfo("Orbit Distance", ButtonType.Slider, 4, Menu.PageIndex.sliders1, nameof(distance), 0, 4)
        };

        public override void OnUpdate()
        {
            if (!state) return;

            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            if (x < -0.1f || x > 0.1f || y < -0.1f || y > 0.1f)
            {
                if (Shared.menu.objects.TryGetValue(Utils.CreateID("Orbit", (int)Menu.PageIndex.toggles1), out Menu.MenuObject obj))
                    obj.toggle.SetState(false);
                SetState(false);
                return;
            }

            if (Shared.TargetPlayer == null) return;

            GameObject puppet = new GameObject();

            if (Shared.modules.misc.bAnnoyance)
                puppet.transform.position = Shared.TargetPlayer.field_Private_VRCPlayerApi_0.GetBonePosition(HumanBodyBones.Head);
            else
                puppet.transform.position = Shared.TargetPlayer.transform.position;
            puppet.transform.Rotate(new Vector3(0, 1, 0), Time.time * speed * 90);
            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position = puppet.transform.position + (puppet.transform.forward * distance);
            Object.Destroy(puppet);
        }
    }
}
