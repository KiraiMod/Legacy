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
            KiraiLib.UI.Button.Create("p3/open-p4", "More", "Reveal more options", -2f, 2f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.buttons2];
            }));

            KiraiLib.UI.Button.Create("p3/open-p5", "Next", "Opens KiraiMod's next page", -2f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.sliders1];
            }));

            KiraiLib.UI.Button.Create("p3/close-p3", "Previous", "Opens KiraiMod's previous page", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.toggles1];
            }));

            KiraiLib.UI.Button.Create("p3/save", "Save", "Save configuration to disk", -1f, -1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons1]].transform, new System.Action(() =>
            {
                Config.Save();
            }));

            KiraiLib.UI.Button.Create("p3/load", "Load", "Load configuration from disk", 0f, -1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons1]].transform, new System.Action(() =>
            {
                Config.Load();
            }));

            KiraiLib.UI.Button.Create("p3/crash-self", "Crash Self", "Manually initiate a crash to GTFO", 2f, -1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons1]].transform, new System.Action(() =>
            {
                MelonLogger.Msg("vvvvvvvvvvvvvvvvvvvvvvvvv");
                MelonLogger.Msg("Manually Initiated Crash.");
                MelonLogger.Msg("^^^^^^^^^^^^^^^^^^^^^^^^^");

                string[] urls = {
                    "https://youtu.be/G9LXGvr7Nyc",
                    "https://youtu.be/cDqytGXoXWI",
                    "https://youtu.be/mwnu2aP0Q8g",
                    "https://youtu.be/eEuIHatfgkE"
                };

                System.Diagnostics.Process.Start(urls[UnityEngine.Random.Range(0, urls.Length)]);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }));

            KiraiLib.UI.Button.Create("p4/drop-all", "Drop All", "Drop every pickup in the world", -1f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons2]].transform, new Action(() =>
            {
                foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                {
                    if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer) Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                    pickup.Drop();
                }
            }));

            KiraiLib.UI.Button.Create("p4/use-all", "Use All", "Use all triggers in the world.", 0f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons2]].transform, new Action(() =>
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

            KiraiLib.UI.Button.Create("p4/crash-selected", "Crash Selected", "Crash the selected player", 1f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons2]].transform, new Action(() =>
            {
                Helper.CrashSelected();
            }));

            KiraiLib.UI.Button.Create("buttons2/force-jump", "Force Jump", "Allows yourself to jump in a world without it", -1, 0, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons2]].transform, new Action(() =>
            {
                Networking.LocalPlayer.SetJumpImpulse(3);
            }));

            KiraiLib.UI.Button.Create("buttons2/classic-movement", "Classic\nMovement", "Use old VRChat movement in SDK3 worlds", 0, 0, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons2]].transform, new Action(() =>
            {
                Networking.LocalPlayer.UseLegacyLocomotion();
            }));

            KiraiLib.UI.Button.Create("p4/open-p5", "Next", "Opens KiraiMod's next page", -2f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons2]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.sliders1];
            }));

            KiraiLib.UI.Button.Create("p4/close-p4", "Back", "Close KiraiMod's extra options", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.buttons2]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.buttons1];
            }));
        }
    }
}