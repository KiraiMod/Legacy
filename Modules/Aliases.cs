namespace KiraiMod.Modules
{
    public class Aliases : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("Name Aliases", "Spoofs name from KiraiMod.alias.json", ButtonType.Toggle, 4, 0, nameof(state))
        };

        public static void ProcessString(ref string __result)
        {
            if (!Shared.modules.aliases.state) return;

            foreach (string[] alias in Shared.config.aliases) __result = __result.Replace(alias[0], alias[1]);
        }
    }
}
