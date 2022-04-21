using MelonLoader;
using System.Collections.Generic;

namespace KiraiMod
{
    public class Config
    {
        public static readonly string config = "kiraimod.config.json";
        public static readonly string alias = "kiraimod.alias.json";
        public List<string[]> aliases = new List<string[]>();
        Options options = new Options();

        public void Save()
        {
            MelonModLogger.Log("Saving to config");
            System.IO.File.WriteAllText(config, MelonLoader.TinyJSON.JSON.Dump(options));
        }

        public void Load()
        {
            MelonModLogger.Log("Loading from config");

            if (!System.IO.File.Exists(config))
            {
                MelonModLogger.Log("Config did not exist, creating new one with current values");
                System.IO.File.WriteAllText(config, MelonLoader.TinyJSON.JSON.Dump(options));
            }

            options = MelonLoader.TinyJSON.JSON.Load(System.IO.File.ReadAllText(config)).Make<Options>();

            if (System.IO.File.Exists(alias)) aliases = MelonLoader.TinyJSON.JSON.Load(System.IO.File.ReadAllText(alias)).Make<List<string[]>>();
        }

        sealed internal class Options
        {
            public bool bKOS;
            public bool bInfinite;
            public bool bModLog;
            public bool bNameplates;
            public bool bNameplatesRGB;
            public bool bHeadlight;
            public bool bAliases;

            public float fRun;
            public float fWalk;
            public float fFly;
            public float fPortalDistance;
            public float fOrbitSpeed;
            public float fOrbitDistance;
            public float fRGBSpeed;

            [MelonLoader.TinyJSON.BeforeEncode]
            public void BeforeEncode()
            {
                Set(false);
            }

            [MelonLoader.TinyJSON.AfterDecode]
            public void AfterDecode()
            {
                Set(true);
            }

            public void Set(bool load)
            {
                Move(load, ref Shared.modules.kos.state,        ref bKOS           );
                Move(load, ref Shared.modules.portal.state,     ref bInfinite      );
                Move(load, ref Shared.modules.modlog.state,     ref bModLog        );
                Move(load, ref Shared.modules.nameplates.state, ref bNameplates    );
                Move(load, ref Shared.modules.nameplates.rgb,   ref bNameplatesRGB );
                Move(load, ref Shared.modules.headlight.state,  ref bHeadlight     );
                Move(load, ref Shared.modules.aliases.state,    ref bAliases       );

                Move(load, ref Shared.modules.speed.speedRun,   ref fRun           );
                Move(load, ref Shared.modules.speed.speedWalk,  ref fWalk          );
                Move(load, ref Shared.modules.flight.speed,     ref fFly           );
                Move(load, ref Shared.modules.portal.distance,  ref fPortalDistance);
                Move(load, ref Shared.modules.orbit.speed,      ref fOrbitSpeed    );
                Move(load, ref Shared.modules.orbit.distance,   ref fOrbitDistance );
                Move(load, ref Utils.fRGBSpeed,                 ref fRGBSpeed      );

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
        }
    }
}
