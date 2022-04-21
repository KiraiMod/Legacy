using KiraiMod.Modules;
using MelonLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod
{
    public class Menu
    {
        public QuickMenu qm;
        public GameObject sm;
        public GameObject um;

        public System.Collections.Generic.Dictionary<string, MenuObject> objects = new System.Collections.Generic.Dictionary<string, MenuObject>();
        public System.Collections.Generic.List<GameObject> pages = new System.Collections.Generic.List<GameObject>();
        public int selected = -1;
        private int lastSelected = -1;

        public Menu()
        {
            MelonModLogger.Log("Menu initializing");

            qm = QuickMenu.prop_QuickMenu_0;
            sm = qm.transform.Find("ShortcutMenu").gameObject;
            um = qm.transform.Find("UserInteractMenu").gameObject;
        }

        public bool? GetBool(string id)
        {
            MenuObject obj;
            if (Shared.menu.objects.TryGetValue(id, out obj))
            {
                return obj.toggle.state;
            }
            else return null;
        }

        public float? GetFloat(string id)
        {
            MenuObject obj;
            if (Shared.menu.objects.TryGetValue(id, out obj))
            {
                return obj.slider.value;
            }
            else return null;
        }

        public bool Set(string id, bool value)
        {
            MenuObject obj;
            if (Shared.menu.objects.TryGetValue(id, out obj))
            {
                obj.toggle.SetState(value);
                return true;
            } else return false;
        }

        public bool Set(string id, float value)
        {
            MenuObject obj;
            if (Shared.menu.objects.TryGetValue(id, out obj))
            {
                obj.slider.SetValue(value);
                return true;
            }
            else return false;
        }

        public void CreatePage(string name)
        {
            GameObject page = UnityEngine.Object.Instantiate(sm.gameObject);
            page.transform.name = name;
            for (int i = 0; i < page.transform.childCount; i++)
            {
                Utils.DestroyRecursive(page.transform.GetChild(i));
            }
            page.transform.SetParent(qm.transform, false);
            page.gameObject.SetActive(false);

            pages.Add(page);
        }

        public Toggle CreateToggle(string id, string label, string tooltip, float x, float y, bool state, Transform parent, Action OnEnable, Action OnDisable)
        {
            Toggle toggle = new Toggle(label, tooltip, x, y, state, parent, OnEnable, OnDisable);
            objects.Add(id, toggle);
            return toggle;
        }

        public Toggle CreateToggle(string id, bool state, string text, string tooltip, float x, float y, Transform parent, Action<bool> OnChange)
        {
            Toggle toggle = new Toggle(text, tooltip, x, y, state, parent,
                new Action(() => OnChange.Invoke(true)),
                new Action(() => OnChange.Invoke(false))
            );
            objects.Add(id, toggle);
            return toggle;
        }

        public Toggle CreateToggle(string label, string tooltip, float x, float y, int page, ModuleBase module)
        {
            return CreateToggle($"p{page}/" + label.ToLower().Replace(' ', '-'), module.state, label, tooltip, x, y, pages[page].transform, new Action<bool>(state => module.SetState(state)));
        }

        public Button CreateButton(string id, string label, string tooltip, float x, float y, Transform parent, Action OnClick)
        {
            Button button = new Button(label, tooltip, x, y, parent, OnClick);
            objects.Add(id, button);
            return button;
        }

        public Slider CreateSlider(string id, string label, float x, float y, float min, float max, float initial, Transform parent, Action<float> OnChange)
        {
            Slider slider = new Slider(label, x, y, min, max, initial, parent, OnChange);
            objects.Add(id, slider);
            return slider;
        }

        public void HandlePages()
        {
            if (qm == null) return;

            if (!qm.prop_Boolean_0)
            {
                selected = -1;
            }

            if (selected != -1)
            {
                sm.SetActive(false);
                um.SetActive(false);
            }

            if (selected != lastSelected)
            {
                if (lastSelected > -1)
                {
                    if (pages.Count > lastSelected)
                    {
                        pages[lastSelected].SetActive(false);
                    }
                }
                else
                {
                    sm.SetActive(false);
                }

                if (selected > -1)
                {
                    if (pages.Count > selected)
                    {
                        pages[selected].SetActive(true);
                    }
                }
                else
                {
                    if (qm.prop_Boolean_0)
                    {
                        sm.SetActive(true);
                    }
                }

                lastSelected = selected;
            }
        }

        public struct MenuObject
        {
            public Toggle toggle;
            public Button button;
            public Slider slider;

            public static implicit operator MenuObject(Toggle _toggle)
            {
                return new MenuObject() { toggle = _toggle, button = null, slider = null };
            }

            public static implicit operator MenuObject(Button _button)
            {
                return new MenuObject() { toggle = null, button = _button, slider = null };
            }

            public static implicit operator MenuObject(Slider _slider)
            {
                return new MenuObject() { toggle = null, button = null, slider = _slider };
            }
        }

        public class Toggle
        {
            public GameObject self;
            private GameObject buttonOn;
            private GameObject buttonOff;
            private Action OnDisable;
            private Action OnEnable;
            public bool state;

            public Toggle(string label, string tooltip, float x, float y, bool state, Transform parent, Action OnEnable, Action OnDisable)
            {
                this.state = state;
                this.OnEnable = OnEnable;
                this.OnDisable = OnDisable;

                QuickMenu qm = QuickMenu.prop_QuickMenu_0;

                Transform nameplatesButton = qm.transform.Find("UIElementsMenu/ToggleNameplatesButton");
                if (nameplatesButton == null) MelonModLogger.LogError("Failed to find UIElementMenu/ToggleNamePlatesButton");
                GameObject button = UnityEngine.Object.Instantiate(nameplatesButton.gameObject, parent);

                float size =
                    qm.transform.Find("UserInteractMenu/ForceLogoutButton").localPosition.x -
                    qm.transform.Find("UserInteractMenu/BanButton").localPosition.x;

                button.transform.localPosition = new Vector3(
                    button.transform.localPosition.x + (size * x),
                    button.transform.localPosition.y + (size * y),
                    button.transform.localPosition.z
                );

                buttonOn = button.transform.Find("Toggle_States_NameplatesEnabled/ON").gameObject;
                buttonOn.GetComponentInChildren<Image>().color = Utils.Colors.highlight;

                buttonOff = button.transform.Find("Toggle_States_NameplatesEnabled/OFF").gameObject;
                buttonOff.GetComponentInChildren<Image>().color = Utils.Colors.primary;

                Text[] buttonTextOn = buttonOn.GetComponentsInChildren<Text>();
                buttonTextOn[0].text = label + " On";
                buttonTextOn[1].text = label + " Off";

                Text[] buttonTextOff = buttonOff.GetComponentsInChildren<Text>();
                buttonTextOff[0].text = label + " On";
                buttonTextOff[1].text = label + " Off";

                buttonOn.SetActive(state);
                buttonOff.SetActive(!state);

                UiTooltip buttonTooltip = button.transform.GetComponent<UiTooltip>();
                buttonTooltip.text = tooltip;
                buttonTooltip.alternateText = tooltip;

                UnityEngine.UI.Button buttonClick = button.transform.GetComponentInChildren<UnityEngine.UI.Button>();
                buttonClick.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                buttonClick.onClick.AddListener(new Action(() =>
                {
                    SetState();
                }));

                self = button;
            }

            public void Enable()
            {
                SetState(true);
            }

            public void Disable()
            {
                SetState(false);
            }

            public void SetState(bool? n_state = null)
            {
                if (state == n_state) return;

                state = n_state ?? !state;

                buttonOn.SetActive(state);
                buttonOff.SetActive(!state);

                if (!state) OnDisable.Invoke(); else OnEnable.Invoke();
            }
        }

        public class Button
        {
            public GameObject self;
            public Action OnClick;

            public Button(string text, string tooltip, float x, float y, Transform parent, Action OnClick)
            {
                QuickMenu qm = QuickMenu.prop_QuickMenu_0;

                Transform blockButton = qm.transform.Find("NotificationInteractMenu/BlockButton");
                if (blockButton == null) MelonModLogger.LogError("Failed to find NotificationInteractMenu/BlockButton");
                GameObject button = UnityEngine.Object.Instantiate(blockButton.gameObject, parent);

                float size =
                    qm.transform.Find("UserInteractMenu/ForceLogoutButton").localPosition.x -
                    qm.transform.Find("UserInteractMenu/BanButton").localPosition.x;

                button.transform.localPosition = new Vector3(
                    button.transform.localPosition.x + (size * (x - 1)),
                    button.transform.localPosition.y + (size * y),
                    button.transform.localPosition.z
                );

                button.transform.GetComponentInChildren<Text>().text = text;

                button.transform.GetComponentInChildren<Image>().color = Utils.Colors.black;

                UiTooltip buttonTooltip = button.transform.GetComponentInChildren<UiTooltip>();
                buttonTooltip.text = tooltip;
                buttonTooltip.alternateText = tooltip;

                UnityEngine.UI.Button buttonClick = button.transform.GetComponentInChildren<UnityEngine.UI.Button>();
                buttonClick.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                buttonClick.onClick.AddListener(OnClick);

                self = button;
            }

            public void Click()
            {
                OnClick.Invoke();
            }
        }

        public class Slider
        {
            private bool initialized = false;
            private float initial;

            public GameObject self;
            public Action OnChange;

            public float value;

            public Slider(string text, float x, float y, float min, float max, float initial, Transform parent, Action<float> OnChange)
            {
                QuickMenu qm = QuickMenu.prop_QuickMenu_0;
                this.initial = initial;

                GameObject slider = UnityEngine.Object.Instantiate(VRCUiManager.field_Protected_Static_VRCUiManager_0.menuContent.transform.Find("Screens/Settings/AudioDevicePanel/VolumeSlider"), parent).gameObject;

                float size =
                    qm.transform.Find("UserInteractMenu/ForceLogoutButton").localPosition.x - 
                    qm.transform.Find("UserInteractMenu/BanButton").localPosition.x;

                slider.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                slider.transform.localPosition = new Vector3(size * (x - 0.25f), size * (y + 2.5f), 0.01f);
                slider.transform.localRotation = Quaternion.Euler(0, 0, 0);

                UnityEngine.UI.Slider uiSlider = slider.GetComponentInChildren<UnityEngine.UI.Slider>();
                uiSlider.minValue = min;
                uiSlider.maxValue = max;
                uiSlider.onValueChanged = new UnityEngine.UI.Slider.SliderEvent();
                uiSlider.onValueChanged.AddListener(new Action<float>(_ =>
                {
                    uiSlider.onValueChanged.RemoveAllListeners();
                    uiSlider.onValueChanged.AddListener(OnChange);
                    MelonCoroutines.Start(DelayedInit());
                }));

                slider.transform.Find("Background").GetComponent<Image>().color = Utils.Colors.primary;
                slider.transform.Find("Fill Area").GetComponentInChildren<Image>().color = Utils.Colors.highlight;
                slider.transform.Find("Handle Slide Area").GetComponentInChildren<Image>().color = Utils.Colors.highlight;

                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(parent.transform, false);

                Text textComp = textGO.AddComponent<Text>();
                textComp.text = text;
                textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textComp.fontSize = 64;
                textComp.color = Utils.Colors.highlight;
                textComp.alignment = TextAnchor.MiddleCenter;
                textComp.transform.localPosition = slider.transform.localPosition;
                textComp.transform.localPosition += new Vector3(0, 80, 0);
                textComp.GetComponent<RectTransform>().sizeDelta = new Vector2(textComp.fontSize * text.Count(), 100f);
                textComp.enabled = true;

                self = slider.gameObject;
            }

            public void SetValue(float value)
            {
                this.value = value;
                if (initialized) self.GetComponentInChildren<UnityEngine.UI.Slider>().value = value;
                else initial = value;
            }

            private System.Collections.IEnumerator DelayedInit()
            {
                yield return new WaitForEndOfFrame();
                initialized = true;
                self.GetComponentInChildren<UnityEngine.UI.Slider>().Set(initial, true);
            }
        }
    }
}