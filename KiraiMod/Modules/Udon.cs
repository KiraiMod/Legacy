using MelonLoader;
using System.Collections.Generic;
using UnityEngine;
using VRC.Udon;

namespace KiraiMod.Modules
{
    public class Udon : ModuleBase
    {
        public Udon() => MelonCoroutines.Start(Setup(true));

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Select\nIndex", "Select a UdonBehaviour by its index", ButtonType.Button, 3, 3, Shared.PageIndex.udon1, nameof(SelectIndex)),
            new ModuleInfo("Refresh", "Fetch all UdonBehaviours again", ButtonType.Button, 3, 2, Shared.PageIndex.udon1, nameof(Refresh)),
            new ModuleInfo("Up", "See less UdonBehaviours", ButtonType.Button, 3, 1, Shared.PageIndex.udon1, nameof(Up)),
            new ModuleInfo("Down", "See more UdonBehaviours", ButtonType.Button, 3, 0, Shared.PageIndex.udon1, nameof(Down)),
            new ModuleInfo("Broadcast", "Broadcast an event to every UdonBehaviour", ButtonType.Button, 3, -1, Shared.PageIndex.udon1, nameof(Broadcast)),

            new ModuleInfo("Set Alpha", "Stores the last used button in the alpha register", ButtonType.Half, 0, 3, false, Shared.PageIndex.udon2, nameof(SetAlpha)),
            new ModuleInfo("Set Beta", "Stores the last used button in the beta register", ButtonType.Half, 1, 3, false, Shared.PageIndex.udon2, nameof(SetBeta)),
            new ModuleInfo("Set Gamma", "Stores the last used button in the gamma register", ButtonType.Half, 2, 3, false, Shared.PageIndex.udon2, nameof(SetGamma)),
            new ModuleInfo("Clear Alpha", "Clears the alpha register", ButtonType.Half, 0, 3, true, Shared.PageIndex.udon2, nameof(ClearAlpha)),
            new ModuleInfo("Clear Beta", "Clears the beta register", ButtonType.Half, 1, 3, true, Shared.PageIndex.udon2, nameof(ClearBeta)),
            new ModuleInfo("Clear Gamma", "Clears the gamma register", ButtonType.Half, 2, 3, true, Shared.PageIndex.udon2, nameof(ClearGamma)),

            new ModuleInfo("Repeat", "Repeat the last clicked event 10 times a second", ButtonType.Toggle, 3, 3, Shared.PageIndex.udon2, nameof(Repeat)),
            new ModuleInfo("Targeted", "Execute the event for the targeted player", ButtonType.Toggle, 3, 2, Shared.PageIndex.udon2, nameof(Targeted)),
            new ModuleInfo("Networked", "Execute the event for everyone", ButtonType.Toggle, 3, -1, Shared.PageIndex.udon2, nameof(Networked)),
            new ModuleInfo("Up", "See less event names", ButtonType.Button, 3, 1, Shared.PageIndex.udon2, nameof(Up2)),
            new ModuleInfo("Down", "See more event names", ButtonType.Button, 3, 0, Shared.PageIndex.udon2, nameof(Down2)),
        };

        private List<GameObject> pages = new List<GameObject>();
        private List<KiraiLib.UI.Button> buttons = new List<KiraiLib.UI.Button>(); // this might be a data race
        private readonly int pageSize = 12;
        private UdonBehaviour selected;
        private int currentPage = 0;
        private int buttonPage = 0;
        private object token = null;
        private (int, string, bool) storedAlpha = (0, null, false);
        private (int, string, bool) storedBeta = (0, null, false);
        private (int, string, bool) storedGamma = (0, null, false);
        private (int, string, bool) last = (0, null, false);
        private bool hasSet;

        public bool Networked = false;
        public bool Targeted = false;
        public bool Repeat = false;

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

        public override void OnUnload()
        {
            OnStateChangeRepeat(Repeat = false);
            MelonCoroutines.Start(Setup(false));
        }
        public override void OnReload() => MelonCoroutines.Start(Setup(true));

        private System.Collections.IEnumerator Setup(bool expand)
        {
            while (VRCUiManager.prop_VRCUiManager_0 is null) yield return null;

            BoxCollider collider = QuickMenu.prop_QuickMenu_0.GetComponent<BoxCollider>();

            if (expand && !hasSet) // expand
            {
                hasSet = true;

                collider.extents += new Vector3(0, 420, 0);
                collider.center += new Vector3(0, 420, 0);
            } else if (!expand && hasSet)
            {
                hasSet = false;

                collider.extents -= new Vector3(0, 420, 0);
                collider.center -= new Vector3(0, 420, 0);
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

                page.transform.SetParent(KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.udon1]].transform, false);
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
                        KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.udon2];
                        selected = behaviours[current];
                        last.Item1 = current;
                        last.Item3 = true;
                        ButtonPage = 0;
                    }));
                }
            }

            currentPage = 0;
        }

        public void OnStateChangeNetworked(bool state)
        {
            if (state)
                (KiraiLib.UI.elements[Utils.CreateID("Targeted", Shared.PageRemap[(int)Shared.PageIndex.udon2])] as KiraiLib.UI.Toggle)?.SetState(false);
        }

        public void OnStateChangeTargeted(bool state)
        {
            if (state)
                (KiraiLib.UI.elements[Utils.CreateID("Networked", Shared.PageRemap[(int)Shared.PageIndex.udon2])] as KiraiLib.UI.Toggle)?.SetState(false);
        }

        public void OnStateChangeRepeat(bool state)
        {
            if (token != null)
                MelonCoroutines.Stop(token);

            if (state) token = MelonCoroutines.Start(Repeater());
            else token = null;
        }

        private System.Collections.IEnumerator Repeater()
        {
            for (;;)
            {
                if (storedAlpha.Item3) Execute(storedAlpha.Item1, storedAlpha.Item2);
                if (storedBeta.Item3) Execute(storedBeta.Item1, storedBeta.Item2);
                if (storedGamma.Item3) Execute(storedGamma.Item1, storedGamma.Item2);

                yield return new WaitForSecondsRealtime(0.1f);
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
                buttons.Add(KiraiLib.UI.Button.Create($"udon2/execute-{i + (buttonPage * 12) - 1}", name, "Execute this event", x, y,
                    KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.udon2]].transform, new System.Action(() =>
                {
                    last.Item2 = name;
                    Execute(last.Item1, last.Item2);
                }), false));
            }
        }

        private void ClearButtons()
        {
            foreach (KiraiLib.UI.Button button in buttons)
                button.Destroy();
            buttons.Clear();
        }

        private string GetEventName(int index)
        {
            int i = 0;
            foreach (var a in selected._eventTable)
            {
                if (i == index) return a.key;
                i++;
            }
            return null;
        }

        private void Execute(int index, string name)
        {
            if (Networked)
            {
                if (name.StartsWith("_")) KiraiLib.Logger.Log("Events starting with _ are non-networkable.");
                behaviours[index].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, name);
            } else if (Targeted)
            {
                if (name.StartsWith("_")) KiraiLib.Logger.Log("Events starting with _ are non-targetable.");
                VRC.SDKBase.Networking.SetOwner(Shared.TargetPlayer.field_Private_VRCPlayerApi_0, behaviours[index].gameObject);
                behaviours[index].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, name);
            }
            else behaviours[index].SendCustomEvent(name);
        }

        #region Buttons
        public void SelectIndex()
        {
            KiraiLib.HUDKeypad("Select By Index", "Select", "5306", (val) => {
                if (int.TryParse(val, out int index))
                {
                    if (index >= behaviours.Length && index < 1)
                        KiraiLib.Logger.Log("Index is out of bounds");
                    else
                    {
                        KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.udon2];
                        selected = behaviours[index - 1];
                        last.Item1 = index - 1;
                        last.Item3 = true;
                        ButtonPage = 0;
                    }
                } // Keypad will never return an invalid or negative number
            });
        }

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

        public void Up2() => ButtonPage--;
        public void Down2() => ButtonPage++;

        public void SetAlpha()
        {
            if (!last.Item3) return;

            if (last == storedBeta) KiraiLib.Logger.Log("Value Already in Beta");
            else if (last == storedGamma) KiraiLib.Logger.Log("Value Already in Gamma");
            else
            {
                KiraiLib.Logger.Log($"Setting Alpha to {selected.name}::{last}");
                storedAlpha = last;
            }
        }

        public void SetBeta()
        {
            if (!last.Item3) return;

            if (last == storedAlpha) KiraiLib.Logger.Log("Value Already in Alpha");
            else if (last == storedGamma) KiraiLib.Logger.Log("Value Already in Gamma");
            else
            {
                storedBeta = last;
                KiraiLib.Logger.Log($"Setting Beta to {selected.name}::{last}");
            }
        }

        public void SetGamma()
        {
            if (!last.Item3) return;

            if (last == storedAlpha) KiraiLib.Logger.Log("Value Already in Alpha");
            else if (last == storedBeta) KiraiLib.Logger.Log("Value Already in Beta");
            else
            {
                storedGamma = last;
                KiraiLib.Logger.Log($"Setting Gamma to {selected.name}::{last}");
            }
        }

        public void ClearAlpha()
        {
            if (!storedAlpha.Item3) return; 
            KiraiLib.Logger.Log("Clearing Alpha register");
            storedAlpha.Item3 = false;
        }

        public void ClearBeta()
        {
            if (!storedBeta.Item3) return; 
            KiraiLib.Logger.Log("Clearing Beta register");
            storedBeta.Item3 = false;
        }

        public void ClearGamma()
        {
            if (!storedGamma.Item3) return; 
            KiraiLib.Logger.Log("Clearing Gamma register");
            storedGamma.Item3 = false;
        }
        #endregion
    }
}
