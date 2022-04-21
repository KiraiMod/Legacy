﻿using MelonLoader;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod
{
    public class KiraiMod : MelonMod
    {
        public bool bUnload = false;

        public override void OnApplicationStart()
        {
            MelonLogger.Log("Starting");

            if (Random.Range(1, 8192) == 1)
                MelonLogger.Log(Utils.StringIya);


#pragma warning disable 0162
            try
            {
                if (BuildInfo.Version != "0.2.7.2")
                {
                    MelonLogger.Log("You should update");
                    System.Diagnostics.Process.Start("https://github.com/HerpDerpinstine/MelonLoader/releases/latest");
                }
            } catch (System.Exception) {}
#pragma warning restore 0162

            System.IO.Stream stream = Assembly.GetManifestResourceStream("KiraiMod.resources.assetbundle");
            System.IO.MemoryStream mem = new System.IO.MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            Shared.resources = AssetBundle.LoadFromMemory(mem.ToArray(), 0);
            Shared.resources.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            Shared.harmony = Harmony.HarmonyInstance.Create("KiraiMod");

            Shared.modules = new Modules.Modules();
            Shared.config = new Config();
            Shared.config.Load();
            Shared.ipc = new IPC();
            Shared.hooks = new Hooks();

            Shared.modules.StartCoroutines();

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiRPC")))
            {
                MelonLogger.Log("Found KiraiRPC, using it.");
                new System.Action(() =>
                {
                    var oCallback = KiraiRPC.callbackChain;
                    KiraiRPC.callbackChain = new System.Action<string, string, string[]>((target, type, data) =>
                    {
                        switch (type)
                        {
                            case "PlayerUsingMod":
                                Shared.modules.nameplates.users.Add(data[0], data[1]);
                                Shared.modules.nameplates.Refresh();
                                break;
                        }

                        if (oCallback != null) 
                            oCallback.Invoke(target, type, data);
                    });
                    KiraiRPC.Config.primary = "KiraiMod";
                }).Invoke();
            }
        }

        public override void OnApplicationQuit()
        {
            if (!bUnload) Shared.config.Save();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                Reload();
            }

            if (bUnload) return;

            Shared.modules.OnUpdate();

            if (Shared.menu != null)
            {
                if (!Shared.menu.qm.prop_Boolean_0)
                {
                    Shared.modules.xutils.SetState(false);
                    if (Shared.modules.xutils.state2)
                    {
                        HighlightsFX.prop_HighlightsFX_0
                            .Method_Public_Void_Renderer_Boolean_0(Shared.modules.xutils.hit.collider.gameObject.GetComponent<Renderer>(), false);
                        Shared.modules.xutils.state2 = false;
                    }
                }

                Shared.menu.HandlePages();
            }

            if (Input.GetKeyDown(KeyCode.Keypad1)) Shared.modules.speed.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad2)) Shared.modules.flight.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad3)) Shared.modules.noclip.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad4)) Shared.modules.esp.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad5)) Shared.modules.orbit.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad6)) Shared.Options.bWorldTriggers ^= true;
            if (Input.GetKeyDown(KeyCode.Keypad7)) Helper.CrashSelected();
            if (Input.GetKeyDown(KeyCode.KeypadMinus)) MelonLogger.Log("Alive");
            if (Input.GetKeyDown(KeyCode.KeypadMultiply)) Helper.Teleport(new Vector3(0, 0, 0));
            if (Input.GetKeyDown(KeyCode.Delete)) Unload();
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (bUnload) return;

            if (HighlightsFX.prop_HighlightsFX_0 != null && HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0 != null)
                HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0.SetColor("_HighlightColor", new Color(0.34f, 0f, 0.65f));

            Shared.modules.OnLevelWasLoaded();
        }

        public override void VRChat_OnUiManagerInit()
        {
            Shared.menu = new Menu();

            // make quick menu background flush
            Shared.menu.qm.transform.Find("QuickMenu_NewElements/_Background/Panel").GetComponent<Image>().color = Color.white;
            //.material = Shared.resources.LoadAsset_Internal("assets/uiglass.mat", Il2CppType.Of<Material>()).Cast<Material>();

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiUI")))
            {
                new System.Action(() =>
                {
                    if (!KiraiUI.instance.hasStored) KiraiUI.instance.Store();
                    KiraiUI.instance.Apply();
                }).Invoke();
            }

            Shared.menu.CreatePage("kiraimod_options");
            Shared.menu.CreatePage("kiraimod_options2");
            Shared.menu.CreatePage("kiraimod_buttons");
            Shared.menu.CreatePage("kiraimod_buttons2");
            Shared.menu.CreatePage("kiraimod_sliders");
            Shared.menu.CreatePage("kiraimod_xutils");

            new Pages.Pages();

            foreach(Modules.ModuleBase module in Shared.modules.modules)
            {
                foreach (Modules.ModuleInfo info in (Modules.ModuleInfo[])module.GetType()
                    .GetField(nameof(Modules.ModuleBase.info), BindingFlags.Public | BindingFlags.Instance).GetValue(module))
                {
                    if (info.type == Modules.ButtonType.Toggle)
                    {
                        Utils.GetGenericLayout(info.index, out int x, out int y);
                        if (info.reference == nameof(Modules.ModuleBase.state))
                        {
                            Shared.menu.CreateToggle(Utils.CreateID(info.label, info.page), module.state,
                            info.label, info.description, x, y, Shared.menu.pages[info.page].transform,
                            new System.Action<bool>(state =>
                            {
                                module.SetState(state);
                            }));
                        } else
                        {
                            MethodInfo onStateChange = module.GetType().GetMethod($"OnStateChange{(/*info.reference == "state" ? "" :*/ info.reference)}", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            FieldInfo reference = module.GetType().GetField(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                            if (reference == null)
                            {
                                MelonLogger.LogWarning($"Failed to find property {info.reference} on {module.GetType()}");
                                continue;
                            }
                            bool cval = (bool)reference.GetValue(module);
                            Shared.menu.CreateToggle(Utils.CreateID(info.label, info.page), cval,
                                info.label, info.description, x, y, Shared.menu.pages[info.page].transform,
                                onStateChange == null
                                ? new System.Action<bool>(state =>
                                {
                                    reference.SetValue(module, state);
                                })
                                : new System.Action<bool>(state =>
                                {
                                    if (state == cval) return;

                                    cval = !cval;

                                    MelonLogger.Log($"{module.GetType().Name}.{info.reference} {(cval ? "On" : "Off")}");
#if DEBUG
                                    MelonLogger.Log($"cval: {cval}, state: {state}");
#endif
                                    reference.SetValue(module, state);

                                    onStateChange.Invoke(module, new object[] { cval });
                                }));

#if DEBUG
                            MelonLogger.Log($"{module.GetType().Name}.{info.reference}: {(onStateChange == null ? "Acchi" : "Kocchi")}");
#endif
                        }
                    }
                    else if (info.type == Modules.ButtonType.Button)
                    {
                        MethodInfo reference = module.GetType().GetMethod(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (reference == null)
                        {
                            MelonLogger.LogWarning($"Failed to find method {info.reference} on {module.GetType()}");
                            continue;
                        }
                        Utils.GetGenericLayout(info.index, out int x, out int y);
                        Shared.menu.CreateButton(Utils.CreateID(info.label, info.page),
                            info.label, info.description, x, y, Shared.menu.pages[info.page].transform,
                            new System.Action(() =>
                            {
                                reference.Invoke(module, null);
                            }));
                    }
                    else if (info.type == Modules.ButtonType.Slider)
                    {

                    }
                    else
                    {
                        MelonLogger.LogWarning($"[UI] Unknown object {info.label} on {module.GetType()}");
                    }
                }
            }

            Shared.menu.CreateButton("p2/unload", "Unload", "Reverses most KiraiMod changes", 1f, -1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Utils.HUDMessage("Press INSERT to reload");
                Unload();
            }));
        }

        public void Unload()
        {
            if (bUnload) return;
            MelonLogger.Log("Unloading");

            bUnload = true;

            Shared.config.Save();

            for (int i = 0; i < Shared.modules.modules.Count; i++) {
                Shared.modules.modules[i].SetState(false);
            }

            Helper.DeletePortals();

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiUI")))
                new System.Action(() => KiraiUI.instance.Restore()).Invoke();

            foreach (Menu.MenuObject menuObject in Shared.menu.objects.Values)
            {
                if (menuObject.button != null) Object.Destroy(menuObject.button.self);
                else if (menuObject.toggle != null) Object.Destroy(menuObject.toggle.self);
                else if (menuObject.slider != null) Object.Destroy(menuObject.slider.self);
            }

            for (int i = 0; i < Shared.menu.pages.Count; i++) Object.Destroy(Shared.menu.pages[i]);

            Shared.menu.sm.SetActive(Shared.menu.qm.field_Internal_Boolean_0);
        }

        public void Reload()
        {
            Unload();

            bUnload = false;
            MelonLogger.Log("Reloading");
            Shared.config.Load();
            VRChat_OnUiManagerInit();
        }
    }
}