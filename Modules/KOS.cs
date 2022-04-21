using System.Collections.Generic;
using System.IO;
using System.Net;
using VRC;
using MelonLoader;

namespace KiraiMod.Modules
{
    public class KOS : ModuleBase
    {
        private bool active = false;
        private bool oAuto = false;
        private bool oInfinite = false;
        private Player oTarget = null;

        public string[] kosList = new string[0];
        public List<Player> players = new List<Player>();

        public KOS()
        {
            RefreshList();
        }

        public override void OnPlayerJoined(Player player)
        {
            players.Add(player);
            if (state) RefreshStatus();
        }

        public override void OnPlayerLeft(Player player)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                    i--;
                }
            }
            
            if (state) RefreshStatus();
        }

        public override void OnStateChange(bool state)
        {
            if (state) RefreshStatus();
            else Deactivate();
        }

        public void RefreshList()
        {
            try
            {
                string data = new StreamReader(((HttpWebResponse)WebRequest
                    .Create("https://pastebin.com/raw/pMb9zqQM")
                    .GetResponse())
                    .GetResponseStream())
                    .ReadToEnd();
                kosList = data.Split('\n');
                MelonModLogger.Log("Downloaded KOS list with " + kosList.Length + " users");
            }
            catch
            {
                MelonModLogger.LogWarning("Failed to download KOS list.");
            }
        }

        public void RefreshStatus()
        {
            if (!state) return;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                    i--;
                    continue;
                }

                if (players[i].field_Private_VRCPlayerApi_0 == null) continue;

                if (players[i].IsKOS()) {
                    MelonModLogger.Log("Found user on KOS list");
                    Activate(players[i]);
                    return;
                }
            }
            Deactivate();
        }

        private void Activate(Player target)
        {
            foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.IsMod())
                {
                    MelonModLogger.Log("[KOS] Would have activated but a moderator is in the instance");
                    return;
                }
            }

            if (active) return;
            active = true;

            oTarget = Shared.targetPlayer;
            Shared.targetPlayer = target;

            oAuto = Shared.menu.GetBool("p0/auto-portal") ?? oAuto;
            Shared.menu.Set("p0/auto-portal", true);

            oInfinite = Shared.menu.GetBool("p0/auto-portal") ?? oInfinite;
            Shared.menu.Set("p0/infinite-portals", true);
        }

        private void Deactivate()
        {
            if (!active) return;
            active = false;

            Shared.targetPlayer = oTarget;
            oTarget = null;

            Menu.MenuObject obj;
            if (Shared.menu.objects.TryGetValue("p0/auto-portal", out obj))
            {
                obj.toggle.SetState(oAuto);
                oAuto = false;
            }

            if (Shared.menu.objects.TryGetValue("p0/infinite-portals", out obj))
            {
                obj.toggle.SetState(oInfinite);
                oInfinite = false;
            }

            Helper.DeletePortals();
        }
    }
}
