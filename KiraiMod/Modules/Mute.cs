using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;

namespace KiraiMod.Modules
{
    public class Mute : ModuleBase
    {
        public bool Friends = true;
        public bool Targeted = false;
        public bool Favorited = false;

        public new ModuleInfo[] info = {
            new ModuleInfo("Mute Self", "Mutes yourself for certain people", ButtonType.Toggle, 6, Menu.PageIndex.options1, nameof(state)),
            new ModuleInfo("Mute Self Friends", "Only friends can hear you", ButtonType.Toggle, 8, Menu.PageIndex.options2, nameof(Friends)),
            new ModuleInfo("Mute Self Targeted", "Only your targeted player can hear you", ButtonType.Toggle, 9, Menu.PageIndex.options2, nameof(Targeted)),
            new ModuleInfo("Mute Self Favorited", "Only favorited friends can hear you", ButtonType.Toggle, 10, Menu.PageIndex.options2, nameof(Favorited))
        };

        public override void OnStateChange(bool state)
        {
            Refresh();
        }

        public override void OnConfigLoaded()
        {
            Refresh();
        }

        public override void OnAvatarInitialized(GameObject avatar, VRCAvatarManager manager)
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
                Apply(PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[i]);
        }

        public IEnumerator DelayedRefresh()
        {
            yield return new WaitForSeconds(1);
            Refresh();
        }

        public void Apply(Player player)
        {
            //if (player.field_Internal_VRCPlayer_0 == null) return;

            //player.field_Internal_VRCPlayer_0.prop_Boolean_9 = !state;

            //if (state)
            //{
            //    if (Friends)
            //    {
            //        if (player.IsFriend())
            //            player.field_Internal_VRCPlayer_0.field_Private_Boolean_3 = true;
            //    }
            //    else if (Targeted)
            //    {
            //        if (player == Shared.targetPlayer)
            //            player.field_Internal_VRCPlayer_0.field_Private_Boolean_3 = true;
            //    }
            //    else if (Favorited)
            //    {
            //        if (player.IsFavorite())
            //            player.field_Internal_VRCPlayer_0.field_Private_Boolean_3 = true;
            //    }
            //    else MelonLogger.Log("Invalid state @ Mute::Apply");
            //}
        }

        public void OnStateChangeFriends(bool state)
        {
            if (state)
            {
                DisableOthers(0);
                Refresh();
            }
            else ReenableDefault();
        }

        public void OnStateChangeTargeted(bool state)
        {
            if (state)
            {
                DisableOthers(1);
                Refresh();
            }
            else ReenableDefault();
        }

        public void OnStateChangeFavorited(bool state)
        {
            if (state)
            {
                DisableOthers(2);
                Refresh();
            }
            else ReenableDefault();
        }

        private void DisableOthers(int target)
        {
            if (target > 0)
                Shared.menu.Set("p1/mute-self-friends", false);

            if (target != 1)
                Shared.menu.Set("p1/mute-self-targeted", false);

            if (target < 2)
                Shared.menu.Set("p1/mute-self-favorited", false);
        }

        private void ReenableDefault()
        {
            if (Friends || Targeted || Favorited) return;

            Friends = true;
            Shared.menu.Set("p1/mute-self-friends", true);
        }
    }
}
