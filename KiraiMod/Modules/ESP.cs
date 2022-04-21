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
            new ModuleInfo("ESP", "Allows you to see players through walls", ButtonType.Toggle, 3, Menu.PageIndex.options1, nameof(state))
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
            MelonCoroutines.Start(Delay(player));
        }

        public override void OnAvatarInitialized(GameObject avatar, VRCAvatarManager manager)
        {
            if (state)
                MelonCoroutines.Start(DelayRefresh());
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

            Renderer renderer = GetBubbleRenderer(player.gameObject);
            HighlightBubble(renderer, state);
            SetBubbleColor(renderer);
        }

        public IEnumerator DelayRefresh()
        {
            yield return null;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                HighlightBubble(players[i], true);
            }
        }

        public static Renderer GetBubbleRenderer(GameObject player)
        {
            if (player == null) return null;

            Transform bubble = player.transform.Find("SelectRegion");

            if (bubble == null) return null;

            Renderer renderer = bubble.GetComponent<Renderer>();

            return renderer ?? null;
        }

        public static void HighlightBubble(GameObject player, bool state)
        {
            Renderer renderer = GetBubbleRenderer(player);

            if (renderer != null) HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(renderer, state);
        }

        public static void HighlightBubble(Renderer renderer, bool state)
        {
            if (renderer != null) HighlightsFX.prop_HighlightsFX_0.Method_Public_Void_Renderer_Boolean_0(renderer, state);
        }

        public static void SetBubbleColor(GameObject player)
        {
            Renderer renderer = GetBubbleRenderer(player);

            if (renderer != null) renderer.sharedMaterial.color = Utils.Colors.primary;
        }

        public static void SetBubbleColor(Renderer renderer)
        {
            if (renderer != null) renderer.sharedMaterial.color = Utils.Colors.primary;
        }
    }
}