using MelonLoader;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod
{
    public class KiraiMod : MelonMod
    {
        private string oName = "Kirai Chan";

        public bool bUnload = false;
        public bool bSpoof = false;

        public override void OnApplicationStart()
        {
            MelonModLogger.Log("Starting");

            System.IO.Stream stream = Assembly.GetManifestResourceStream("KiraiMod.resources.assetbundle");
            System.IO.MemoryStream mem = new System.IO.MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            Shared.resources = AssetBundle.LoadFromMemory(mem.ToArray(), 0);
            Shared.resources.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            Shared.harmony = Harmony.HarmonyInstance.Create("KiraiMod");

            Shared.modules = new Modules.Modules();
            Shared.config = new Config();
            Shared.config.Load();
            Shared.hooks = new Hooks();

            MelonCoroutines.Start(Shared.modules.portal.AutoPortal());
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
                    Shared.modules.xutils.state = false;
                    if (Shared.modules.xutils.hit != null)
                    {
                        HighlightsFX.prop_HighlightsFX_0
                            .Method_Public_Void_Renderer_Boolean_0(Shared.modules.xutils.hit.gameObject.GetComponent<Renderer>(), false);
                        Shared.modules.xutils.hit = null;
                    }
                }

                Shared.menu.HandlePages();
            }

            if (Input.GetKeyDown(KeyCode.Keypad1)) Shared.modules.speed.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad2)) Shared.modules.flight.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad3)) Shared.modules.noclip.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad4)) Shared.modules.esp.SetState();
            if (Input.GetKeyDown(KeyCode.Keypad5)) Shared.modules.orbit.SetState();
            if (Input.GetKeyDown(KeyCode.KeypadMinus)) MelonModLogger.Log("Alive");
            if (Input.GetKeyDown(KeyCode.KeypadMultiply)) Helper.Teleport(new Vector3(0, 0, 0));
            if (Input.GetKeyDown(KeyCode.Delete)) Unload();
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (bUnload) return;

            if (HighlightsFX.prop_HighlightsFX_0 != null && HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0 != null)
                HighlightsFX.prop_HighlightsFX_0.field_Protected_Material_0.SetColor("_HighlightColor", new Color(0.34f, 0f, 0.65f));

            Shared.modules.speed.Reapply();
        }

        public override void VRChat_OnUiManagerInit()
        {
            Shared.menu = new Menu();

            //Shared.menu.qm.transform.Find("QuickMenu_NewElements/_Background/Panel").GetComponent<Image>().material = 
            //    Shared.resources.LoadAsset_Internal("assets/uiglass.mat", Il2CppType.Of<Material>()).Cast<Material>();

            Shared.menu.CreatePage("kiraimod_options");
            Shared.menu.CreatePage("kiraimod_options2");
            Shared.menu.CreatePage("kiraimod_buttons");
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
                        FieldInfo reference = module.GetType().GetField(info.reference, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (reference == null)
                        {
                            MelonModLogger.LogWarning($"Failed to find property {info.reference} on {module.GetType()}");
                            continue;
                        }
                        Utils.GetGenericLayout(info.index, out int x, out int y);
                        Shared.menu.CreateToggle(Utils.CreateID(info.label, info.page), (bool)reference.GetValue(module),
                            info.label, info.description, x, y, Shared.menu.pages[info.page].transform, (info.reference == nameof(module.state)) ? 
                            new System.Action<bool>(state => 
                            {
                                module.SetState(state);
                            }) : 
                            new System.Action<bool>(state =>
                            {
                                reference.SetValue(module, state);
                            }));
                    }
                    else if (info.type == Modules.ButtonType.Button)
                    {

                    }
                    else if (info.type == Modules.ButtonType.Slider)
                    {

                    }
                    else
                    {
                        MelonModLogger.LogWarning($"[UI] Unknown object {info.label} on {module.GetType()}");
                    }
                }
            }

            Shared.menu.CreateToggle("p0/spoof-name", false, "Spoof Name", "Spoofs name in quick menu", -1f, 0f, Shared.menu.pages[0].transform, new System.Action<bool>((state) =>
            {
                ToggleSpoof(state);
            }));

            Shared.menu.CreateButton("p2/unload", "Unload", "Reverses most KiraiMod changes", 2f, -1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Unload();
            }));
        }

        public void Unload()
        {
            if (bUnload) return;
            MelonModLogger.Log("Unloading");

            bUnload = true;

            Shared.config.Save();
            Shared.modules.flight.SetState(false);
            Shared.modules.speed.SetState(false);
            Shared.modules.noclip.SetState(false);
            Shared.modules.esp.SetState(false);
            Shared.modules.nameplates.SetState(false);
            Shared.modules.kos.SetState(false);
            Shared.modules.portal.SetState(false);
            Shared.modules.modlog.SetState(false);
            Shared.modules.orbit.SetState(false);
            ToggleSpoof(false);

            Helper.DeletePortals();

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
            MelonModLogger.Log("Reloading");
            Shared.config.Load();
            VRChat_OnUiManagerInit();
        }

        public void ToggleSpoof(bool? state = null)
        {
            if ((state ?? !bSpoof) == bSpoof) return;

            bSpoof = state ?? !bSpoof;

            Text nametext = Shared.menu.sm.transform.Find("NameText").GetComponent<Text>();

            if (bSpoof) oName = nametext.text.Substring(4);

            nametext.text = bSpoof ? nametext.text.Replace(oName, "Kirai Chan") : nametext.text.Replace("Kirai Chan", oName);
        }
    }
}