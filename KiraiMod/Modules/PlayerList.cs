using MelonLoader;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace KiraiMod.Modules
{
    public class PlayerList : ModuleBase
    {
        private GameObject go;
        private Text text;

        public new ModuleInfo[] info = {
            new ModuleInfo("Player List", "Show the players in your instance", ButtonType.Toggle, 8, Menu.PageIndex.options2, nameof(state))
        };

        public PlayerList()
        {
            MelonCoroutines.Start(DelayedInit());
        }

        public override void OnConfigLoaded()
        {
            KiraiUI.Config.extended = state;
            KiraiUI.instance.Apply();
        }

        public override void OnStateChange(bool state)
        {
            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiUI")))
            {
                new Action(() =>
                {
                    KiraiUI.Config.extended = state;
                    KiraiUI.instance.Apply();
                }).Invoke();
            }

            Refresh();
        }

        public override void OnPlayerJoined(Player player)
        {
            if (state) Refresh();
        }

        public override void OnPlayerLeft(Player player)
        {
            if (state) Refresh();
        }

        public override void OnUpdate()
        {
            if (state && go != null)
                go.active = Shared.menu.qm.prop_Boolean_0;
        }

        private IEnumerator DelayedInit()
        {
            while (Shared.menu == null) yield return null; // wait for ui init

            Init();
        }

        private void Init()
        {
            if (go == null)
            {
                go = new GameObject();
                go.transform.SetParent(Shared.menu.qm.transform);
                go.transform.localPosition = new Vector3(-1650, -210, 0);
                go.transform.localScale = new Vector3(1, 1, 1);
                go.transform.rotation = Shared.menu.qm.transform.rotation;
                text = go.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = 64;
                text.alignment = TextAnchor.UpperLeft;
                text.color = Utils.Colors.highlight;
                text.supportRichText = true;
                go.transform.GetComponent<RectTransform>().sizeDelta = new Vector3(840, 1260);
                go.active = false;
            }
        }

        public void Refresh()
        {
            if (text == null) Init();

            go.active = state;

            if (state)
            {
                text.text = "";

                if (PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 == null) return;


                for (int i = 0; i < PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count; i++)
                {
                    Player user = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[i];
                    text.text += $"<color={user.GetTextColorLegacy().ToHex()}>{i + 1} </color>" +
                        (user.IsFriend() ? "<b>" : "") +
                        $"<color={user.field_Private_APIUser_0.GetTrustColor().ToHex()}>{user.field_Private_APIUser_0.displayName}</color>" +
                        (user.IsFriend() ? "</b>" : "") +
                        "\n";
                }
            }
        }
    }
}
