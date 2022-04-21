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

        public static Config config;
        public static Modules.Modules modules;
        public static Hooks hooks;
        public static HarmonyInstance harmony;
        public static IPC ipc;

        public static class Options
        {
            public static bool bWorldTriggers;
        }

        public static bool unloaded;

        public static System.Collections.Generic.List<int> PageRemap = new System.Collections.Generic.List<int>();

        public enum PageIndex
        {
            toggles1,
            toggles2,
            toggles3,
            buttons1,
            buttons2,
            sliders1,
            xutils,
            udon1,
            udon2,
            userinfo
        }
    }
}