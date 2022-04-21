using MelonLoader;
using System;
using UnityEngine;
using VRC.SDKBase;
using System.Linq;

namespace KiraiMod.Pages
{
    public class Buttons
    {
        public Buttons()
        {
            Shared.menu.CreateButton("p3/open-p4", "More", "Reveal more options", -2f, 2f, Shared.menu.pages[(int)Menu.PageIndex.buttons1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 4;
            }));

            Shared.menu.CreateButton("p3/open-p5", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[(int)Menu.PageIndex.buttons1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 5;
            }));

            Shared.menu.CreateButton("p3/close-p3", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[(int)Menu.PageIndex.buttons1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 0;
            }));

            Shared.menu.CreateButton("p3/save", "Save", "Save configuration to disk", -1f, -1f, Shared.menu.pages[(int)Menu.PageIndex.buttons1].transform, new System.Action(() =>
            {
                Shared.config.Save();
            }));

            Shared.menu.CreateButton("p3/load", "Load", "Load configuration from disk", 0f, -1f, Shared.menu.pages[(int)Menu.PageIndex.buttons1].transform, new System.Action(() =>
            {
                Shared.config.Load();
            }));

            Shared.menu.CreateButton("p3/crash-self", "Crash Self", "Manually initiate a crash to GTFO", 2f, -1f, Shared.menu.pages[(int)Menu.PageIndex.buttons1].transform, new System.Action(() =>
            {
                MelonLogger.Log("vvvvvvvvvvvvvvvvvvvvvvvvv");
                MelonLogger.Log("Manually Initiated Crash.");
                MelonLogger.Log("^^^^^^^^^^^^^^^^^^^^^^^^^");
                System.Diagnostics.Process.Start("https://youtu.be/sWTYu8e3MK4");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }));

            Shared.menu.CreateButton("p4/drop-all", "Drop All", "Drop every pickup in the world", -1f, 1f, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new Action(() =>
            {
                foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                {
                    if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer) Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                    pickup.Drop();
                }
            }));

            Shared.menu.CreateButton("p4/use-all", "Use All", "Use all triggers in the world.", 0f, 1f, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new Action(() =>
            {
                Vector3 oPos = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position;
                Quaternion oRot = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.rotation;
                foreach (VRC_Trigger trigger in UnityEngine.Object.FindObjectsOfType<VRC_Trigger>())
                {
                    if (Networking.GetOwner(trigger.gameObject) != Networking.LocalPlayer) Networking.SetOwner(Networking.LocalPlayer, trigger.gameObject);
                    trigger.Interact();
                }
                VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position = oPos;
                VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.rotation = oRot;
            }));

            Shared.menu.CreateButton("p4/crash-selected", "Crash Selected", "Crash the selected player", 1f, 1f, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new Action(() =>
            {
                Helper.CrashSelected();
            }));

            Shared.menu.CreateButton("buttons2/force-jump", "Force Jump", "Allows yourself to jump in a world without it", -1, 0, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new Action(() =>
            {
                Networking.LocalPlayer.SetJumpImpulse(3);
            }));

            Shared.menu.CreateButton("buttons2/classic-movement", "Classic\nMovement", "Use old VRChat movement in SDK3 worlds", 0, 0, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new Action(() =>
            {
                Networking.LocalPlayer.UseLegacyLocomotion();
            }));

            Shared.menu.CreateButton("p4/open-p5", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new System.Action(() =>
            {
                Shared.menu.selected = 5;
            }));

            Shared.menu.CreateButton("p4/close-p4", "Back", "Close KiraiMod's extra options", -2f, 0f, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new System.Action(() =>
            {
                Shared.menu.selected = 3;
            }));
        }
    }
}