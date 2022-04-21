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
        public enum PageIndex
        {
            options1 = 0,
            options2,
            options3,
            buttons1,
            buttons2,
            sliders1,
            xutils,
            udon1,
            udon2
        }

        public QuickMenu qm;
        public GameObject sm;
        public GameObject um;
        public float size = 420f;

        public System.Collections.Generic.Dictionary<string, MenuObject> objects = new System.Collections.Generic.Dictionary<string, MenuObject>();
        public System.Collections.Generic.List<GameObject> pages = new System.Collections.Generic.List<GameObject>();
        public int selected = -1;
        private int lastSelected = -1;

        public Menu()
        {
            MelonLogger.Log("Menu initializing");

            qm = QuickMenu.prop_QuickMenu_0;
            sm = qm.transform.Find("ShortcutMenu").gameObject;
            um = qm.transform.Find("UserInteractMenu").gameObject;
        }

        public bool? GetBool(string id)
        {
            if (Shared.menu.objects.TryGetValue(id, out MenuObject obj))
            {
                return obj.toggle.state;
            }
            else return null;
        }

        public float? GetFloat(string id)
        {
            if (Shared.menu.objects.TryGetValue(id, out MenuObject obj))
            {
                return obj.slider.value;
            }
            else return null;
        }

        public bool Set(string id, bool value)
        {
            if (Shared.menu.objects.TryGetValue(id, out MenuObject obj))
            {
                obj.toggle.SetState(value);
                return true;
            }
            else return false;
        }

        public bool Set(string id, float value)
        {
            if (Shared.menu.objects.TryGetValue(id, out MenuObject obj))
            {
                obj.slider.SetValue(value);
                return true;
            }
            else return false;
        }

        public void CreatePage(string name)
        {
            GameObject page = UnityEngine.Object.Instantiate(sm.gameObject);
            page.name = name;
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
            if (Shared.config?.buttons != null && Shared.config.buttons.TryGetValue(id, out var value))
            {
                x = value[0];
                y = value[1];
            }

            Toggle toggle = new Toggle(this, label, tooltip, x, y, state, parent, OnEnable, OnDisable);
            objects.Add(id, toggle);
            return toggle;
        }

        public Toggle CreateToggle(string id, bool state, string text, string tooltip, float x, float y, Transform parent, Action<bool> OnChange)
        {
            if (Shared.config?.buttons != null && Shared.config.buttons.TryGetValue(id, out var value))
            {
                x = value[0];
                y = value[1];
            }

            Toggle toggle = new Toggle(this, text, tooltip, x, y, state, parent,
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

        public Button CreateButton(string id, string label, string tooltip, float x, float y, Transform parent, Action OnClick, bool managed = true)
        {
            if (Shared.config?.buttons != null && Shared.config.buttons.TryGetValue(id, out var value))
            {
                x = value[0];
                y = value[1];
            }

            Button button = new Button(this, label, tooltip, x, y, parent, OnClick);
            if (managed) objects.Add(id, button);
            return button;
        }

        public Slider CreateSlider(string id, string label, float x, float y, float min, float max, float initial, Transform parent, Action<float> OnChange)
        {
            if (Shared.config?.buttons != null && Shared.config.buttons.TryGetValue(id, out var value))
            {
                x = value[0];
                y = value[1];
            }

            Slider slider = new Slider(this, label, x, y, min, max, initial, parent, OnChange);
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

            public Toggle(Menu menu, string label, string tooltip, float x, float y, bool state, Transform parent, Action OnEnable, Action OnDisable)
            {
                this.state = state;
                this.OnEnable = OnEnable;
                this.OnDisable = OnDisable;

                QuickMenu qm = QuickMenu.prop_QuickMenu_0;

                Transform hudButton = qm.transform.Find("UIElementsMenu/ToggleHUDButton");
                if (hudButton == null) MelonLogger.LogError("Failed to find UIElementMenu/ToggleHUDButton");
                GameObject button = UnityEngine.Object.Instantiate(hudButton.gameObject, parent);

                button.transform.localPosition = new Vector3(
                    button.transform.localPosition.x + (menu.size * (x + 1)),
                    button.transform.localPosition.y + (menu.size * (y + 1)) + 18,
                    button.transform.localPosition.z
                );

                buttonOn = button.transform.Find("Toggle_States_HUDEnabled/ON").gameObject;
                buttonOn.GetComponentInChildren<Image>().color = Utils.Colors.highlight;

                buttonOff = button.transform.Find("Toggle_States_HUDEnabled/OFF").gameObject;
                buttonOff.GetComponentInChildren<Image>().color = Utils.Colors.primary;

                Text[] buttonTextOn = buttonOn.GetComponentsInChildren<Text>();
                buttonTextOn[0].text = label + " On";
                buttonTextOn[1].text = label + " Off";
                buttonTextOn[0].resizeTextForBestFit = true;
                buttonTextOn[1].resizeTextForBestFit = true;

                Text[] buttonTextOff = buttonOff.GetComponentsInChildren<Text>();
                buttonTextOff[0].text = label + " On";
                buttonTextOff[1].text = label + " Off";
                buttonTextOff[0].resizeTextForBestFit = true;
                buttonTextOff[1].resizeTextForBestFit = true;

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

            public Button(Menu menu, string text, string tooltip, float x, float y, Transform parent, Action OnClick)
            {
                QuickMenu qm = QuickMenu.prop_QuickMenu_0;

                Transform blockButton = qm.transform.Find("NotificationInteractMenu/BlockButton");
                if (blockButton == null) MelonLogger.LogError("Failed to find NotificationInteractMenu/BlockButton");
                GameObject button = UnityEngine.Object.Instantiate(blockButton.gameObject, parent);

                button.transform.localPosition = new Vector3(
                    button.transform.localPosition.x + (menu.size * (x - 1)),
                    button.transform.localPosition.y + (menu.size * y),
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
            private Text valueText;
            private UnityEngine.UI.Slider uiSlider;

            public GameObject self;
            public Action<float> OnChange;

            public float value;

            public Slider(Menu menu, string text, float x, float y, float min, float max, float initial, Transform parent, Action<float> OnChange)
            {
                QuickMenu qm = QuickMenu.prop_QuickMenu_0;
                this.initial = initial;
                this.OnChange = OnChange;

                GameObject slider = UnityEngine.Object.Instantiate(VRCUiManager.prop_VRCUiManager_0.menuContent.transform.Find("Screens/Settings/AudioDevicePanel/VolumeSlider"), parent).gameObject;

                slider.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                slider.transform.localPosition = new Vector3(menu.size * (x - 0.25f), menu.size * (y + 2.5f), 0.01f);
                slider.transform.localRotation = Quaternion.Euler(0, 0, 0);

                uiSlider = slider.GetComponentInChildren<UnityEngine.UI.Slider>();
                uiSlider.minValue = min;
                uiSlider.maxValue = max;
                uiSlider.onValueChanged = new UnityEngine.UI.Slider.SliderEvent();
                uiSlider.onValueChanged.AddListener(new Action<float>(value => {
                    _SetValue(value);
                }));

                Utils.LogGO(slider);
                slider.GetComponent<Image>().color = Utils.Colors.primary;
                slider.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Utils.Colors.highlight;

                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(parent.transform, false);

                Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

                Text textText = textGO.AddComponent<Text>();
                textText.supportRichText = true;
                textText.text = $"<b>{text}</b>";
                textText.font = font;
                textText.fontSize = 64;
                textText.color = Utils.Colors.highlight;
                textText.alignment = TextAnchor.MiddleCenter;
                textText.transform.localPosition = slider.transform.localPosition;
                textText.transform.localPosition += new Vector3(0, 80, 0);
                textText.GetComponent<RectTransform>().sizeDelta = new Vector2(textText.fontSize * text.Count(), 100f);

                valueText = slider.transform.Find("Fill Area/Label").GetComponent<Text>();
                valueText.color = Utils.Colors.primary;

                uiSlider.Set(initial, true);

                self = slider.gameObject;
            }

            public void SetValue(float value)
            {
                this.value = value;
                if (!initialized) initial = value;

                uiSlider.value = value;
                _SetValue(value);
            }

            private void _SetValue(float value)
            {
                valueText.text = $"<b>{value:0.00}</b>";
                OnChange.Invoke(value);
            }
        }
    }
}