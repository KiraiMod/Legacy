using Harmony;
using UnityEngine;
using VRC;

namespace KiraiMod
{
    public static class Shared
    {
        public static Player targetPlayer;
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