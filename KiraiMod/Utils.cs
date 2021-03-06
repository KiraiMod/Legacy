using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace KiraiMod
{
    public class Utils
    {
        public static class Colors
        {
            public readonly static Color primary = new Color(0.34f, 0f, 0.65f);
            public readonly static Color highlight = new Color(0.8f, 0.8f, 1f);

            public readonly static Color black = new Color(0.0f, 0.0f, 0.0f);
            public readonly static Color red = new Color(1.0f, 0.0f, 0.0f);
            public readonly static Color green = new Color(0.0f, 1.0f, 0.0f);
            public readonly static Color aqua = new Color(0.0f, 1.0f, 1.0f);
            public readonly static Color orange = new Color(1.0f, 0.65f, 0.0f);
            public readonly static Color white = new Color(1.0f, 1.0f, 1.0f);
        }

        public static void DestroyRecursive(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                DestroyRecursive(transform.GetChild(i));
            }
            UnityEngine.Object.Destroy(transform.gameObject);
        }

        public static System.Collections.IEnumerator DestroyDelayed(float seconds, UnityEngine.Object obj)
        {
            yield return new WaitForSeconds(seconds);
            UnityEngine.Object.Destroy(obj);
        }

        public static void LogKeywords(System.Reflection.MethodBase method)
        {
            int i = 0, j = 0;

            foreach (UnhollowerRuntimeLib.XrefScans.XrefInstance instance in UnhollowerRuntimeLib.XrefScans.XrefScanner.XrefScan(method))
            {
                i++;
                if (instance.Type == UnhollowerRuntimeLib.XrefScans.XrefType.Global)
                {
                    j++;
                    MelonLogger.Msg(instance.ReadAsObject()?.ToString());
                }
            }

            MelonLogger.Msg($"Found {i} objects and {j} keywords");
        }

        public static Player GetPlayer(string name)
        {
            List<Player> players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].field_Private_APIUser_0.displayName == name)
                {
                    return players[i];
                }
            }

            return null;
        }

        public static Player GetPlayerByID(string id)
        {
            List<Player> players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].field_Private_APIUser_0.id == id)
                {
                    return players[i];
                }
            }

            return null;
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

        public static void SelectPlayer(Player user)
        {
            QuickMenu.prop_QuickMenu_0.Method_Public_Void_Player_0(user);
        }

        public static void GetGenericLayout(int i, out int x, out int y)
        {
            x = i % 4 - 1;
            y = 1 - i / 4;
        }

        public static void GetSliderLayout(int i, out float x, out float y)
        {
            x = (float)(-0.75 + i % 2 * 2);
            y = (float)(1.25 - i / 2 * 0.5);
        }

        public static string CreateID(string name, int page)
        {
            return $"p{page}/{name.ToLower().Replace('\n', '-').Replace(' ', '-')}";
        }

        public static string GetClipboard()
        {
            if (System.Windows.Forms.Clipboard.ContainsText())
                return System.Windows.Forms.Clipboard.GetText();
            return null;
        }
        
        public static readonly string StringIya = "\r\tyyyyyyyyyhshosmMMMMMMN:NNMMMMMMMMMMMMMMMMMMMMms/syyyyyyyyyyyyy\n\ty/.---../+yMMMMMMMMMmsyMMMMMMMMMMMMMMMMMMMMMMMMMy+yyyyyyyyyyyy\n\ty-yyyyoNo.++yyyyhhhhmMMMMMMMMMMMMMMMMMMMMMMMMMMMMMyoyyyyyyyyyy\n\tysoyyyshs/s`/+ssssssyhmMMMMMMMMMMMMMMMMMMMMMMMMMMMMNsyyyyyyyyy\n\tyyyosyhohy:sMMMMMMMmNNhssyhNMMMMMMMMMMMMMMMMMMMMMMMMMdyyyyyyyy\n\tyyyyyoos+:dmMMMMMMMmyNdNNMmy+odMMMMMMMMMMMMMMMMMMMMMMN+yyyyyyy\n\tyyyyyyys+sy+smMMMMmd//oydmNmhsy//yNMMMMMMMMMMMMMMMMMN:/yyyyyyy\n\tyyyyyyyy-ymhdhhhdmdmd+dMMdyss/sMNs`/yMMMMMMMMMMMMMMN:`+yyyyyyy\n\tyyyyyyyy/oMNsMNmmmmmh/mMMydNh:.sNM.ys-omMMMMMMMMMMN:``oyyyyyyy\n\tyyyyyyysdoMMsoNMMMMMNm/NdNs-/oo:-d:sMNs`/dMMMMMMMd:-:/yyyyyyyy\n\tyyyyyyysmNhMM/.yMmNMMMMyyMhmsyy/mNo/MMyd-`/mMMMMh/+oo+hyyyyyyy\n\tyyyyyyysNMymNm``:hMMMMMMhohMNddyMyMydmmMy`:`hMNyossyy+yyyyyyyy\n\tyyyyyyyoMMy:mMh`:hoyydMMMhNMMMMhyhMMdmMMm.m..Myyyhyhssyyyyyyyy\n\tyyyyyyyoMNs`.hy+:-oMMhhhdMMMMMMsyoMmMMMMN-N`sdddhddm+yyyyyyyyy\n\tyyyyyyyoMy`  sh. ` /dNMMMMMMMMNdNsMmMMMMdos:mhmmmmm+oyyyyyyyyy\n\tyyyyyyyyh.   `md.  -:MMMMMMMMMddm+MdMMMM+o`dhmmmdh/-yyyyyyyyyy\n\tyyyyyyysy+`   -MN:.`/-shNMMMNMMNm+MyMMdh``yhmdyydh`syyyyyyyyyy\n\tyyyyyyyyys:    /Mmhs:.``..:+so-`:oN-MNo-`+M+hdMMm`-yyyyyyyyyyy\n\tyyyyyyyyyys.    /dsNNo-.--.:/+oo+h/+m+:+:MMMMMMm.  -syyyyyyyyy\n\tyyyyyyyyyyyy/`   -s-smd/:+`/++++:+/s+oo:NMMMMMh.+d:``+hyyyyyyy\n\tyyyyyyyyyyyyyy+:` -/-.:o//-+:o+/osooyy/NMMMMMo/mMMMh/.:yyyyyyy\n\tyyyyyyyyyyyyyyyso.----:/+/-++/o++osss/mMMMMdsmMMMMMMMy--yyyyyy\n\tyyyyyyyyyyyyyyyyyo:--://s/-++oosyyys+NMMMmmNMMMMMMMMMMN+-yyyyy\n\tyyyyyyyyyyyyyyyyy+:/:/+os+.ooo+///:oMMNmmMMMMMMMMMMMMMMMy-yyyy\n\tyyyyyyyyyyyyyyyyy++/++ooss.`-: -hshMdhNMMMMMMMMMMMMMMMMMMd:yyy\n\tyyyyyyyyyyyyyyyyy+++/++sss/y-` :hmMMMMMMMMMMMMMMMMMMMMMMMMm/yy\n\tyyyyyyyyyyyyyyyyyoo+/o+oyy//  .hMMMMMMMMMMMMMMMMMMMMMMMMMMMh+y\n\tyyyyyyyyyyyyyyyyyoo++`..+h:.:hMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMos\n\tyyyyyyyyyyyyyyyyyos/.oyys:/+hMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN/\n\tyyyyyyyyyyyyyyyyyos::yoo:+-yMMMMMMMMMMMMMMMMMMMMMMMMMmNmyydMMo\n\tyyyyyyyyyyyyyyyyyoy:+yss.++mMMMMMMMMMMMMdMMMMMMMNmmmNMMMMmhshs\n\t";

        public static class Unsafe
        {
            [DllImport("ntdll.dll")]
            public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);
            [DllImport("ntdll.dll")]
            public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);
            public static unsafe void Kill()
            {
                RtlAdjustPrivilege(19, true, false, out _);
                NtRaiseHardError(0xc0000022, 0, 0, IntPtr.Zero, 6, out _);
            }
        }

        public static int TSPrintRC = 0;
        private static System.Threading.Mutex TSMutex = new System.Threading.Mutex();
        private static System.Collections.Generic.Queue<string> TSPrintQueue = new System.Collections.Generic.Queue<string>();

        public static System.Collections.IEnumerator TSPrintInit()
        {
            while (TSPrintRC > 0)
            {
                if (TSPrintQueue.Count == 0)
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                while (TSPrintQueue.Count != 0)
                    MelonLogger.Msg(TSPrintQueue.Dequeue());
            }
        }

        public static void TSPrint(string msg)
        {
            TSMutex.WaitOne();

            TSPrintQueue.Enqueue(msg);

            TSMutex.ReleaseMutex();
        }
    }
}