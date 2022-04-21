using MelonLoader;
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