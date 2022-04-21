using MelonLoader;
using System;
using System.Linq;
using System.Reflection;

namespace KiraiMod
{
    public static class ModuleLoader
    {
        private static Type[] modules;

        public static void Initialize()
        {
            modules = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == "KiraiMod.Modules")
                .Where(t => t.IsAbstract && t.IsSealed)
                .ToArray();

#if DEBUG
            MelonLogger.Msg($"Found {modules.Length} modules");
#endif


        }
    }
}
