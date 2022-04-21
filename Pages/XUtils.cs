using UnityEngine;

namespace KiraiMod.Pages
{
    public class XUtils
    {
        public XUtils()
        {
            Shared.menu.CreateButton("p3/disable", "Disable", "Disables selected collider", -1f, 1f, Shared.menu.pages[4].transform, new System.Action(() =>
            {
                Shared.modules.xutils.hit.enabled = false;
            }));

            Shared.menu.CreateButton("p3/enable", "Enable", "Enables all child colliders", 0f, 1f, Shared.menu.pages[4].transform, new System.Action(() =>
            {
                Collider[] colliders = Shared.modules.xutils.hit.gameObject.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = true;
                }
            }));

            Shared.menu.CreateButton("p3/destroy", "Destroy", "Destroys selected object", 1f, 1f, Shared.menu.pages[4].transform, new System.Action(() =>
            {
                Object.Destroy(Shared.modules.xutils.hit.gameObject);
            }));

            Shared.menu.CreateButton("p3/log", "Log", "Logs GameObject to logs", 2f, 1f, Shared.menu.pages[4].transform, new System.Action(() =>
            {
                Utils.LogGO(Shared.modules.xutils.hit.gameObject);
            }));
        }
    }
}