﻿using MelonLoader;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KiraiMod
{
    public class Config
    {
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
                MessageBox.Show("Join discord.gg/jsuHmcK", "KiraiMod first time config setup");
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

            public bool bModLog;
            public bool bNameplates;
            public bool bNameplatesRGB;
            public bool bHeadlight;
            public bool bAliases;
            public bool bDirectionalFlight;

            public float fRun;
            public float fWalk;
            public float fFly;
            public float fPortalDistance;
            public float fOrbitSpeed;
            public float fOrbitDistance;
            public float fRGBSpeed;

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

                Move(load, ref Shared.modules.speed.speedRun,     ref fRun              );
                Move(load, ref Shared.modules.speed.speedWalk,    ref fWalk             );
                Move(load, ref Shared.modules.flight.speed,       ref fFly              );
                Move(load, ref Shared.modules.portal.distance,    ref fPortalDistance   );
                Move(load, ref Shared.modules.orbit.speed,        ref fOrbitSpeed       );
                Move(load, ref Shared.modules.orbit.distance,     ref fOrbitDistance    );
                Move(load, ref Utils.fRGBSpeed,                   ref fRGBSpeed         );

                if (load)
                {
                    Shared.modules.OnConfigLoaded();
                }
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