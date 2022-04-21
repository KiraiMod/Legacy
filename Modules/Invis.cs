using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;

namespace KiraiMod.Modules
{
    public class Invis : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("Invis", "Makes your avatar invisible", ButtonType.Toggle, 8, 0, nameof(state))
        };

        public override void OnStateChange(bool state)
        {
            Apply();
        }

        public override void OnAvatarInitialized(GameObject avatar, VRCAvatarManager manager)
        {
            if (state && avatar.GetComponentInParent<VRCPlayer>().field_Private_Player_0.IsLocal()) MelonCoroutines.Start(DelayedApply());
        }

        public IEnumerator DelayedApply()
        {
            yield return null;
            Apply();
        }

        public void Apply()
        {
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0?.gameObject == null) return;

            VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.gameObject.SetActive(!state);
        }
    }
}
