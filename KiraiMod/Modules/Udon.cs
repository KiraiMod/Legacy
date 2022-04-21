using MelonLoader;
using System.Collections.Generic;
using UnityEngine;
using VRC.Udon;

namespace KiraiMod.Modules
{
    public class Udon : ModuleBase
    {
        private List<GameObject> pages = new List<GameObject>();
        private UdonBehaviour[] behaviours;

        private readonly int pageSize = 12;

        private int currentPage = 0;

        public int CurrentPage { 
            get => currentPage; 
            set { 
                if (value < 0 || value > pages.Count) return; 
                HandlePage(currentPage, value); 
                currentPage = value; 
            } 
        }

        public override void OnLevelWasLoaded()
        {
            behaviours = Object.FindObjectsOfType<UdonBehaviour>();

            foreach (GameObject page in pages)
                Object.Destroy(page);

            pages.Clear();

            for (int i = 0; i < Mathf.Ceil(behaviours.Length / 12f); i++)
            {
                GameObject page = new GameObject($"page_{i}");
                pages.Add(page);

                page.transform.SetParent(Shared.menu.pages[(int)Menu.PageIndex.udon].transform, false);
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
                        behaviours[current].SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "_interact");

                        foreach (var k in behaviours[current]._eventTable)
                            MelonLogger.Log(k.key);
                    }));
                }
            }

            currentPage = 0;
        }

        private void HandlePage(int original, int target)
        {
            if (pages.Count > original)
                pages[original].active = false;

            if (pages.Count > target)
                pages[target].active = true;
        }
    }
}
