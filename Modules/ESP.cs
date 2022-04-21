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
            new ModuleInfo("ESP", "Allows you to see players through walls", ButtonType.Toggle, 3, 0, nameof(state))
        };

        public override void OnStateChange(bool state)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                HighlightBubble(players[i], state);
            }
        }

        public override void OnPlayerJoined(Player player)
        {
            MelonCoroutines.Start(DelayHighlight(player));
        }

        public IEnumerator DelayHighlight(Player player)
        {
            if (player == null) yield break;

            int timeout = 0;

            while (player.gameObject == null && timeout < 30)
            {
                yield return new WaitForSeconds(1.0f);
                timeout++;
            }

            HighlightBubble(player.gameObject, state);
        }


        public static void HighlightBubble(GameObject player, bool state)
        {
            if (player == null) return;

            Transform bubble = player.transform.Find("SelectRegion");

            if (bubble == null) return;

            Renderer renderer = bubble.GetComponent<Renderer>();

            if (renderer == null) return;

            HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(renderer, state);
        }
    }
}