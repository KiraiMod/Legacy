namespace KiraiMod.Modules
{
    public class Aliases : ModuleBase
    {
        public string self;

        public new ModuleInfo[] info = {
            new ModuleInfo("Name Aliases", "Spoofs name from KiraiMod.alias.json", ButtonType.Toggle, 5, Shared.PageIndex.toggles2, nameof(state))
        };

        public static void ProcessString(ref string __result)
        {
            if (!Shared.modules.aliases.state) return;

            foreach (string[] alias in Shared.config.aliases) __result = __result.Replace(alias[0], alias[1]);
        }

        public static void ProcessStringPrefix(ref string __0)
        {
            if (!Shared.modules.aliases.state || __0 is null) return;

            foreach (string[] alias in Shared.config.aliases) __0 = __0.Replace(alias[0], alias[1]);
        }
    }
}
