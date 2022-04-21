using MelonLoader;
using MelonLoader.TinyJSON;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KiraiMod
{
    public class Config
    {
        public class General
        {
            public static float fRGBSpeed = 1f;
        }

        private Options options = new Options();

        public static readonly string config = "kiraimod.config.json";
        public static readonly string alias = "kiraimod.alias.json";
        public static readonly string menu = "kiraimod.buttons.json";
        public List<string[]> aliases = new List<string[]>();

        public void Save()
        {
            MelonLogger.Msg("Saving to config");
            System.IO.File.WriteAllText(config, JSON.Dump(options, EncodeOptions.PrettyPrint));
        }

        public void Load()
        {
            MelonLogger.Msg("Loading from config");

            if (!System.IO.File.Exists(config))
            {
                MelonLogger.Msg("Config did not exist, creating new one with current values");
                MessageBox.Show("KiraiMod is not a public mod and redistribution is prohibited.\nIf you did not recieve the mod from the server then you are running an illegitimate copy.", "Copyright Notice");
                System.IO.File.WriteAllText(config, JSON.Dump(options, EncodeOptions.PrettyPrint));
            }

            options = JSON.Load(System.IO.File.ReadAllText(config)).Make<Options>();

            if (System.IO.File.Exists(alias)) aliases = JSON.Load(System.IO.File.ReadAllText(alias)).Make<List<string[]>>();
            if (System.IO.File.Exists(menu)) JSON.Load(System.IO.File.ReadAllText(menu)).Make<Dictionary<string, List<float>>>()
                    .ToList().ForEach(x => KiraiLib.UI.overrides[x.Key] = x.Value.ToArray());
        }

        internal sealed class Options
        {
            public bool bInfinitePortals;
            public bool bAutoKOS;

            public bool bESP;
            public bool bNameplates;
            public bool bNameplatesRGB;
            public bool bAliases;
            public bool bDirectionalFlight;
            public bool bPlayerList;
            public bool bUseClipboard;
            public bool bPersistantQM;

            public bool bBindsNumpad;
            public bool bBindsTab;
            public bool bBindsAlt;
            public bool bAnnoyance;

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
            public float fThrowSpeed;

            public string szWorldCrash = "avtr_f3739df2-b502-4728-ba19-3099772c2de3";

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
                #region bool
                Move(load, ref Shared.modules.portal.infinite,           ref bInfinitePortals  );
                Move(load, ref Shared.modules.kos.state,                 ref bAutoKOS          );
                Move(load, ref Shared.modules.nameplates.state,          ref bNameplates       );
                Move(load, ref Shared.modules.nameplates.RGB,            ref bNameplatesRGB    );
                Move(load, ref Shared.modules.aliases.state,             ref bAliases          );
                Move(load, ref Shared.modules.flight.directional,        ref bDirectionalFlight);
                Move(load, ref Shared.modules.esp.state,                 ref bESP              );
                Move(load, ref Shared.modules.misc.BindsNumpad,          ref bBindsNumpad      );
                Move(load, ref Shared.modules.misc.BindsTab,             ref bBindsTab         );
                Move(load, ref Shared.modules.misc.BindsAlt,             ref bBindsAlt         );
                Move(load, ref Shared.modules.misc.bAnnoyance,           ref bAnnoyance        );
                Move(load, ref Shared.modules.tracers.Players,           ref bTracerPlayers    );
                Move(load, ref Shared.modules.tracers.Pickups,           ref bTracerPickups    );
                Move(load, ref Shared.modules.tracers.Triggers,          ref bTracerTriggers   );
                Move(load, ref Shared.modules.playerlist.state,          ref bPlayerList       );
                Move(load, ref Shared.modules.misc.bUseClipboard,        ref bUseClipboard     );
                Move(load, ref Shared.modules.misc.bPersistantQuickMenu, ref bPersistantQM     );
                #endregion
                #region float
                Move(load, ref Shared.modules.speed.SpeedRun,            ref fRun              );
                Move(load, ref Shared.modules.speed.SpeedWalk,           ref fWalk             );
                Move(load, ref Shared.modules.flight.speed,              ref fFly              );
                Move(load, ref Shared.modules.portal.distance,           ref fPortalDistance   );
                Move(load, ref Shared.modules.orbit.speed,               ref fOrbitSpeed       );
                Move(load, ref Shared.modules.orbit.distance,            ref fOrbitDistance    );
                Move(load, ref Shared.modules.itemOrbit.speed,           ref fItemOrbitSize    );
                Move(load, ref Shared.modules.itemOrbit.size,            ref fItemOrbitSpeed   );
                Move(load, ref General.fRGBSpeed,                        ref fRGBSpeed         );
                Move(load, ref Shared.modules.misc.throwSpeed,           ref fThrowSpeed       );
                #endregion
                #region string
                Move(load, ref Shared.modules.misc.newAvtr,              ref szWorldCrash      );
                #endregion

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

            public void Move(bool load, ref string prop1, ref string prop2)
            {
                if (load) prop1 = prop2;
                else prop2 = prop1;
            }

            public void LOSS(bool load, Modules.ModuleBase module, bool state) // Load Only SetState
            {
                if (load) module.SetState(state);
                else return;
            }
        }
    }
}
