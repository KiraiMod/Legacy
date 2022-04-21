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

        public Menu()
        {
            MelonLogger.Log("Menu initializing");

            qm = QuickMenu.prop_QuickMenu_0;
            sm = qm.transform.Find("ShortcutMenu").gameObject;
            um = qm.transform.Find("UserInteractMenu").gameObject;
        }

        public Button CreateButton(string id, string label, string tooltip, float x, float y, Transform parent, Action OnClick)
        {
            Button button = new Button(label, tooltip, x, y, parent, OnClick);
            return button;
        }

        public class Button
        {
            public GameObject self;
            public Action OnClick;

            public Button(string text, string tooltip, float x, float y, Transform parent, Action OnClick)
            {
                QuickMenu qm = QuickMenu.prop_QuickMenu_0;

                Transform blockButton = qm.transform.Find("NotificationInteractMenu/BlockButton");
                if (blockButton == null) MelonLogger.LogError("Failed to find NotificationInteractMenu/BlockButton");
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
    }
}