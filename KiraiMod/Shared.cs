using Harmony;
using VRC;

namespace KiraiMod
{
    public static class Shared
    {
        private static Player targetPlayer;

        public static Player TargetPlayer { 
            get => targetPlayer; 
            set { 
                targetPlayer = value;
                modules.nameplates.Refresh();
            } 
        }

        public static Menu menu;
        public static Config config;
        public static Modules.Modules modules;
        public static Hooks hooks;
        //public static AssetBundle resources;
        public static HarmonyInstance harmony;
        public static IPC ipc;

        public static class Options
        {
            public static bool bWorldTriggers;
        }
    }
}