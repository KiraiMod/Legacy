using MelonLoader;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod
{
    class Menu
    {

        public QuickMenu qm;
        public GameObject um;

        public Menu()
        {
            MelonLogger.Log("Menu initializing");

            qm = QuickMenu.prop_QuickMenu_0;
            um = qm.transform.Find("UserInteractMenu").gameObject;
        }

        public class Button
        {
            public GameObject self;

            public Button(string text, string tooltip, float x, float y, Transform parent, Action OnClick)
            {
                QuickMenu qm = QuickMenu.prop_QuickMenu_0;

                Transform blockButton = qm.transform.Find("NotificationInteractMenu/BlockButton");
                if (blockButton == null) MelonLogger.LogError("Failed to find NotificationInteractMenu/BlockButton");
                GameObject button = UnityEngine.Object.Instantiate(blockButton.gameObject, parent);

                button.transform.localPosition = new Vector3(
                    button.transform.localPosition.x + (420 * (x - 1)),
                    button.transform.localPosition.y + (420 * y),
                    button.transform.localPosition.z
                );

                button.transform.GetComponentInChildren<Text>().text = text;

                button.transform.GetComponentInChildren<Image>().color = Color.black;

                UiTooltip buttonTooltip = button.transform.GetComponentInChildren<UiTooltip>();
                buttonTooltip.text = tooltip;
                buttonTooltip.alternateText = tooltip;

                UnityEngine.UI.Button buttonClick = button.transform.GetComponentInChildren<UnityEngine.UI.Button>();
                buttonClick.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                buttonClick.onClick.AddListener(OnClick);

                self = button;
            }
        }
    }
}
