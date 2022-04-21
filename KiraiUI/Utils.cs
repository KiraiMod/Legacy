using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod
{
    class Utils
    {
        public static float EstimateBlockSize()
        {
            if (QuickMenu.prop_QuickMenu_0 == null) return 0;

            float? fl = QuickMenu.prop_QuickMenu_0
                .transform.Find("UserInteractMenu/ForceLogoutButton")?.localPosition.x;
            
            float? bb = QuickMenu.prop_QuickMenu_0
                .transform.Find("UserInteractMenu/BanButton")?.localPosition.x;

            if (bb == null || fl == null)
                return 0;

            return (float)bb - (float)fl;
        }

        public static void MoveImageColor(int op, ref Image c, ref Color s, Color i)
        {
            if (op == 0) c.color = i;
            else if (op == 1) s = c.color;
            else c.color = s;
        }

        public static void MovePedalGraphicColor(int op, ref PedalGraphic c, ref Color s, Color i)
        {
            if (op == 0) c.color = i;
            else if (op == 1) s = c.color;
            else c.color = s;
        }

        public static void MoveRectTransformSizeDelta(int op, ref RectTransform t, ref Vector2 s, Vector2 i)
        {
            if (op == 0) t.sizeDelta = i;
            else if (op == 1) s = t.sizeDelta;
            else t.sizeDelta = s;
        }

        public static void MoveRectTransformLocalPosition(int op, ref RectTransform t, ref Vector3 s, Vector3 i)
        {
            if (op == 0) t.localPosition = i;
            else if (op == 1) s = t.localPosition;
            else t.localPosition = s;
        }

        public static void MoveImageSprite(int op, ref Image t, ref Sprite s, Sprite i)
        {
            if (op == 0) t.sprite = i;
            else if (op == 1) s = t.sprite;
            else t.sprite = s;
        }

        public static void MoveCanvasScalarRefPixPerUnit(int op, ref CanvasScaler t, ref float s, float i)
        {
            if (op == 0) t.m_ReferencePixelsPerUnit = i;
            else if (op == 1) s = t.m_ReferencePixelsPerUnit;
            else t.m_ReferencePixelsPerUnit = s;
        }

        public static void MoveTextText(int op, ref Text t, ref string s, string i)
        {
            if (op == 0) t.text = i;
            else if (op == 1) s = t.text;
            else t.text = s;
        }

        public static void MoveParticleSystemColor(int op, ref ParticleSystem t, ref Color s, Color i)
        {
            if (op == 0) t.startColor = i;
            else if (op == 1) s = t.startColor;
            else t.startColor = s;
        }

        public static void MoveImageActive(int op, ref Image t, ref bool s, bool i)
        {
            if (op == 0) t.enabled = i;
            else if (op == 1) s = t.enabled;
            else t.enabled = s;
        }

        public class CRC32
        {
            private readonly uint[] ChecksumTable;
            private readonly uint Polynomial = 0xEDB88320;

            public CRC32()
            {
                ChecksumTable = new uint[0x100];

                for (uint index = 0; index < 0x100; ++index)
                {
                    uint item = index;
                    for (int bit = 0; bit < 8; ++bit)
                        item = ((item & 1) != 0) ? (Polynomial ^ (item >> 1)) : (item >> 1);
                    ChecksumTable[index] = item;
                }
            }

            public byte[] ComputeHash(System.IO.Stream stream)
            {
                uint result = 0xFFFFFFFF;

                int current;
                while ((current = stream.ReadByte()) != -1)
                    result = ChecksumTable[(result & 0xFF) ^ (byte)current] ^ (result >> 8);

                byte[] hash = BitConverter.GetBytes(~result);
                Array.Reverse(hash);
                return hash;
            }

            public byte[] ComputeHash(byte[] data)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data))
                    return ComputeHash(stream);
            }
        }
    }
}
