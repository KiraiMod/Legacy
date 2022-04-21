using MelonLoader;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[assembly: MelonInfo(typeof(KiraiMod.KiraiFriends), "KiraiFriends", "1", "Kirai Chan#8315")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace KiraiMod
{
    public class KiraiFriends : MelonMod
    {
        private bool active = true;
        private bool notifs = true;
        private bool connected = false;

        private HttpClient client;
        private List<string[]> online;

        public string name;
        public Dictionary<string, string> config = new Dictionary<string, string>();

        private readonly string nameFile = "kiraimod.name.txt";
        private readonly string configFile = "kiraimod.friends.json";

        public override void OnApplicationStart()
        {
            if (System.IO.File.Exists(nameFile))
                name = System.IO.File.ReadAllText(nameFile);
            else name = Environment.UserName;

            if (System.IO.File.Exists(configFile))
            {
                try
                {
                    config = JSON.Load(System.IO.File.ReadAllText(configFile)).Make<Dictionary<string, string>>();

                    if (config.TryGetValue("name", out string username))
                        name = username;

                    if (config.TryGetValue("online", out string visibility))
                    {
                        if (visibility.ToLower() == "false") active = false;
                        else active = true;
                    }

                    if (config.TryGetValue("notifs", out string notifications))
                    {
                        if (notifications.ToLower() == "false") notifs = false;
                        else notifs = true;
                    }
                }
                catch { MelonLogger.Msg("Malformed KiraiMod.config.json"); }
            }

            client = new HttpClient();
            client.BaseAddress = new Uri("http://5306aadf57b2262f40c.ddns.net:53066/friends/");

            MelonCoroutines.Start(KeepAlive());
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Pause) || Input.GetKeyDown(KeyCode.Break)) HUDInput("Join Kirai Friend by name", "Join", "Kirai Chan", "", new Action<string>((value) =>
            {
                string[] selected = null;
                foreach (string[] user in online)
                    if (user[0] == value) selected = user;

                if (selected is null)
                    HUDMessage("User is not online");
                else 
                    JoinWorldById(selected[1]);
            }));
        }

        public override void OnSceneWasLoaded(int level, string sceneName)
        {
            if (level == -1 && active)
                MelonCoroutines.Start(SetLocationDelayed());
        }

        public override void OnGUI()
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                GUI.Label(new Rect(10, 10, 150, 20), $"Welcome, {name}");
                GUI.Label(new Rect(10, 25, 150, 20), $"Currently {(connected ? active ? "<color=lime>Online" : "<color=red>Offline" : "<color=aqua>Disconnected")}</color>");
                GUI.Label(new Rect(10, 40, 150, 20), "Redistribution forbidden");

                float stack = 2.5f;

                if (active)
                {
                    if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Go Offline"))
                    {
                        active = false;
                        WriteToConfig();
                        SetLocation("undefined");
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Go Online"))
                    {
                        active = true;
                        WriteToConfig();
                        SetLocation();
                    }
                }

                if (notifs)
                {
                    if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Disable Notifications"))
                    {
                        notifs = false;
                        WriteToConfig();
                    }
                }
                else if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Enable Notifications"))
                {
                    notifs = true;
                    WriteToConfig();
                }

                if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Refresh"))
                {
                    RefreshOnline();
                }

                if (online != null && online.Count > 0)
                {
                    for (int i = 0; i < online.Count; i++)
                    {
                        if (GUI.Button(new Rect(10, stack++ * 20 + 15, 150, 20), online[i][0]))
                            JoinWorldById(online[i][1]);
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(10, 10, 150, 20), $"{(connected ? active ? "<color=lime>Online" : "<color=red>Offline" : "<color=aqua>Disconnected")}</color>");
            }
        }

        private System.Collections.IEnumerator KeepAlive()
        {
            for (; ; )
            {
                RefreshOnline();
                client.PostAsync($"keep-alive?name={Uri.EscapeDataString(name)}", null).ContinueWith(new Action<System.Threading.Tasks.Task<HttpResponseMessage>>(msg =>
                {
                    if (msg.Exception == null) connected = true;
                    else connected = false;
                }));

                yield return new WaitForSeconds(5);
            }
        }

        private System.Collections.IEnumerator SetLocationDelayed()
        {
            while (RoomManager.field_Internal_Static_ApiWorld_0 is null) yield return new WaitForSeconds(1);

            SetLocation();
        }

        public void RefreshOnline()
        {
            client.GetAsync("online").ContinueWith(new Action<System.Threading.Tasks.Task<HttpResponseMessage>>(msg =>
            {
                if (msg.Exception == null)
                {
                    connected = true;
                    msg.Result.Content.ReadAsStringAsync().ContinueWith(new Action<System.Threading.Tasks.Task<string>>((response) =>
                    {
                        var prevOnline = online;
                        online = JSON.Load(response.Result).Make<List<string[]>>();

                        if (!notifs || prevOnline is null) return;

                        string[] a = new string[prevOnline.Count];
                        string[] b = new string[online.Count];

                        for (int i = 0; i < prevOnline.Count; i++)
                            a[i] = prevOnline[i][0];

                        for (int i = 0; i < online.Count; i++)
                            b[i] = online[i][0];

                        var loggedIn = b.Except(a);
                        var loggedOut = a.Except(b);

                        foreach (var user in b.Except(a))
                            HUDMessage($"{user} logged in");

                        foreach (var user in a.Except(b))
                            HUDMessage($"{user} logged out");

                        if (active && !online.Any(u => u[0] == name))
                            SetLocation();
                    }));
                }
                else connected = false;
            }));
        }

        public static void HUDMessage(string message)
        {
            if (VRCUiManager.prop_VRCUiManager_0 == null) return;

            VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_0(message);
        }

        public static void HUDInput(string title, string text, string placeholder, string initial, Action<string> OnAccept)
        {
            VRCUiPopupManager
                .field_Private_Static_VRCUiPopupManager_0
                .Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_PDM_0
                (
                    title,
                    initial,
                    InputField.InputType.Standard,
                    false,
                    text,
                    UnhollowerRuntimeLib
                        .DelegateSupport
                        .ConvertDelegate<
                            Il2CppSystem.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>
                        >(
                            new System.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text>(
                                (a, b, c) =>
                                {
                                    OnAccept(a);
                                }
                            )
                        ),
                    null,
                    placeholder,
                    true,
                    null
                );
        }

        private void SetLocation(string replace = "")
        {
            string wrld = $"{RoomManager.field_Internal_Static_ApiWorld_0.id}:{RoomManager.field_Internal_Static_ApiWorld_0.currentInstanceIdWithTags}";
            client.PostAsync($"set-location?name={Uri.EscapeDataString(name)}&location={Uri.EscapeDataString(replace == "" ? wrld : "undefined")}", null)
                .ContinueWith(new Action<System.Threading.Tasks.Task<HttpResponseMessage>>(msg =>
                {
                    if (msg.Exception == null) connected = true;
                    else connected = false;
                }));
        }

        public static bool JoinWorldById(string id)
        {
            if (!Networking.GoToRoom(id))
            {
                string[] split = id.Split(':');

                if (split.Length != 2) return false;

                new PortalInternal().Method_Private_Void_String_String_PDM_0(split[0], split[1]);
            }

            return true;
        }

        private void WriteToConfig()
        {
            config["online"] = active.ToString();
            config["notifs"] = notifs.ToString();

            System.IO.File.WriteAllText(configFile, JSON.Dump(config));
        }
    }
}
