using VRC;

namespace KiraiMod.Modules
{
    public class Aliases : ModuleBase
    {
        public string self;

        public new ModuleInfo[] info = {
            new ModuleInfo("Name Aliases", "Spoofs name from KiraiMod.alias.json", ButtonType.Toggle, 5, Menu.PageIndex.options2, nameof(state))
        };

        public static void ProcessString(ref string __result)
        {
            if (!Shared.modules.aliases.state) return;

            foreach (string[] alias in Shared.config.aliases) __result = __result.Replace(alias[0], alias[1]);
        }

        public override void OnStateChange(bool state)
        {
            //if (state) GetSelfAlias();
        }

        public override void OnLevelWasLoaded()
        {
            //if (state) GetSelfAlias();
        }

        public void GetSelfAlias()
        {
            if (Player.prop_Player_0?.field_Private_APIUser_0 == null) return;

            foreach (string[] alias in Shared.config.aliases)
            {
                if (alias[0] == Player.prop_Player_0.field_Private_APIUser_0.displayName)
                    self = alias[1];
                    return;
            }
        }
    }
}
