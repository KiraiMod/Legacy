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
        public bool locked = false;
        public int offset = 0;

        public System.Collections.Generic.List<KiraiLib.UI.Label> players = new System.Collections.Generic.List<KiraiLib.UI.Label>();

        private KiraiLib.UI.Label up;
        private KiraiLib.UI.Label down;
        private bool hasSet = false;

        public new ModuleInfo[] info = {
            new ModuleInfo("Player List", "Show the players in your instance", ButtonType.Toggle, 8, Shared.PageIndex.toggles2, nameof(state))
        };

        public PlayerList() => MelonCoroutines.Start(DelayedInit());

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
            if (state)
            {
                if (locked) Shared.modules.worldcrash.Show();
                else RefreshEx(true);
            }
        }

        public override void OnPlayerLeft(Player player)
        {
            if (state)
            {
                if (PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0.Count % 16 == 0) offset -= 16;

                if (locked) Shared.modules.worldcrash.Show();
                else RefreshEx(true);
            }
        }

        public override void OnLevelWasLoaded()
        {
            if (state && parent != null)
            {
                if (offset != 0)
                {
                    offset = 0;
                    Refresh();
                }       

                if (!QuickMenu.prop_QuickMenu_0.prop_Boolean_0)
                    parent.active = false;
            }  
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

        public void Init()
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

            if (up is null)
                up = KiraiLib.UI.Label.Create("sm/players_up", "<color=#5600a5>======== <color=#ccf>[ Less ]</color> ========</color>", 0, 0, parent.transform, () => {
                    offset -= 16;
                    Refresh();
                }, false);

            if (down is null)
                down = KiraiLib.UI.Label.Create("sm/players_down", "<color=#5600a5>======== <color=#ccf>[ More ]</color> ========</color>", 0, -1190, parent.transform, () => {
                    if (offset + 16 >= PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0.Count) return;

                    offset += 16;
                    Refresh();
                }, false);
        }

        public void Refresh() => RefreshEx(!locked);
        public void RefreshEx(bool normal)
        {
            if (parent == null) Init();

            if (!normal) locked = true;

            if (state)
            {
                foreach (KiraiLib.UI.Label player in players)
                    player.Destroy();
                players.Clear();

                if (PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 == null) return;
                offset = Mathf.Clamp(offset, 0, PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count - 1);

                for (int i = 0; i < Mathf.Min(PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count, 16); i++)
                {
                    if (i + offset >= PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count) break;

                    int _i = i;
                    Player user = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[i + offset];

                    KiraiLib.UI.Label text = null;
                    if (normal)
                    {
                        text = KiraiLib.UI.Label.Create($"sm/players_{i}",
                            " "
                            + (user.IsMaster() ? "<b>" : "")
                            + $"<color={user.GetTextColor().ToHex()}>{i + 1 + offset} </color>"
                            + (user.IsMaster() ? "</b>" : "")
                            + (user.IsFriend() ? "<b>" : "")
                            + $"<color={user.field_Private_APIUser_0.GetTrustColor().ToHex()}>{user.field_Private_APIUser_0.displayName}</color>"
                            + (user.IsFriend() ? "</b>" : ""),
                            0, (i + 1) * -70, parent.transform, new Action(() => { Utils.SelectPlayer(user); }), false);
                    }
                    else
                    {
                        text = KiraiLib.UI.Label.Create($"sm/players_{i}", GenText(user, Shared.modules.worldcrash.selected.Contains(user.field_Private_APIUser_0.displayName), _i + offset),
                            0, (i + 1) * -70, parent.transform, new Action(() =>
                            {
                                bool targeted = Shared.modules.worldcrash.selected.Contains(user.field_Private_APIUser_0.displayName);

                                if (targeted) Shared.modules.worldcrash.selected.Remove(user.field_Private_APIUser_0.displayName);
                                else Shared.modules.worldcrash.selected.Add(user.field_Private_APIUser_0.displayName);

                                text.SetText(GenText(user, !targeted, _i + offset));
                            }), false);
                    }
                    players.Add(text);
                }
            }
        }

        private string GenText(Player user, bool selected, int index)
        {
            return " "
                + (user.IsMaster() ? "<b>" : "")
                + "<color="
                + (selected ? "#ccf" : "#5600a5")
                + ">"
                + (index + 1)
                + " </color>"
                + (user.IsMaster() ? "</b>" : "")
                + (user.IsFriend() ? "<b>" : "")
                + $"<color={user.field_Private_APIUser_0.GetTrustColor().ToHex()}>{user.field_Private_APIUser_0.displayName}</color>"
                + (user.IsFriend() ? "</b>" : "");
        }
    }
}
