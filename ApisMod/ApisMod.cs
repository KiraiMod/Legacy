using MelonLoader;
using System.Linq;
using UnityEngine;
using VRC;

namespace ApisMod
{
    public class ApisMod : MelonMod
    {
        public Player master;
        public int priority;

        public KiraiMod.Menu menu;

        public System.Collections.Generic.List<string> masters = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> slaves = new System.Collections.Generic.List<string>();

        private bool missingRPC;

        private int currentPage;

        private GameObject[] masterButtons = new GameObject[12];
        private GameObject[] slaveButtons = new GameObject[12];

        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(Initialize());

            MelonPrefs.RegisterInt("ApisMod", "MasterCount", 0, "MasterCount", true);
            MelonPrefs.RegisterInt("ApisMod", "SlaveCount", 0, "SlaveCount", true);

            OnModSettingsApplied();

            if (MelonHandler.Mods.Any(m => m.Assembly.GetName().Name.Contains("KiraiRPC")))
            {
                new System.Action(() =>
                {
                    var SendRPC = KiraiMod.KiraiRPC.GetCallback("ApisMod");
                    var oCallback = KiraiMod.KiraiRPC.callbackChain;
                    KiraiMod.KiraiRPC.callbackChain = new System.Action<string, string, string[]>((target, type, data) =>
                    {
                        if (target == "KiraiRPC")
                        {
                            switch (type)
                            {
                                case "D00":
                                    SendRPC(KiraiMod.KiraiRPC.SendType.Get, "00", data[0]);
                                    break;
                            }
                        } else if (target == "ApisMod")
                        {
                            switch (type)
                            {
                                case "A00": // Someone wants to know if we are using ApisMod
                                    if (data[1] == Player.prop_Player_0.field_Private_APIUser_0.displayName)
                                    {
                                        SendRPC(KiraiMod.KiraiRPC.SendType.Post, "00", data[0]);

                                        OnPlayerUsingApis(data[0]);
                                    }
                                    break;
                                case "A01": // Someone wants to be our slave
                                    if (data[1] == Player.prop_Player_0.field_Private_APIUser_0.displayName)
                                        ; // Display prompt
                                    break;
                                case "C00": // Telling whoever asked that we are using the mod.
                                    if (data[1] == Player.prop_Player_0.field_Private_APIUser_0.displayName)
                                        OnPlayerUsingApis(data[0]);
                                    break;
                            } 
                        }

                        if (oCallback != null)
                            oCallback.Invoke(target, type, data);
                    });

                }).Invoke();
            } else missingRPC = true;
        }

        private System.Collections.IEnumerator Initialize()
        {
            while (ReferenceEquals(NetworkManager.field_Internal_Static_NetworkManager_0, null)) yield return null;

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_ObjectPublicHa1UnT1Unique_1_Player_0
                    .field_Private_HashSet_1_UnityAction_1_T_0
                    .Add(new System.Action<Player>(player => OnPlayerJoined(player)));
                MelonLogger.Log("Hooking OnPlayerJoined... Passed");
            }
            catch { MelonLogger.LogWarning("Hooking OnPlayerJoined... Failed"); }

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_ObjectPublicHa1UnT1Unique_1_Player_1
                    .field_Private_HashSet_1_UnityAction_1_T_0
                    .Add(new System.Action<Player>(player => OnPlayerLeft(player)));
                MelonLogger.Log("Hooking OnPlayerLeft... Passed");
            }
            catch { MelonLogger.LogWarning("Hooking OnPlayerLeft... Failed"); }
        }

        public override void OnUpdate()
        {
            menu?.HandlePages();
        }

        public override void OnModSettingsApplied()
        {
            int masterCount = MelonPrefs.GetInt("ApisMod", "MasterCount");
            int slaveCount = MelonPrefs.GetInt("ApisMod", "MasterCount");

            masters.Clear();
            slaves.Clear();

            for (int i = 0; i < masterCount; i++)
            {
                masters.Add(MelonPrefs.GetString("ApisMod", "Master" + i));
            }

            for (int i = 0; i < slaveCount; i++)
            {
                slaves.Add(MelonPrefs.GetString("ApisMod", "Slave" + i));
            }
        }

        public override void VRChat_OnUiManagerInit()
        {
            if (missingRPC) VRCUiManager.field_Protected_Static_VRCUiManager_0.Method_Public_Void_String_PDM_0("ApisMod is missing dependency KiraiRPC");
            else
            {
                menu = new KiraiMod.Menu();

                menu.CreatePage("ApisModControls");
                menu.CreatePage("ApisModMasters");
                menu.CreatePage("ApisModSlaves");

                menu.CreateButton("apis/set-master", "Request\nMaster", "Ask this player to become your master", -2f, 2f, menu.um.transform, new System.Action(() =>
                {
                    var SendRPC = KiraiMod.KiraiRPC.GetCallback("ApisMod");

                    SendRPC(KiraiMod.KiraiRPC.SendType.Get, "01", KiraiMod.Utils.GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id).field_Private_APIUser_0.displayName);
                }));

                menu.CreateButton("apis-sm/apismod-controls", "ApisMod\nControls", "Accept/Decline requests and remove existing", -2f, 2f, menu.sm.transform, new System.Action(() =>
                {
                    menu.selected = 0;
                }));

                menu.CreateButton("apis-um/close", "Close", "Return to the Quick Menu", -2f, 2f, menu.pages[0].transform, new System.Action(() =>
                {
                    menu.selected = -1;
                }));

                menu.CreateButton("apis-0/view-masters", "View\nMasters", "", -1f, 1f, menu.pages[0].transform, new System.Action(() =>
                {
                    FetchMasters();
                    menu.selected = 1;
                }));

                menu.CreateButton("apis-0/view-slaves", "View\nSlaves", "", 0f, 1f, menu.pages[0].transform, new System.Action(() =>
                {
                    menu.selected = 2;
                }));

                menu.CreateButton("apis-1/back", "Back", "Go back to the controls page", -2f, 2f, menu.pages[1].transform, new System.Action(() =>
                {
                    menu.selected = 0;
                }));

                menu.CreateButton("apis-2/back", "^", "Scroll up", 3f, 1f, menu.pages[1].transform, new System.Action(() =>
                {
                    currentPage -= currentPage <= 0 ? 0 : 1;
                    FetchMasters();
                }));

                menu.CreateButton("apis-1/next", "v", "Scroll down", 3f, 0f, menu.pages[1].transform, new System.Action(() =>
                {
                    currentPage++;
                    FetchMasters();
                }));
            }
        }

        private void OnPlayerUsingApis(string player)
        {
            MelonLogger.Log($"{player} is using ApisMod");
        }

        private void OnPlayerJoined(Player player)
        {
            if (master == null)
                for (int x = 0; x < masters.Count; x++)
                    if (masters[x] == player.field_Private_APIUser_0.displayName)
                        master = player;
        }

        private void OnPlayerLeft(Player player)
        {
            if (player == master)
            {
                master = null;
                for (int x = 0; x < masters.Count; x++)
                    for (int y = 0; y < PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count; y++)
                        if (masters[x] == PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[y].field_Private_APIUser_0.displayName)
                            master = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0[y];
            }
        }

        private void FetchMasters()
        {
            for (int i = 0; i < masterButtons.Length; i++)
            {
                MelonLogger.Log((masterButtons[i] == null).ToString());
                if (masterButtons[i] != null)
                    Object.Destroy(masterButtons[i]);

                GameObject gameObject = new GameObject((i + (masterButtons.Length * currentPage) + masterButtons.Length).ToString());

                masterButtons[i] = gameObject;

                var text = gameObject.AddComponent<UnityEngine.UI.Text>();
                var button = gameObject.AddComponent<UnityEngine.UI.Button>();

                gameObject.transform.SetParent(menu.pages[1].transform, false);
                gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1600, 100);
                gameObject.transform.localPosition = new Vector3(0, 1600 - i * 100);

                text.color = KiraiMod.Utils.Colors.white;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
                text.alignment = TextAnchor.MiddleLeft;
                text.fontSize = 54;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.supportRichText = true;

                string str;
                if (i + currentPage * masterButtons.Length < masters.Count)
                    str = masters[i + currentPage * masterButtons.Length];
                else 
                    str = "";

                text.text = $"{i + (currentPage * masterButtons.Length) + 1}: {str}";

                button.onClick.AddListener(new System.Action(() =>
                {
                    MelonLogger.Log("clicked: " + (i + currentPage * masterButtons.Length));
                }));
            }
        }
    }
}
