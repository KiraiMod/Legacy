using MelonLoader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Windows.Forms;
using UnityEngine;

[assembly: MelonInfo(typeof(KiraiMod.KiraiMod), "KiraiMod", KiraiMod.KiraiLib.Constants.VT_ERASE, "Kirai Chan#8315")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("KiraiUI", "KiraiRPC")]
[assembly: AssemblyMetadata("Iya", "IyaIyaIyaIyaIyaIya")]

namespace KiraiMod
{
    public class KiraiMod : MelonMod
    {
        static KiraiMod()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
#if DEBUG
                "KiraiMod.Lib.KiraiLib.dll"
#else
                "KiraiMod.Lib.KiraiLibLoader.dll"
#endif
                );

            MemoryStream mem = new MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            Assembly.Load(mem.ToArray());

#if !DEBUG
            new Action(() => KiraiLibLoader.Load())();
#endif
        }

        public bool bUnload = false;

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Starting");

            if (UnityEngine.Random.Range(1, 8192) == 1)
                MelonLogger.Msg(Utils.StringIya);

            if (int.TryParse(((string)typeof(BuildInfo).GetField(nameof(BuildInfo.Version)).GetValue(null)).Replace(".", ""), out int version))
            {
                if (int.TryParse(BuildInfo.Version.Replace(".", ""), out int buildVer))
                {
                    if (buildVer > version)
                    {
                        MessageBox.Show("Your MelonLoader is outdated.", "Outdated Loader");
                        Process.Start("https://github.com/HerpDerpinstine/MelonLoader/releases/latest");
                    } else if (version > buildVer)
                    {
                        MelonLogger.Error("Your MelonLoader is too new.");
                        MelonLogger.Error("This mod is written for MelonLoader version " + BuildInfo.Version);
                        MelonLogger.Warning("The mod may work correctly however support will not be provided.");
                    }
                }
            }

            Shared.harmony = Harmony;
            Shared.http = new HttpClient();

            ModuleLoader.Initialize();
            Shared.modules = new Modules.Modules();
            Config.Load();
            Shared.ipc = new IPC();
            Shared.hooks = new Hooks();

            Shared.modules.StartCoroutines();

            KiraiLib.Events.OnUIUnload += () =>
            {
                MelonLogger.Msg("OnUIUnload");
                Shared.modules.OnUnload();

                Shared.unloaded = true;
            };

            KiraiLib.Events.OnUIReload += () => 
            {
                MelonLogger.Msg("OnUIReload");
                VRChat_OnUiManagerInit();
                Shared.modules.OnReload();

                Shared.unloaded = false;
            };

            Integrations.Initialize();
        }

        public override void OnApplicationQuit()
        {
            if (!bUnload) Config.Save();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Insert)) Reload();
             
            if (bUnload) return;

            Shared.modules.OnUpdate();
            Shared.Events.OnUpdate();

            // todo: make this event driven using OnPlayerJoined and OnMenuOpened/Closed
            if (VRCUiManager.prop_VRCUiManager_0 != null)
            {
                if (!QuickMenu.prop_QuickMenu_0.prop_Boolean_0)
                {
                    Shared.modules.xutils.SetState(false);
                    if (Shared.modules.xutils.state2)
                    {
                        HighlightsFX.prop_HighlightsFX_0
                            .Method_Public_Void_Renderer_Boolean_0(Shared.modules.xutils.hit.collider.gameObject.GetComponent<Renderer>(), false);
                        Shared.modules.xutils.state2 = false;
                    }
                }
            }

            if (Shared.modules.misc.BindsNumpad)
            {
                if (Input.GetKeyDown(KeyCode.Keypad1)) Shared.modules.speed.SetState();
                if (Input.GetKeyDown(KeyCode.Keypad2)) Shared.modules.flight.SetState();
                if (Input.GetKeyDown(KeyCode.Keypad3)) Shared.modules.noclip.SetState();
                if (Input.GetKeyDown(KeyCode.Keypad4)) Shared.modules.esp.SetState();
                if (Input.GetKeyDown(KeyCode.Keypad5)) Shared.modules.orbit.SetState();
                if (Input.GetKeyDown(KeyCode.Keypad6)) Shared.Options.bWorldTriggers ^= true;
                if (Input.GetKeyDown(KeyCode.Keypad7)) Shared.modules.hideself.SetState();
                if (Input.GetKeyDown(KeyCode.Keypad8)) 
                    (KiraiLib.UI.elements[Utils.CreateID("World\nCrash", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle).SetState();
                if (Input.GetKeyDown(KeyCode.Keypad9)) Shared.modules.misc.SpawnDynamicPrefab();
                if (Input.GetKeyDown(KeyCode.KeypadMinus))
#if DEBUG
                {
                
                }
#else
                    MelonLogger.Msg("Alive");
#endif
            }
            else if (Shared.modules.misc.BindsTab)
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    if (Input.GetKeyDown(KeyCode.W)) Shared.modules.speed.SetState();
                    if (Input.GetKeyDown(KeyCode.F)) Shared.modules.noclip.SetState();
                    if (Input.GetKeyDown(KeyCode.E)) Shared.modules.esp.SetState();
                    if (Input.GetKeyDown(KeyCode.Q)) Shared.modules.orbit.SetState();
                    if (Input.GetKeyDown(KeyCode.T)) Shared.Options.bWorldTriggers ^= true;
                    if (Input.GetKeyDown(KeyCode.C)) Shared.modules.hideself.SetState();
                    if (Input.GetKeyDown(KeyCode.X)) 
                        (KiraiLib.UI.elements[Utils.CreateID("World\nCrash", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle).SetState();
                    if (Input.GetKeyDown(KeyCode.Mouse2))
                        VRC.SDKBase.Networking.RPC(
                            VRC.SDKBase.RPC.Destination.All,
                            VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject,
                            "PlayEmoteRPC",
                            new Il2CppSystem.Object[] {
                                new Il2CppSystem.Int32
                                {
                                    m_value = 3
                                }.BoxIl2CppObject()
                            });
                    if (Input.GetKeyDown(KeyCode.KeypadMultiply)) Helper.Teleport(new Vector3(0, 0, 0));
                }
            }
            else if (Shared.modules.misc.BindsAlt)
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1)) Shared.modules.speed.SetState();
                    if (Input.GetKeyDown(KeyCode.Alpha2)) Shared.modules.flight.SetState();
                    if (Input.GetKeyDown(KeyCode.Alpha3)) Shared.modules.noclip.SetState();
                    if (Input.GetKeyDown(KeyCode.Alpha4)) Shared.modules.esp.SetState();
                    if (Input.GetKeyDown(KeyCode.Alpha5)) Shared.modules.orbit.SetState();
                    if (Input.GetKeyDown(KeyCode.Alpha6)) Shared.Options.bWorldTriggers ^= true;
                    if (Input.GetKeyDown(KeyCode.Alpha7)) Shared.modules.hideself.SetState();
                    if (Input.GetKeyDown(KeyCode.Alpha8)) 
                        (KiraiLib.UI.elements[Utils.CreateID("World\nCrash", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle).SetState();
                }
            }

            if (Input.GetKeyDown(KeyCode.Delete)) Unload();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (bUnload) return;

            KiraiLib.Events.OnSceneLoad(buildIndex, sceneName);

            if (HighlightsFX.prop_HighlightsFX_0 != null && HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0 != null)
                HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0.SetColor("_HighlightColor", new Color(0.34f, 0f, 0.65f));

            Shared.modules.OnLevelWasLoaded();

            if (Shared.Options.bOSLPush) KiraiLib.Events.OnSceneLoad(buildIndex, sceneName);
        }

        public override void VRChat_OnUiManagerInit()
        {
            MelonLogger.Msg("OnUiManagerInit");

            KiraiLib.UI.Initialize();

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiUI")))
            {
                new Action(() =>
                {
                    if (!KiraiUI.instance.hasStored) KiraiUI.instance.Store();
                    KiraiUI.instance.Apply();
                }).Invoke();
            }

            Shared.PageRemap.Clear();

            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Toggles1"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Toggles2"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Toggles3"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Buttons1"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Buttons2"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Sliders1"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.XUtils"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Udon1"));
            Shared.PageRemap.Add(KiraiLib.UI.CreatePage("KiraiMod.Udon2"));

            ModuleLoader.CreateUIElements();
            new Pages.Pages();

            foreach(Modules.ModuleBase module in Shared.modules.modules)
            {
                foreach (Modules.ModuleInfo info in (Modules.ModuleInfo[])module.GetType()
                    .GetField(nameof(Modules.ModuleBase.info), BindingFlags.Public | BindingFlags.Instance).GetValue(module))
                {
                    if (info.type == Modules.ButtonType.Toggle)
                    {
                        if (info.reference == nameof(Modules.ModuleBase.state))
                        {
                            KiraiLib.UI.Toggle.Create(Utils.CreateID(info.label, (int)info.page),
                            info.label, info.description, info.x, info.y, module.state, KiraiLib.UI.pages[Shared.PageRemap[(int)info.page]].transform,
                            new Action<bool>(state =>
                            {
                                module.SetState(state);
                            }));
                        } 
                        else
                        {
                            MethodInfo onStateChange = module.GetType().GetMethod($"OnStateChange{(/*info.reference == "state" ? "" :*/ info.reference)}", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            FieldInfo reference = module.GetType().GetField(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            if (reference == null)
                            {
                                MelonLogger.Warning($"Failed to find property {info.reference} on {module.GetType()}");
                                continue;
                            }
                            bool cval = (bool)reference.GetValue(module);
                            KiraiLib.UI.Toggle.Create(Utils.CreateID(info.label, (int)info.page),
                                info.label, info.description, info.x, info.y, cval, KiraiLib.UI.pages[Shared.PageRemap[(int)info.page]].transform,
                                onStateChange == null
                                ? new Action<bool>(state =>
                                {
                                    reference.SetValue(module, state);
                                })
                                : new Action<bool>(state =>
                                {
                                    if (state == cval) return;

                                    cval = !cval;

                                    MelonLogger.Msg($"{module.GetType().Name}.{info.reference} {(cval ? "On" : "Off")}");
#if DEBUG
                                    MelonLogger.Msg($"cval: {cval}, state: {state}");
#endif
                                    reference.SetValue(module, state);

                                    onStateChange.Invoke(module, new object[] { cval });
                                }));

#if DEBUG
                            MelonLogger.Msg($"{module.GetType().Name}.{info.reference}: {(onStateChange == null ? "Acchi" : "Kocchi")}");
#endif
                        }
                    }
                    else if (info.type == Modules.ButtonType.Button)
                    {
                        MethodInfo reference = module.GetType().GetMethod(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (reference == null)
                        {
                            MelonLogger.Warning($"Failed to find method {info.reference} on {module.GetType()}");
                            continue;
                        }
                        KiraiLib.UI.Button.Create(Utils.CreateID(info.label, (int)info.page),
                            info.label, info.description, info.x, info.y, KiraiLib.UI.pages[Shared.PageRemap[(int)info.page]].transform,
                            new Action(() =>
                            {
                                reference.Invoke(module, null);
                            }));
                    }
                    else if (info.type == Modules.ButtonType.Half)
                    {
                        MethodInfo reference = module.GetType().GetMethod(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (reference == null)
                        {
                            MelonLogger.Warning($"Failed to find method {info.reference} on {module.GetType()}");
                            continue;
                        }
                        KiraiLib.UI.HalfButton.Create(Utils.CreateID(info.label, (int)info.page),
                            info.label, info.description, info.x, info.y, info.lower, KiraiLib.UI.pages[Shared.PageRemap[(int)info.page]].transform,
                            new Action(() =>
                            {
                                reference.Invoke(module, null);
                            }));
                    }
                    else if (info.type == Modules.ButtonType.Slider)
                    {
                        FieldInfo reference = module.GetType().GetField(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (reference == null)
                        {
                            MelonLogger.Warning($"Failed to find method {info.reference} on {module.GetType()}");
                            continue;
                        }
                        float cval = (float)reference.GetValue(module);

                        MethodInfo onValueChange = module.GetType().GetMethod($"OnValueChange{info.reference}", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        KiraiLib.UI.Slider.Create(Utils.CreateID(info.label, (int)info.page),
                            info.label, info.x, info.y, info.min, info.max, cval, KiraiLib.UI.pages[Shared.PageRemap[(int)info.page]].transform, onValueChange == null ? new Action<float>((value) =>
                            {
                                reference.SetValue(module, value);
                            }) : new Action<float>((value) =>
                            {
                                onValueChange.Invoke(module, new object[] { value });
                            }));
#if DEBUG
                            MelonLogger.Msg($"{module.GetType().Name}.{info.reference}: {(onValueChange == null ? "Acchi" : "Kocchi")}");
#endif
                    }
                    else
                    {
                        MelonLogger.Warning($"[UI] Unknown object {info.label} on {module.GetType()}");
                    }
                }
            }

            KiraiLib.UI.Button.Create("p2/unload", "Unload", "Reverses most KiraiMod changes", 1f, -1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons1]].transform, new Action(() =>
            {
                KiraiLib.Logger.Log("Press INSERT to reload");
                Unload();
            }));
        }

        public void Unload()
        {
            if (bUnload) return;
            MelonLogger.Msg("Unloading");

            bUnload = true;

            Config.Save();

            for (int i = 0; i < Shared.modules.modules.Count; i++) {
                Shared.modules.modules[i].SetState(false);
            }

            Helper.DeletePortals();

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiUI")))
                new Action(() => KiraiUI.instance.Restore()).Invoke();

            KiraiLib.UI.Unload();
        }

        public void Reload()
        {
            Unload();

            bUnload = false;
            MelonLogger.Msg("Reloading");
            Config.Load();

            KiraiLib.UI.Reload();
        }
    }
}