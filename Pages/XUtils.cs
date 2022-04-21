using UnityEngine;

namespace KiraiMod.Pages
{
    public class XUtils
    {
        public XUtils()
        {
            Shared.menu.CreateButton("p5/disable", "Disable", "Disables selected collider", -1f, 1f, Shared.menu.pages[5].transform, new System.Action(() =>
            {
                Shared.modules.xutils.hit.collider.enabled = false;
            }));

            Shared.menu.CreateButton("p5/enable", "Enable", "Enables all child colliders", 0f, 1f, Shared.menu.pages[5].transform, new System.Action(() =>
            {
                Collider[] colliders = Shared.modules.xutils.hit.collider.gameObject.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = true;
                }
            }));

            Shared.menu.CreateButton("p5/destroy", "Destroy", "Destroys selected object", 1f, 1f, Shared.menu.pages[5].transform, new System.Action(() =>
            {
                Object.Destroy(Shared.modules.xutils.hit.collider.gameObject);
                Shared.modules.xutils.state2 = false;
                Shared.menu.selected = 0;
            }));

            Shared.menu.CreateButton("p5/log", "Log", "Logs GameObject to logs", 2f, 1f, Shared.menu.pages[5].transform, new System.Action(() =>
            {
                Utils.LogGO(Shared.modules.xutils.hit.collider.gameObject);
            }));

            Shared.menu.CreateButton("p5/portal", "Portal", "Place portal at hit location", -1f, 0f, Shared.menu.pages[5].transform, new System.Action(() =>
            {
                Helper.PortalPosition(Shared.modules.xutils.hit.point, Quaternion.Euler(0, 0, 0), Shared.modules.portal.infinite);
            }));
        }
    }
}