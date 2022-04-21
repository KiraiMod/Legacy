using MelonLoader;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using VRC;

namespace KiraiMod.Modules
{
    public class PlayerList : ModuleBase
    {
        public GameObject parent;

        private System.Collections.Generic.List<KiraiLib.UI.Label> players = new System.Collections.Generic.List<KiraiLib.UI.Label>();
        private bool hasSet = false;

        public new ModuleInfo[] info = {
            new ModuleInfo("Player List", "Show the players in your instance", ButtonType.Toggle, 8, Shared.PageIndex.toggles2, nameof(state))
        };

        public PlayerList()
        {
            MelonCoroutines.Start(DelayedInit());
        }

        public override void OnConfigLoaded()
        {
            if (MelonHandler.Mods.Any(mod => mod.Assembly.GetName().Name.Contains("KiraiUI")))
            {
                new Action(() =>
                {
                    KiraiUI.Config.extended = state;
                    KiraiUI.instance.Apply();
                }).Invoke();
            }

            MelonCoroutines.Start(Setup(state));
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

            MelonCoroutines.Start(Setup(state));
        }

        public override void OnPlayerJoined(Player player)
        {
            if (state) Refresh();
        }

        public override void OnPlayerLeft(Player player)
        {
            if (state) Refresh();
        }

        public override void OnLevelWasLoaded()
        {
            if (state && parent != null && !QuickMenu.prop_QuickMenu_0.prop_Boolean_0)
                parent.active = false;
        }

        private IEnumerator Setup(bool state)
        {
            while (parent is null) yield return null;

            BoxCollider collider = QuickMenu.prop_QuickMenu_0.GetComponent<BoxCollider>();
            if (collider != null)
            {
                if (!hasSet && state)
                {
                    hasSet = true;
                    collider.extents += new Vector3(420, 0, 0);
                    collider.center -= new Vector3(420, 0, 0);
                }
                else if (hasSet && !state)
                {
                    hasSet = false;
                    collider.extents += new Vector3(-420, 0, 0);
                    collider.center -= new Vector3(-420, 0, 0);
                }
            }

            parent.active = state && QuickMenu.prop_QuickMenu_0.prop_Boolean_0;
        }

        private IEnumerator DelayedInit()
        {
            while (VRCUiManager.prop_VRCUiManager_0 is null) yield return null; // wait for parent and ui init

            Init();
        }

        private void Init()
        {
            if (parent == null)
            {
                parent = new GameObject();
                parent.name = "KiraiPlayerList";
                parent.AddComponent<RectTransform>();
                parent.transform.SetParent(QuickMenu.prop_QuickMenu_0.transform, true);
                parent.transform.localPosition = new Vector3(-1670, 885, 0);
                parent.transform.localScale = Vector3.one;
                parent.transform.localRotation = Quaternion.identity;

                parent.active = false;
            }
        }

        public void Refresh()
        {
            if (parent == null) Init();

            if (state)
            {
                foreach (KiraiLib.UI.Label player in players)
                    player.Destroy();
                players.Clear();

                if (PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 == null) return;

                for (int i = 0; i < Mathf.Min(PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count, 18); i++)
                {
                    Player user = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[i];

                    KiraiLib.UI.Label text = KiraiLib.UI.Label.Create($"sm/player_{i}", " " +
                        (user.IsMaster() ? "<b>" : "") +
                        $"<color={user.GetTextColor().ToHex()}>{i + 1} </color>" +
                        (user.IsMaster() ? "</b>" : "") +
                        (user.IsFriend() ? "<b>" : "") +
                        $"<color={user.field_Private_APIUser_0.GetTrustColor().ToHex()}>{user.field_Private_APIUser_0.displayName}</color>" +
                        (user.IsFriend() ? "</b>" : "") +
                        "\n", i, parent.transform, new Action(() => { Utils.SelectPlayer(user); }), false);
                    players.Add(text);
                }
            }
        }
    }
}
