﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KiraiMod.Modules
{
    public class HideSelf : ModuleBase
    {
        private Transform head;

        public string messageToYou;

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Hide Self", "Hide your nameplate and make yourself unclickable", ButtonType.Toggle, 6, Menu.PageIndex.options1, nameof(state))
        };

        bool changed;

        public override void OnStateChange(bool state)
        {
            if (head is null)
            {
                head = VRCVrCamera.field_Private_Static_VRCVrCamera_0.transform.parent;

                messageToYou = "you fags should stop stealing my code and concepts. " +
                    "im talking to day for my buttons in hostelgang client, " +
                    "fiass for mute self in fclient, " +
                    "the emm team for menu unloading, " +
                    "and finally the creator of OldMate for stealing name protection (and fucking it up in the process, gj on that).";
                messageToYou = null;
            }

            VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position += new Vector3(0, state ? -4 : 4, 0);

            MelonLoader.MelonLogger.Log($"{head.parent.localScale.x} {head.parent.localScale.y} {head.parent.localScale.z}");

            if (state)
            {
                if (!Shared.modules.noclip.state)
                {
                    Shared.menu.Set(Utils.CreateID("noclip", Shared.modules.noclip.info[0].page), true);
                    changed = true;
                }
                else 
                    changed = false;

                head.localPosition += new Vector3(0, 4 / head.parent.transform.localScale.y, 0);
            }
            else
            {
                head.localPosition = Vector3.zero;

                if (changed)
                    Shared.menu.Set(Utils.CreateID("noclip", Shared.modules.noclip.info[0].page), false);
            }
        }
    }
}