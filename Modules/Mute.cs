using UnityEngine;
using VRC;

namespace KiraiMod.Modules
{
    public class Mute : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("Mute Self", "Mutes yourself for non-friends", ButtonType.Toggle, 6, 0, nameof(state))
        };

        public override void OnStateChange(bool state)
        {
            Refresh();
        }

        public override void OnConfigLoaded()
        {
            Refresh();
        }

        public override void OnAvatarInitialized(VRCAvatarManager manager)
        {
            Refresh();
        }

        public override void OnPlayerJoined(Player player)
        {
            Apply(player);
        }

        public void Refresh()
        {
            if (PlayerManager.field_Private_Static_PlayerManager_0 == null ||
                PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0 == null)
                return;

            for (int i = 0; i < PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count; i++)
            {
                Apply(PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[i]);
            }
        }

        public void Apply(Player player)
        {
            if (player.field_Internal_VRCPlayer_0 == null) return;

            if (!player.IsFriend()) player.field_Internal_VRCPlayer_0.field_Internal_Boolean_3 = !state;
        }
    }
}
