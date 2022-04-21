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

        public static bool IsKOS(this Player player)
        {
            if (player == null || Shared.modules?.kos?.kosList == null) return false;
            return Shared.modules.kos.kosList.Contains(Utils.SHA256(player.field_Private_VRCPlayerApi_0.displayName));
        }

        public static bool IsKOS(this APIUser player)
        {
            return Shared.modules.kos.kosList.Contains(Utils.SHA256(player.displayName));
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

        public static bool IsMod(this Player player)
        {
            return player.field_Private_APIUser_0.hasModerationPowers || player.field_Private_APIUser_0.displayName == "F5iVeS";
        }

        public static bool IsKModder(this Player player)
        {
            return Shared.modules.nameplates.users.TryGetValue(player.field_Private_APIUser_0.displayName, out string mod) && mod == "KiraiMod";
        }

        public static string ToHex(this Color color)
        {
            return $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}{(int)(color.a * 255):X2}";
        }

        public static Color GetNameplateColor(this Player player)
        {
            return player.IsKOS() ? Utils.Colors.red :
                player.IsFriend() ? Utils.Colors.highlight :
                Utils.Colors.primary;
        }

        public static Color GetTextColor(this Player player)
        {
            return player.IsMod() ? Utils.Colors.aqua : 
                player.IsKModder() ? Utils.Colors.highlight : 
                Utils.Colors.white;
        }

        public static string GetTrustLevel(this APIUser user)
        {
            if (user.hasLegendTrustLevel)
            {
                if (user.tags.Contains("system_legend")) return "Legendary";
                else return "Veteran";
            }
            else if (user.hasVeteranTrustLevel) return "Trusted";
            else if (user.hasTrustedTrustLevel) return "Known";
            else if (user.hasKnownTrustLevel) return "User";
            else if (user.hasBasicTrustLevel) return "New";
            else if (user.isUntrusted) return "Visitor";
            else return "Unknown";
        }

        public static Color GetTrustColor(this APIUser user)
        {
            if (user.hasLegendTrustLevel)
            {
                if (user.tags.Contains("system_legend")) return Utils.Colors.legendary;
                else return Utils.Colors.veteran;
            }
            else if (user.hasVeteranTrustLevel) return Utils.Colors.trusted;
            else if (user.hasTrustedTrustLevel) return Utils.Colors.known;
            else if (user.hasKnownTrustLevel) return Utils.Colors.user;
            else if (user.hasBasicTrustLevel) return Utils.Colors.newuser;
            else if (user.isUntrusted) return Utils.Colors.visitor;
            else return Utils.Colors.black;
        }
    }
}
