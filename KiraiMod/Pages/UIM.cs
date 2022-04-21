using MelonLoader;

namespace KiraiMod.Pages
{
    public class UIM
    {
        public UIM()
        {
            KiraiLib.UI.Button.Create("uim/portal", "Target", "Sets the selected player as the current target.", 0f, -3f, KiraiLib.UI.UserInteractMenu.transform, new System.Action(() =>
            {
                Shared.TargetPlayer = Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName);
            }));

            KiraiLib.UI.Button.Create("uim/info", "View\nInfo", "View info about this player", 2, -1, KiraiLib.UI.UserInteractMenu.transform, new System.Action(() =>
            {
                VRC.Player player = Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName);

                MelonLogger.Msg(System.ConsoleColor.Blue, QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName);
                KiraiLib.Logger.Log($"<color=#00f>{QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName}</color>", 10);
                LogGroup("  User Info");
                LogBody($"    ID: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id}");
                LogBody($"    Original Name: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.username}");
                LogBody($"    VR: {player?.field_Private_VRCPlayerApi_0?.IsUserInVR()}");
                LogBody($"    Rank: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.GetTrustLevel()}");
                LogBody($"    VRC+: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.isSupporter}");
                LogBody($"    Early VRC+: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.isEarlyAdopter}");
                LogBody($"    Platform: {QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.last_platform}");
                LogGroup("  Avatar Info");
                LogBody($"    Name: {player?.prop_ApiAvatar_0?.name}");
                LogBody($"    ID: {player?.prop_ApiAvatar_0?.id}");
                LogGroup("    Author Info");
                LogBody($"      Name: {player?.prop_ApiAvatar_0?.authorName}");
                LogBody($"      ID: {player?.prop_ApiAvatar_0?.authorId}");
            }));

            KiraiLib.UI.Button.Create("uim/local-clone", "Local\nClone", "Copy their current avatar to your local list", 3, -3, KiraiLib.UI.UserInteractMenu.transform, new System.Action(() =>
            {
                VRC.Player player = Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.displayName);
                Shared.http.GetAsync(player.prop_ApiAvatar_0.assetUrl)
                    .ContinueWith(new System.Action<System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>>(async (resp) =>
                    {
                        System.IO.File.WriteAllBytes($"{System.Environment.GetEnvironmentVariable("TEMP")}/../../LocalLow/vrchat/vrchat/avatars/{player.prop_ApiAvatar_0.name}.vrca", await resp.Result.Content.ReadAsByteArrayAsync());
                        KiraiLib.Logger.Log($"<color=#ccf>{player.prop_ApiAvatar_0.name}</color> saved to <color=#ccf>Other</color> list", 7);
                    }));
            }));
        }

        private void LogGroup(string a)
        {
            MelonLogger.Msg(System.ConsoleColor.Green, a);
            KiraiLib.Logger.Log($"<color=#0f0>{a}</color>", 10);
        }

        private void LogBody(string a)
        {
            MelonLogger.Msg(System.ConsoleColor.Cyan, a);
            KiraiLib.Logger.Log($"<color=#0ff>{a}</color>", 10);
        }
    }
}