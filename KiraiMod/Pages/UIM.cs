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
        }
    }
}