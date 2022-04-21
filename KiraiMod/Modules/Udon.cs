using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.Udon;

namespace KiraiMod.Modules
{
    public class Udon : ModuleBase
    {
        public new ModuleInfo[] info =
        {
            new ModuleInfo("Refresh", "Fetch all UdonBehaviours again", ButtonType.Button, 3, 2, Menu.PageIndex.udon1, nameof(Refresh)),
            new ModuleInfo("Up", "See less UdonBehaviours", ButtonType.Button, 3, 1, Menu.PageIndex.udon1, nameof(Up)),
            new ModuleInfo("Down", "See more UdonBehaviours", ButtonType.Button, 3, 0, Menu.PageIndex.udon1, nameof(Down)),
            new ModuleInfo("Broadcast", "Broadcast an event to every UdonBehaviour", ButtonType.Button, 3, -1, Menu.PageIndex.udon1, nameof(Broadcast)),

            new ModuleInfo("Targeted", "Execute the event for the targeted player", ButtonType.Toggle, 3, 2, Menu.PageIndex.udon2, nameof(Targeted)),
            new ModuleInfo("Networked", "Execute the event for everyone", ButtonType.Toggle, 3, -1, Menu.PageIndex.udon2, nameof(Networked)),
            new ModuleInfo("Up", "See less event names", ButtonType.Button, 3, 1, Menu.PageIndex.udon2, nameof(Up2)),
            new ModuleInfo("Down", "See more event names", ButtonType.Button, 3, 0, Menu.PageIndex.udon2, nameof(Down2)),
        };

        private List<GameObject> pages = new List<GameObject>();
        private List<Menu.Button> buttons = new List<Menu.Button>();
        private readonly int pageSize = 12;
        private UdonBehaviour selected;
        private int currentPage = 0;
        private int buttonPage = 0;

        public bool Networked = false;
        public bool Targeted = false;

        public UdonBehaviour[] behaviours;
        public int CurrentPage { 
            get => currentPage; 
            set {
                if (value < 0 || value > pages.Count - 1) return; 
                HandlePage(currentPage, value); 
                currentPage = value; 
            } 
        }
        public int ButtonPage
        {
            get => buttonPage;
            set
            {
                if ((value < 0 || value > (selected?._eventTable?.Count ?? 0) / 12 - 1) && value != 0) return;
                buttonPage = value;
                HandleButtonPage();
            }
        }

        public override void OnLevelWasLoaded()
        {
            behaviours = Object.FindObjectsOfType<UdonBehaviour>();

            foreach (GameObject page in pages)
                Object.Destroy(page);

            pages.Clear();

            ClearButtons();

            for (int i = 0; i < Mathf.Ceil(behaviours.Length / 12f); i++)
            {
                GameObject page = new GameObject($"page_{i}");
                pages.Add(page);

                page.transform.SetParent(Shared.menu.pages[(int)Menu.PageIndex.udon1].transform, false);
                page.active = i == 0;

                for (int j = 0; j < Mathf.Min(behaviours.Length - (i * pageSize), 12); j++)
                {
                    int current = i * pageSize + j;

                    GameObject gameObject = new GameObject($"udon_{current + 1}");

                    var text = gameObject.AddComponent<UnityEngine.UI.Text>();
                    var button = gameObject.AddComponent<UnityEngine.UI.Button>();

                    gameObject.transform.SetParent(page.transform, false);
                    gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1600, 100);
                    gameObject.transform.localPosition = new Vector3(0, 1600 - j * 100);

                    text.color = Utils.Colors.white;
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    text.verticalOverflow = VerticalWrapMode.Truncate;
                    text.alignment = TextAnchor.MiddleLeft;
                    text.fontSize = 54;
                    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    text.supportRichText = true;

                    text.text = $"{current + 1}: {behaviours[current].name}";

                    button.onClick.AddListener(new System.Action(() =>
                    {
                        Shared.menu.selected = (int)Menu.PageIndex.udon2;
                        selected = behaviours[current];
                        ButtonPage = 0;
                    }));
                }
            }

            currentPage = 0;
        }

        public void OnStateChangeNetworked(bool state)
        {
            if (state && Shared.menu.objects.TryGetValue(Utils.CreateID("Targeted", (int)Menu.PageIndex.udon2), out Menu.MenuObject obj))
            {
                obj.toggle.SetState(false);
            }
        }

        public void OnStateChangeTargeted(bool state)
        {
            if (state && Shared.menu.objects.TryGetValue(Utils.CreateID("Networked", (int)Menu.PageIndex.udon2), out Menu.MenuObject obj))
            {
                obj.toggle.SetState(false);
            }
        }


        private void HandlePage(int original, int target)
        {
            if (pages.Count > original)
                pages[original].active = false;

            if (pages.Count > target)
                pages[target].active = true;
        }

        private void HandleButtonPage()
        {
            ClearButtons();

            for (int i = 0; i < Mathf.Min(selected._eventTable.Count - buttonPage * 12, 12); i++)
            {
                string name = GetEventName(i + buttonPage * 12);
                Utils.GetGenericLayout(i, out int x, out int y);
                buttons.Add(Shared.menu.CreateButton($"udon2/execute-{i + (buttonPage * 12) - 1}", name, "Execute this event", x, y, Shared.menu.pages[(int)Menu.PageIndex.udon2].transform, new System.Action(() =>
                {
                    Execute(name);
                }), false));
            }
        }

        private void ClearButtons()
        {
            foreach (Menu.Button button in buttons)
                Object.Destroy(button.self);
        }

        private string GetEventName(int index)
        {
            int i = 0;
            foreach (var a in selected._eventTable)
            {
                if (i == index) return a.key;
                i++;
            }
            return "<NULL>";
        }

        private void Execute(string name)
        {
            if (Networked)
            {
                if (name.StartsWith("_")) KiraiLib.Logger.Log("Events starting with _ are non-networkable.");
                selected.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, name);
            } else if (Targeted)
            {
                if (name.StartsWith("_")) KiraiLib.Logger.Log("Events starting with _ are non-targetable.");
                VRC.SDKBase.Networking.SetOwner(Shared.TargetPlayer.field_Private_VRCPlayerApi_0, selected.gameObject);
                selected.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, name);
            }
            else
            {
                selected.SendCustomEvent(name);
            }
        }

        #region Buttons
        public void Refresh()
        {
            Shared.modules.udon.OnLevelWasLoaded();
        }

        public void Up()
        {
            CurrentPage--;
        }

        public void Down()
        {
            CurrentPage++;
        }

        public void Broadcast()
        {
            if (Shared.modules.misc.bUseClipboard)
                Helper.BroadcastCustomEvent(System.Windows.Forms.Clipboard.GetText().Trim());
            else
                KiraiLib.HUDInput("Custom event name", "Execute", "_interact", new System.Action<string>((resp) =>
                {
                    Helper.BroadcastCustomEvent(resp.Trim());
                }));
        }

        public void Up2()
        {
            ButtonPage--;
        }

        public void Down2()
        {
            ButtonPage++;
        }
        #endregion
    }
}
