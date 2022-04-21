using MelonLoader;

namespace KiraiMod.Pages
{
    public class UIM
    {
        public UIM()
        {
            Shared.menu.CreateButton("uim/portal", "Target", "Sets the selected player as the current target.", 0f, -3f, Shared.menu.um.transform, new System.Action(() =>
            {
                Shared.TargetPlayer = Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id);
            }));

            Shared.menu.CreateButton("uim/info", "View\nInfo", "View info about this player", 2, -1, Shared.menu.um.transform, new System.Action(() =>
            {
                KiraiLib.Logger.Log("View the console for information");

                VRC.Player player = Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id);

                MelonLogger.Log(System.ConsoleColor.Blue, QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName);
                MelonLogger.Log(System.ConsoleColor.Green, "  User Info");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    ID: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    Original Name: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.username}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    VR: {player?.field_Private_VRCPlayerApi_0?.IsUserInVR()}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    Rank: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.GetTrustLevel()}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    VRC+: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.isSupporter}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    Early VRC+: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.isEarlyAdopter}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    Platform: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.last_platform}");
                MelonLogger.Log(System.ConsoleColor.Green, "  Avatar Info");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    Name: {player?.prop_ApiAvatar_0?.name}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"    ID: {player?.prop_ApiAvatar_0?.id}");
                MelonLogger.Log(System.ConsoleColor.Green, "    Author Info");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"      Name: {player?.prop_ApiAvatar_0?.authorName}");
                MelonLogger.Log(System.ConsoleColor.Cyan, $"      ID: {player?.prop_ApiAvatar_0?.authorId}");
                MelonLogger.Log(System.ConsoleColor.Green, "  Other Info");
            }));
        }
    }
}