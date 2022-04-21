using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;

namespace KiraiMod.Modules
{
    public class ESP : ModuleBase
    {
        public new ModuleInfo[] info =
        {
            new ModuleInfo("ESP", "Allows you to see players through walls", ButtonType.Toggle, 3, Shared.PageIndex.toggles1, nameof(state))
        };

        public override void OnStateChange(bool state)
        {
            foreach (Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
            {
                HighlightPlayer(player, state);
            }
        }

        public override void OnConfigLoaded()
        {
            foreach (Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
            {
                HighlightPlayer(player, state);
            }
        }

        public override void OnPlayerJoined(Player player)
        {
            MelonCoroutines.Start(Delay(player));
        }

        public IEnumerator Delay(Player player)
        {
            if (player == null) yield break;

            int timeout = 0;

            while (player.gameObject == null && timeout < 30)
            {
                yield return new WaitForSeconds(1.0f);
                timeout++;
            }

            HighlightPlayer(player, state);
        }

        public IEnumerator DelayRefresh()
        {
            yield return null;

            foreach (Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
                HighlightPlayer(player, state);
        }

        public static void HighlightPlayer(Player player, bool state)
        {
            Renderer renderer = player?.transform.Find("SelectRegion")?.GetComponent<Renderer>();
            if (renderer)
            {
                HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(renderer, state);
                renderer.sharedMaterial.color = Utils.Colors.primary;
            }
        }
    }
}