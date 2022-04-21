﻿using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using VRC;

namespace KiraiMod
{
    public class Utils
    {
        public static float fRGBSpeed = 1f;

        public static class Colors
        {
            public readonly static Color primary = new Color(0.34f, 0f, 0.65f);
            public readonly static Color highlight = new Color(0.8f, 0.8f, 1f);

            public readonly static Color legendary = new Color(0f, 0.66f, 1f);
            public readonly static Color veteran = new Color(1f, 0.82f, 0f);
            public readonly static Color trusted = new Color(0.75f, 0.26f, 0.9f);
            public readonly static Color known = new Color(1f, 0.48f, 0.25f);
            public readonly static Color user = new Color(0.17f, 0.81f, 0.36f);
            public readonly static Color newuser = new Color(0.09f, 0.47f, 1f);
            public readonly static Color visitor = new Color(0.8f, 0.8f, 0.8f);

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
            Object.Destroy(transform.gameObject);
        }

        public static System.Collections.IEnumerator DestroyDelayed(float seconds, Object obj)
        {
            yield return new WaitForSeconds(seconds);
            Object.Destroy(obj);
        }

        public static void LogGO(GameObject go, int max = -1, int? n_depth = null)
        {
            int depth = n_depth ?? 0;
            if (max != -1 && depth > max) return;

            MelonLogger.Log(System.ConsoleColor.Green, "".PadLeft(depth * 4, ' ') + go.name);

            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                MelonLogger.Log(
                    System.ConsoleColor.Cyan,
                    "".PadLeft((depth + 1) * 4, ' ') +
                    ((go.name.Length + 2 < components[i].ToString().Length) ?
                    components[i].ToString().Substring(
                        go.name.Length + 2,
                        components[i].ToString().Length - go.name.Length - 3
                    ) : components[i].ToString())
                );
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                LogGO(go.transform.GetChild(i).gameObject, max, depth + 1);
            }
        }

        public static Player GetPlayer(string id)
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

        public static void HUDMessage(string message)
        {
            if (VRCUiManager.field_Protected_Static_VRCUiManager_0 == null) return;

            VRCUiManager.field_Protected_Static_VRCUiManager_0.Method_Public_Void_String_PDM_0(message);
        }

        public static Color GetRainbow()
        {
            return new Color((float)System.Math.Sin(fRGBSpeed * Time.time) * 0.5f + 0.5f,
                (float)System.Math.Sin(fRGBSpeed * Time.time + (2 * 3.14 / 3)) * 0.5f + 0.5f,
                (float)System.Math.Sin(fRGBSpeed * Time.time + (4 * 3.14 / 3)) * 0.5f + 0.5f);
        }

        public static void GetGenericLayout(int i, out int x, out int y)
        {
            x = i % 4 - 1;
            y = 1 - i / 4;
        }

        public static string CreateID(string name, int page)
        {
            return $"p{page}/{name.ToLower().Replace('\n', '-').Replace(' ', '-')}";
        }

        public static void Overflow()
        {
            Overflow();
        }

        //public static class Unsafe
        //{
        //    [DllImport("ntdll.dll")]
        //    public static extern uint (int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);
        //    [DllImport("ntdll.dll")]
        //    public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, System.IntPtr Parameters, uint ValidResponseOption, out uint Response);
        //    public static unsafe void Kill()
        //    {
        //        MelonModLogger.Log("Func Stub");
        //        //System.Boolean tmp1;
        //        //uint tmp2;
        //        //RtlAdjustPrivilege(19, true, false, out tmp1);
        //        //NtRaiseHardError(0xc0000022, 0, 0, System.IntPtr.Zero, 6, out tmp2);
        //    }
        //}
    }
}