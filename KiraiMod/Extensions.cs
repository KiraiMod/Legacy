using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase;

namespace KiraiMod
{
    public static class Extensions
    {
        public static bool IsFriend(this Player player)
        {
            if (player.field_Private_APIUser_0 == null) return false;

            return player.IsLocal() ||
                APIUser.CurrentUser.friendIDs.Contains(player.field_Private_APIUser_0.id);
        }

        public static bool IsFavorite(this Player player)
        {
            if (player.field_Private_APIUser_0 == null) return false;

            return player.IsLocal() || APIUser.CurrentUser.IsFavorite(player.field_Private_APIUser_0.id);
        }

        public static bool IsKOS(this APIUser player)
        {
            return Shared.modules.kos.kosList.Contains(Utils.SHA256(player.displayName));
        }

        public static bool IsStreamer(this APIUser player)
        {
            return Shared.modules.kos.streamers.Contains(Utils.SHA256(player.displayName));
        }

        public static bool IsMaster(this Player player)
        {
            return player.field_Private_VRCPlayerApi_0.isMaster;
        }

        public static bool IsLocal(this Player player)
        {
            return player.field_Private_APIUser_0.id == APIUser.CurrentUser.id;
        }

        public static bool IsLocal(this APIUser player)
        {
            return player.id == APIUser.CurrentUser.id;
        }

        public static bool IsMod(this APIUser player)
        {
            return player.hasModerationPowers || player.displayName == "F5iVeS";
        }

        public static bool IsKModder(this Player player)
        {
            return Shared.modules.nameplates.kmodders.Contains(player.field_Private_APIUser_0.displayName);
        }

        public static bool IsCModder(this Player player)
        {
            return Shared.modules.nameplates.cmodders.Contains(player.field_Private_APIUser_0.displayName);
        }

#if BETA
        public static bool IsFModder(this Player player)
        {
            return Shared.modules.nameplates.fmodders.Contains(player.field_Private_APIUser_0.id);
        }
#endif

        public static Color GetNameplateColor(this Player player)
        {
            return player.field_Private_APIUser_0.IsKOS() ? Utils.Colors.red :
                player.IsFriend() ? Utils.Colors.highlight :
                Utils.Colors.primary;
        }

        public static Color GetTextColor(this Player player)
        {
            return player.field_Private_APIUser_0.IsMod() ? Utils.Colors.aqua : 
                player.IsKModder() ? Utils.Colors.highlight : 
                Utils.Colors.white;
        }
    }
}
