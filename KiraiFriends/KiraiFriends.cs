using MelonLoader;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace KiraiMod
{
    public class KiraiFriends : MelonMod
    {
        public string username;

        private WebClient client;
        private bool active = true;
        private bool connected = false;
        private List<string[]> online = new List<string[]>();

        public override void OnApplicationStart()
        {
            username = Environment.UserName;

            client = new WebClient();
            client.QueryString.Set("name", username);

            MelonCoroutines.Start(KeepAlive());
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Q)) VRChat_OnUiManagerInit();
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (level == -1)
                MelonCoroutines.Start(SetLocationDelayed());
        }

        public override void OnGUI()
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                GUI.Label(new Rect(10, 10, 150, 20), $"Welcome, {username}");
                GUI.Label(new Rect(10, 25, 150, 20), $"Currently {(connected ? active ? "<color=lime>Online" : "<color=red>Offline" : "<color=aqua>Disconnected")}</color>");
                GUI.Label(new Rect(10, 40, 150, 20), "Redistribution forbidden");

                float stack = 2.5f;

                if (active)
                {
                    if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Go Offline"))
                    {
                        active = false;
                        SetLocation("undefined");
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Go Online"))
                    {
                        active = true;
                        SetLocation();
                    }
                }

                if (GUI.Button(new Rect(10, stack++ * 20 + 10, 150, 20), "Refresh"))
                {
                    string resp = "";

                    try
                    {
                        resp = client.DownloadString("http://5306aadf57b2262f40c.ddns.net:53066/friends/online");
                        connected = true;
                    } catch { connected = false; }

                    if (resp != "")
                        online = JSON.Load(resp).Make<List<string[]>>();
                }

                if (online.Count > 0)
                {
                    for (int i = 0; i < online.Count; i++)
                    {
                        if (GUI.Button(new Rect(10, stack++ * 20 + 15, 150, 20), online[i][0]))
                            JoinWorldById(online[i][1]);
                    }
                }
            }
        }

        private System.Collections.IEnumerator KeepAlive()
        {
            for (;;)
            {
                try
                {
                    client.UploadValues("http://5306aadf57b2262f40c.ddns.net:53066/friends/keep-alive", "POST", client.QueryString);
                    connected = true;
                }
                catch { connected = false; }

                yield return new WaitForSeconds(25);
            }
        }

        private System.Collections.IEnumerator SetLocationDelayed()
        {
            while (RoomManager.field_Internal_Static_ApiWorld_0 is null) yield return new WaitForSeconds(1);

            SetLocation();
        }

        private void SetLocation(string replace = "")
        {
            client.QueryString.Set("location", replace == "" ? $"{RoomManager.field_Internal_Static_ApiWorld_0.id}:{RoomManager.field_Internal_Static_ApiWorld_0.currentInstanceIdWithTags}" : "undefined");
            
            try
            {
                client.UploadValues("http://5306aadf57b2262f40c.ddns.net:53066/friends/set-location", "POST", client.QueryString);
                connected = true;
            } catch { connected = false; }
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
    }
}
