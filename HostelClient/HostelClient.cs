using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Windows.Forms;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace KiraiMod
{
    public class HostelClient : MelonMod
    {
        private SoundPlayer alert;

        public string[] streamers = new string[0];

        public override void OnApplicationStart()
        {
            alert = new SoundPlayer(Assembly.GetManifestResourceStream("HostelClient.alert.wav"));

            RefreshList();
            MelonCoroutines.Start(InitializeHooks());
        }

        private void RefreshList()
        {
            try
            {
                string data = new StreamReader(((HttpWebResponse)WebRequest
                    .Create("https://raw.githubusercontent.com/xKiraiChan/xKiraiChan/main/Streamers.txt")
                    .GetResponse())
                    .GetResponseStream())
                    .ReadToEnd();

                if (data[data.Length - 1] == '\n')
                    data = data.Remove(data.Length - 1);

                streamers = data.Split('\n');
                MelonLogger.Log("Downloaded streamer list with " + streamers.Length + " streamers");
            }
            catch { MelonLogger.LogWarning("Failed to download streamer list."); }
        }

        private System.Collections.IEnumerator InitializeHooks()
        {
            while (NetworkManager.field_Internal_Static_NetworkManager_0 is null) yield return null;

            try
            {
                NetworkManager
                    .field_Internal_Static_NetworkManager_0
                    .field_Internal_ObjectPublicHa1UnT1Unique_1_Player_0
                    .field_Private_HashSet_1_UnityAction_1_T_0
                    .Add(new Action<Player>(player => OnPlayerJoined(player)));
                LogWithPadding("OnPlayerJoined", true);
            }
            catch { LogWithPadding("OnPlayerJoined", false); }
        }

        private void OnPlayerJoined(Player player)
        {
            if (player != null && streamers.Contains(Utils.SHA256(player.field_Private_APIUser_0.id)))
            {
                Utils.HUDMessage($"Streamer {player.field_Private_APIUser_0.displayName} joined.");
                MelonLogger.Log($"Streamer {player.field_Private_APIUser_0.displayName} joined.");

                alert.Play();
            }
        }

        private static void LogWithPadding(string src, bool passed)
        {
            MelonLogger.Log($"Hooking {src}...".PadRight(69, ' ') + (passed ? "Passed" : "Failed"));
        }
    }

    public static class Utils
    {
        public static class Colors
        {
            public static readonly Color black = new Color(0, 0, 0);
        }

        public static string SHA256(string input)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(input));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public static void HUDMessage(string message)
        {
            if (VRCUiManager.prop_VRCUiManager_0 == null) return;

            VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_String_0(message);
        }
    }
}
