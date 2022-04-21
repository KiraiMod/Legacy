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

            if (int.TryParse(((string)typeof(BuildInfo).GetField(nameof(BuildInfo.Version)).GetValue(null)).Replace(".", ""), out int version))
            {
                if (int.TryParse(BuildInfo.Version.Replace(".", ""), out int buildVer))
                {
                    if (buildVer > version)
                    {
                        System.Windows.Forms.MessageBox.Show("Your MelonLoader is outdated.", "Outdated Loader");
                        System.Diagnostics.Process.Start("https://github.com/HerpDerpinstine/MelonLoader/releases/latest");
                    }
                }
            }

            //System.IO.Stream stream = Assembly.GetManifestResourceStream("KiraiMod.resources.assetbundle");
            //System.IO.MemoryStream mem = new System.IO.MemoryStream((int)stream.Length);
            //stream.CopyTo(mem);

            //Shared.resources = AssetBundle.LoadFromMemory(mem.ToArray(), 0);
            //Shared.resources.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            Shared.harmony = Harmony.HarmonyInstance.Create("KiraiMod");

            Shared.modules = new Modules.Modules();
            Shared.config = new Config();
            Shared.config.Load();
            Shared.ipc = new IPC();
            Shared.hooks = new Hooks();

            Shared.modules.StartCoroutines();

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiRPC")))
            {
                MelonLogger.Log("Found KiraiRPC, using it");
                new System.Action(() =>
                {
                    var SendRPC = KiraiRPC.GetSendRPC("KiraiMod");
                    KiraiRPC.callbackChain += new System.Action<KiraiRPC.RPCData>((data) =>
                    {
                        if (data.target == "KiraiRPC")
                        {
                            switch (data.id)
                            {
                                case (int)KiraiRPC.RPCEventIDs.OnInit:
                                    SendRPC(0x000, data.sender);
                                    break;
                            }

                        }
                        else if (data.target == "KiraiMod")
                        {
                            switch (data.id)
                            {
                                case 0x000:
                                case 0x001:
                                    if (data.payload == VRC.Player.prop_Player_0.field_Private_APIUser_0.displayName)
                                    {
                                        Shared.modules.nameplates.users.Add(data.sender);
                                        Shared.modules.nameplates.Refresh();
                                        if (data.id == 0x000)
                                            SendRPC(0x001, data.sender);
                                    }
                                    break;
                            }
                        }
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
            if (Input.GetKeyDown(KeyCode.Keypad7)) Shared.modules.hideself.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                string text = System.Windows.Forms.Clipboard.GetText().Trim();
                if (!text.StartsWith("avtr_"))
                    Utils.HUDMessage("Invalid avatar ID in clipboard");
                else
                    foreach (var a in Object.FindObjectsOfType<VRC.SDKBase.VRC_AvatarPedestal>())
                    {
                        VRC.SDKBase.Networking.SetOwner(VRC.SDKBase.Networking.LocalPlayer, a.gameObject);
                        VRC.SDKBase.Networking.RPC(VRC.SDKBase.RPC.Destination.All, a.gameObject, "SwitchAvatar", new Il2CppSystem.Object[] { text });
                    }
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
#if DEBUG
            {
                MelonLogger.Log(Shared.menu.qm.prop_Boolean_0);
                MelonLogger.Log(Shared.menu.qm.prop_Boolean_1);
                MelonLogger.Log(Shared.menu.qm.prop_Boolean_2);
                MelonLogger.Log(Shared.menu.qm.prop_Boolean_3);
                MelonLogger.Log(Shared.menu.qm.prop_Boolean_4);
            }
#else
                MelonLogger.Log("Alive");
#endif
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

            if (!MelonHandler.Mods.Any(m => m.Assembly.GetName().Name == "KiraiUI"))
                Shared.menu.qm.transform.Find("QuickMenu_NewElements/_Background/Panel").GetComponent<Image>().color = Color.white;

            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiUI")))
            {
                new System.Action(() =>
                {
                    if (!KiraiUI.instance.hasStored) KiraiUI.instance.Store();
                    KiraiUI.instance.Apply();
                }).Invoke();
            }

            Shared.menu.CreatePage("kiraimod_options1");
            Shared.menu.CreatePage("kiraimod_options2");
            Shared.menu.CreatePage("kiraimod_options3");
            Shared.menu.CreatePage("kiraimod_buttons1");
            Shared.menu.CreatePage("kiraimod_buttons2");
            Shared.menu.CreatePage("kiraimod_sliders1");
            Shared.menu.CreatePage("kiraimod_xutils");
            Shared.menu.CreatePage("kiraimod_udon1");
            Shared.menu.CreatePage("kiraimod_udon2");

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
                            Shared.menu.CreateToggle(Utils.CreateID(info.label, info.page), module.state,
                            info.label, info.description, info.x, info.y, Shared.menu.pages[info.page].transform,
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
                                info.label, info.description, info.x, info.y, Shared.menu.pages[info.page].transform,
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
                        Shared.menu.CreateButton(Utils.CreateID(info.label, info.page),
                            info.label, info.description, info.x, info.y, Shared.menu.pages[info.page].transform,
                            new System.Action(() =>
                            {
                                reference.Invoke(module, null);
                            }));
                    }
                    else if (info.type == Modules.ButtonType.Slider)
                    {
                        FieldInfo reference = module.GetType().GetField(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (reference == null)
                        {
                            MelonLogger.LogWarning($"Failed to find method {info.reference} on {module.GetType()}");
                            continue;
                        }
                        float cval = (float)reference.GetValue(module);

                        MethodInfo onValueChange = module.GetType().GetMethod($"OnValueChange{info.reference}", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        Shared.menu.CreateSlider(Utils.CreateID(info.label, info.page),
                            info.label, info.x, info.y, info.min, info.max, cval, Shared.menu.pages[info.page].transform, onValueChange == null ? new System.Action<float>((value) =>
                            {
                                reference.SetValue(module, value);
                            }) : new System.Action<float>((value) =>
                            {
                                onValueChange.Invoke(module, new object[] { value });
                            }));
#if DEBUG
                            MelonLogger.Log($"{module.GetType().Name}.{info.reference}: {(onValueChange == null ? "Acchi" : "Kocchi")}");
#endif
                    }
                    else
                    {
                        MelonLogger.LogWarning($"[UI] Unknown object {info.label} on {module.GetType()}");
                    }
                }
            }

            Shared.menu.CreateButton("p2/unload", "Unload", "Reverses most KiraiMod changes", 1f, -1f, Shared.menu.pages[(int)Menu.PageIndex.buttons1].transform, new System.Action(() =>
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