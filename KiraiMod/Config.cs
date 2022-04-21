using MelonLoader;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KiraiMod
{
    public class Config
    {
        public class General
        {
            public static bool bPersistantQuickMenu = false;
            public static float fRGBSpeed = 1f;
        }

        private Options options = new Options();

        public static readonly string config = "kiraimod.config.json";
        public static readonly string alias = "kiraimod.alias.json";
        public static readonly string menu = "kiraimod.buttons.json";
        public List<string[]> aliases = new List<string[]>();
        public Dictionary<string, List<float>> buttons = new Dictionary<string, List<float>>();

        public void Save()
        {
            MelonLogger.Log("Saving to config");
            System.IO.File.WriteAllText(config, JSON.Dump(options));
        }

        public void Load()
        {
            MelonLogger.Log("Loading from config");

            if (!System.IO.File.Exists(config))
            {
                MelonLogger.Log("Config did not exist, creating new one with current values");
                //MessageBox.Show("Join discord.gg/jsuHmcK", "KiraiMod first time config setup");
                System.IO.File.WriteAllText(config, JSON.Dump(options));
            }

            options = JSON.Load(System.IO.File.ReadAllText(config)).Make<Options>();

            if (System.IO.File.Exists(alias)) aliases = JSON.Load(System.IO.File.ReadAllText(alias)).Make<List<string[]>>();
            if (System.IO.File.Exists(menu)) buttons = JSON.Load(System.IO.File.ReadAllText(menu)).Make<Dictionary<string, List<float>>>();
        }

        internal sealed class Options
        {
            public bool bInfinitePortals;
            public bool bAutoKOS;

            public bool bESP;
            public bool bModLog;
            public bool bNameplates;
            public bool bNameplatesRGB;
            public bool bHeadlight;
            public bool bAliases;
            public bool bDirectionalFlight;
            public bool bPlayerList;
            public bool bUseClipboard;
            public bool bPersistantQM;

            public bool bMuteSelfFriends;
            public bool bMuteSelfTargeted;
            public bool bMuteSelfFavorited;

            public bool bTracerPlayers;
            public bool bTracerPickups;
            public bool bTracerTriggers;

            public float fRun;
            public float fWalk;
            public float fFly;
            public float fPortalDistance;
            public float fOrbitSpeed;
            public float fOrbitDistance;
            public float fRGBSpeed;
            public float fItemOrbitSize;
            public float fItemOrbitSpeed;

            [BeforeEncode]
            public void BeforeEncode()
            {
                Set(false);
            }

            [AfterDecode]
            public void AfterDecode()
            {
                Set(true);
            }

            public void Set(bool load)
            {
                Move(load, ref Shared.modules.portal.infinite,    ref bInfinitePortals  );
                Move(load, ref Shared.modules.kos.state,          ref bAutoKOS          );
                Move(load, ref Shared.modules.modlog.state,       ref bModLog           );
                Move(load, ref Shared.modules.nameplates.state,   ref bNameplates       );
                Move(load, ref Shared.modules.nameplates.RGB,     ref bNameplatesRGB    );
                Move(load, ref Shared.modules.headlight.state,    ref bHeadlight        );
                Move(load, ref Shared.modules.aliases.state,      ref bAliases          );
                Move(load, ref Shared.modules.flight.directional, ref bDirectionalFlight);
                Move(load, ref Shared.modules.esp.state,          ref bESP              );
                Move(load, ref Shared.modules.mute.Friends,       ref bMuteSelfFriends  );
                Move(load, ref Shared.modules.mute.Targeted,      ref bMuteSelfTargeted );
                Move(load, ref Shared.modules.mute.Favorited,     ref bMuteSelfFavorited);
                Move(load, ref Shared.modules.tracers.Players,    ref bTracerPlayers    );
                Move(load, ref Shared.modules.tracers.Pickups,    ref bTracerPickups    );
                Move(load, ref Shared.modules.tracers.Triggers,   ref bTracerTriggers   );
                Move(load, ref Shared.modules.playerlist.state,   ref bPlayerList       );

                Move(load, ref Shared.modules.speed.SpeedRun,     ref fRun              );
                Move(load, ref Shared.modules.speed.SpeedWalk,    ref fWalk             );
                Move(load, ref Shared.modules.flight.speed,       ref fFly              );
                Move(load, ref Shared.modules.portal.distance,    ref fPortalDistance   );
                Move(load, ref Shared.modules.orbit.speed,        ref fOrbitSpeed       );
                Move(load, ref Shared.modules.orbit.distance,     ref fOrbitDistance    );
                Move(load, ref Shared.modules.itemOrbit.speed,    ref fItemOrbitSize    );
                Move(load, ref Shared.modules.itemOrbit.size,     ref fItemOrbitSpeed   );

                Move(load, ref Shared.modules.misc.bUseClipboard, ref bUseClipboard     );
                Move(load, ref General.bPersistantQuickMenu,      ref bPersistantQM     );
                Move(load, ref General.fRGBSpeed,                 ref fRGBSpeed         );

                if (load) Shared.modules.OnConfigLoaded();
            }

            public void Move(bool load, ref float prop1, ref float prop2)
            {
                if (load) prop1 = prop2;
                else      prop2 = prop1;
            }

            public void Move(bool load, ref bool prop1, ref bool prop2)
            {
                if (load) prop1 = prop2;
                else      prop2 = prop1;
            }

            public void LOSS(bool load, Modules.ModuleBase module, bool state) // Load Only SetState
            {
                if (load) module.SetState(state);
                else return;
            }
        }
    }
}
