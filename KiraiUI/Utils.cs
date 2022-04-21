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

        public static void Move(int op, ref Image c, ref Color s, Color i)
        {
            if (op == 0) c.color = i;
            else if (op == 1) s = c.color;
            else c.color = s;
        }

        public static void Move(int op, ref RectTransform t, ref Vector2 s, Vector2 i)
        {
            if (op == 0) t.sizeDelta = i;
            else if (op == 1) s = t.sizeDelta;
            else t.sizeDelta = s;
        }
    }
}
