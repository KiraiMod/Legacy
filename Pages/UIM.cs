using MelonLoader;
using System.Linq;

namespace KiraiMod.Pages
{
    public class UIM
    {
        public UIM()
        {
            Shared.menu.CreateButton("uim/portal", "Target", "Sets the selected player as the current target.", 0f, -3f, Shared.menu.um.transform, new System.Action(() =>
            {
                Shared.targetPlayer = Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id);
            }));

            if (Main.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("AviFav+")))
            {
                Shared.menu.CreateButton("uim/save-to-avifav+", "Save to AviFav+", "Requires you to have AviFav+ by 404 installed", 2f, -3f, Shared.menu.um.transform, new System.Action(() =>
                {
                    AviFav_.AvatarListHelper.AvatarListPassthru(
                        Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id).field_Private_VRCAvatarManager_0.field_Private_ApiAvatar_0);
                }));
            }
            else MelonModLogger.Log("Failed to find AviFav+, Not creating UI element");
        }
    }
}