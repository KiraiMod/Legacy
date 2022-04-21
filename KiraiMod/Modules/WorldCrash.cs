using System;
using UnityEngine;
using VRC;

namespace KiraiMod.Modules
{
    public class WorldCrash : ModuleBase
    {
        public new ModuleInfo[] info =
        {
            new ModuleInfo("Select\nMulti Crash", "Select which people in the lobby to crash", ButtonType.Button, 7, Shared.PageIndex.buttons2, nameof(Show)),
            new ModuleInfo("Activate\nMulti Crash", "Crash all the people you have selected", ButtonType.Half, 11, false, Shared.PageIndex.buttons2, nameof(Activate)),
            new ModuleInfo("Deactivate\nMulti Crash", "Disable Multi Crash after activation", ButtonType.Half, 11, true, Shared.PageIndex.buttons2, nameof(Deactivate))
        };

        public System.Collections.Generic.List<string> selected = new System.Collections.Generic.List<string>();
        private System.Collections.Generic.List<string> processed = new System.Collections.Generic.List<string>();

        public void Activate()
        {
            if (state) return;
            
            if (PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 == null) return;

            state = true;

            KiraiLib.Logger.Log($"Starting Multi Crash with {selected.Count} users");

            MelonLoader.MelonCoroutines.Start(ActivateEx());
        }

        private System.Collections.IEnumerator ActivateEx()
        {
            int count = 0;
            foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (!selected.Contains(player.field_Private_APIUser_0.displayName))
                {
                    processed.Add(player.field_Private_APIUser_0.displayName);

                    QuickMenu.prop_QuickMenu_0.Method_Public_Void_Player_0(player);
                    QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu/BlockButton").GetComponent<UnityEngine.UI.Button>().Press();

                    yield return new WaitForSecondsRealtime(0.15f);
                }
                else count++;
            }

            KiraiLib.Logger.Log($"Hitting {count} users.");

            yield return new WaitForSecondsRealtime(4.0f);

            KiraiLib.Logger.Log("Switched into crasher");

            (KiraiLib.UI.elements[Utils.CreateID("World\nCrash", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle).SetState(true);
        }

        public void Deactivate()
        {
            if (!state) return;

            state = false;

            MelonLoader.MelonCoroutines.Start(DeactivateEx());
        }
        
        private System.Collections.IEnumerator DeactivateEx()
        {
            (KiraiLib.UI.elements[Utils.CreateID("World\nCrash", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle).SetState(false);

            yield return new WaitForSecondsRealtime(5.0f);

            foreach (string name in processed)
            {
                Player player = Utils.GetPlayer(name);

                if (player is null) KiraiLib.Logger.Log($"<color=red>{name} left before restoration</color>");
                else
                {
                    QuickMenu.prop_QuickMenu_0.Method_Public_Void_Player_0(player);
                    QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu/BlockButton").GetComponent<UnityEngine.UI.Button>().Press();

                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }
            processed.Clear();
        }

        public void Show() => Shared.modules.playerlist.RefreshEx(false);
    }
}
