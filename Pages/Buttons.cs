﻿using UnityEngine;
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
                VRC_Pickup[] pickups = Object.FindObjectsOfType<VRC_Pickup>();
                for (int i = 0; i < pickups.Length; i++)
                {
                    pickups[i].DisallowTheft = false;
                }
            }));

            Shared.menu.CreateButton("p2/portal", "Portal", "Portals the targeted player", 0f, 1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Helper.PortalPlayer(Shared.targetPlayer, Shared.modules.portal.distance, Shared.modules.portal.infinite);
            }));
            
            Shared.menu.CreateButton("p2/delete-portals", "Delete\nPortals", "Deletes all non-static portals", 1f, 1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Helper.DeletePortals();
            }));

            Shared.menu.CreateButton("p2/fast-pickups", "Fast Pickups", "Thrown pickups are very fast", -1f, 0f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                VRC_Pickup[] pickups = Object.FindObjectsOfType<VRC_Pickup>();
                for (int i = 0; i < pickups.Length; i++)
                {
                    pickups[i].ThrowVelocityBoostScale = 5f;
                }
            }));

            Shared.menu.CreateButton("p2/refresh-kos", "Refresh KOS", "Refreshes KOS list and scans lobby", 0f, 0f, Shared.menu.pages[2].transform, new System.Action(() => 
            {
                Shared.modules.kos.RefreshList();
                Shared.modules.kos.RefreshStatus();
                Shared.modules.nameplates.Refresh();
            }));

            Shared.menu.CreateButton("p2/save", "Save", "Save configuration to disk", 2f, 1f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Shared.config.Save();
            }));

            Shared.menu.CreateButton("p2/load", "Load", "Load configuration from disk", 2f, 0f, Shared.menu.pages[2].transform, new System.Action(() =>
            {
                Shared.config.Load();
            }));
        }
    }
}