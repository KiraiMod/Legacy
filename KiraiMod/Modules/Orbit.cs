using UnityEngine;

namespace KiraiMod.Modules
{
    public class Orbit : ModuleBase
    {
        public float distance = 1;
        public float speed = 2;

        public new ModuleInfo[] info = { 
            new ModuleInfo("Orbit", "Orbits the selected player", ButtonType.Toggle, 7, 0, nameof(state))
        };

        public override void OnUpdate()
        {
            if (!state) return;

            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            if (x < -0.1f || x > 0.1f || y < -0.1f || y > 0.1f)
            {
                if (Shared.menu.objects.TryGetValue(Utils.CreateID("Orbit", 0), out Menu.MenuObject obj))
                    obj.toggle.SetState(false);
                SetState(false);
                return;
            }

            if (Shared.targetPlayer == null) return;

            GameObject puppet = new GameObject();
            puppet.transform.position = Shared.targetPlayer.transform.position;
            puppet.transform.Rotate(new Vector3(0, 1, 0), Time.time * speed * 90);
            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position = puppet.transform.position + (puppet.transform.forward * distance);
            Object.Destroy(puppet);
        }
    }
}
