﻿using MelonLoader;
using System;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Pages
{
    public class Buttons
    {
        public Buttons()
        {
            Shared.menu.CreateButton("p2/open-p3", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Shared.menu.selected = 3;
            }));

            Shared.menu.CreateButton("p2/close-p2", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Shared.menu.selected = 0;
            }));

            Shared.menu.CreateButton("p2/force-pickups", "Force\nPickups", "Enabled theft on all pickups", -1f, 1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                VRC_Pickup[] pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
                for (int i = 0; i < pickups.Length; i++)
                {
                    pickups[i].DisallowTheft = false;
                }
            }));

            Shared.menu.CreateButton("p2/fast-pickups", "Fast Pickups", "Thrown pickups are very fast", 0f, 1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                VRC_Pickup[] pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
                for (int i = 0; i < pickups.Length; i++)
                {
                    pickups[i].ThrowVelocityBoostScale = 5f;
                }
            }));

            Shared.menu.CreateButton("p2/nuke-videosync", "Nuke\nVideoSync", "Overrides all video players to a custom URL", 0f, 0f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                foreach (SyncVideoPlayer svp in UnityEngine.Object.FindObjectsOfType<SyncVideoPlayer>())
                {
                    if (svp == null) continue;
                    Networking.LocalPlayer.TakeOwnership(svp.gameObject);
                    svp.field_Private_VRC_SyncVideoPlayer_0.Stop();
                    svp.field_Private_VRC_SyncVideoPlayer_0.Clear();
                    svp.field_Private_VRC_SyncVideoPlayer_0.AddURL("https://www.youtube.com/watch?v=LhCYW9dKC5s");
                    svp.field_Private_VRC_SyncVideoPlayer_0.Next();
                    svp.field_Private_VRC_SyncVideoPlayer_0.Play();
                }

                foreach (SyncVideoStream svs in UnityEngine.Object.FindObjectsOfType<SyncVideoStream>())
                {
                    if (svs == null) continue;
                    Networking.LocalPlayer.TakeOwnership(svs.gameObject);
                    svs.field_Private_VRC_SyncVideoStream_0.Stop();
                    svs.field_Private_VRC_SyncVideoStream_0.Clear();
                    svs.field_Private_VRC_SyncVideoStream_0.AddURL("https://www.youtube.com/watch?v=LhCYW9dKC5s");
                    svs.field_Private_VRC_SyncVideoStream_0.Next();
                    svs.field_Private_VRC_SyncVideoStream_0.Play();
                }
            }));

            Shared.menu.CreateButton("p2/bring-pickups", "Bring\nPickups", "Brings all pickups in the scene", 1f, 0f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                {
                    Networking.LocalPlayer.TakeOwnership(pickup.gameObject);
                    pickup.transform.localPosition = new Vector3(0, 0, 0);
                    pickup.transform.position = (Shared.targetPlayer?.transform.position ?? VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position) + new Vector3(0, 0.1f, 0);
                }
            }));

            Shared.menu.CreateButton("p2/drop-target", "Drop\nTarget", "Forget the current target", 2f, 0f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Shared.targetPlayer = null;
            }));

            Shared.menu.CreateButton("p2/save", "Save", "Save configuration to disk", -1f, -1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Shared.config.Save();
            }));

            Shared.menu.CreateButton("p2/load", "Load", "Load configuration from disk", 0f, -1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Shared.config.Load();
            }));

            Shared.menu.CreateButton("p2/crash", "Crash", "Manually initiate a crash to GTFO", 2f, -1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                MelonModLogger.Log("vvvvvvvvvvvvvvvvvvvvvvvvv");
                MelonModLogger.Log("Manually Initiated Crash.");
                MelonModLogger.Log("^^^^^^^^^^^^^^^^^^^^^^^^^");
                Utils.Overflow();
            }));
        }
    }
}