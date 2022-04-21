using MelonLoader;

namespace KiraiMod
{
    public class Config
    {
        public static readonly string path = "kiraimod.config.json";
        Options options = new Options();

        public void Save()
        {
            MelonModLogger.Log("Saving to config");
            System.IO.File.WriteAllText(path, MelonLoader.TinyJSON.JSON.Dump(options));
        }

        public void Load()
        {
            MelonModLogger.Log("Loading from config");

            if (!System.IO.File.Exists(path))
            {
                MelonModLogger.Log("Config did not exist, creating new one with current values");
                System.IO.File.WriteAllText(path, MelonLoader.TinyJSON.JSON.Dump(options));
            }

            options = MelonLoader.TinyJSON.JSON.Load(System.IO.File.ReadAllText(path)).Make<Options>();
        }

        sealed internal class Options
        {
            public bool bKOS;
            public bool bInfinite;
            public bool bModLog;
            public bool bNameplates;
            public bool bNameplatesRGB;

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
                MoveEx(load, ref Shared.modules.kos.state,        ref bKOS,            "p0/auto-kos"        );
                MoveEx(load, ref Shared.modules.portal.state,     ref bInfinite,       "p0/infinite-portals");
                MoveEx(load, ref Shared.modules.modlog.state,     ref bModLog,         "p1/mod-log"         );
                MoveEx(load, ref Shared.modules.nameplates.state, ref bNameplates,     "p1/nameplates"      );
                MoveEx(load, ref Shared.modules.nameplates.rgb,   ref bNameplatesRGB,  "p1/nameplate-rgb"   );

                MoveEx(load, ref Shared.modules.speed.speedRun,   ref fRun,            "p3/walk-speed"      );
                MoveEx(load, ref Shared.modules.speed.speedWalk,  ref fWalk,           "p3/run-speed"       );
                MoveEx(load, ref Shared.modules.flight.speed,     ref fFly,            "p3/flight-speed"    );
                MoveEx(load, ref Shared.modules.portal.distance,  ref fPortalDistance, "p3/portal-distance" );
                MoveEx(load, ref Shared.modules.orbit.speed,      ref fOrbitSpeed,     "p3/orbit-speed"     );
                MoveEx(load, ref Shared.modules.orbit.distance,   ref fOrbitDistance,  "p3/orbit-distance"  );
                MoveEx(load, ref Utils.fRGBSpeed,                 ref fRGBSpeed,       "p3/rgb-speed"       );
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

            public void MoveEx(bool load, ref float prop1, ref float prop2, string id)
            {
                if (true || Shared.menu == null) Move(load, ref prop1, ref prop2); 
                else if (load) { if (!Shared.menu.Set(id, prop2)) prop1 = prop2; }
                else prop2 = prop1;
            }

            public void MoveEx(bool load, ref bool prop1, ref bool prop2, string id)
            {
                if (true || Shared.menu == null) Move(load, ref prop1, ref prop2);
                else if (load) { if (!Shared.menu.Set(id, prop2)) prop1 = prop2; }
                else prop2 = prop1;
            }
        }
    }
}
