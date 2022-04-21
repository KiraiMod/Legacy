using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;
using VRC.Core;

namespace KiraiMod.Modules
{
    public class Nameplates : ModuleBase
    {
        public bool rgb = false;

		public override void OnStateChange(bool state)
		{
            Refresh();
		}

        public override void OnConfigLoaded()
        {
            Refresh();
        }

        public override void OnPlayerJoined(Player player)
        {
			if (state) Enable(player);
        }

        public override void OnPlayerLeft(Player player)
        {
			if (state && player.field_Private_VRCPlayerApi_0.isMaster) Refresh();
        }

        public override void OnUpdate()
        {
            if (state && rgb)
            {
                if (!PlayerManager.field_Private_Static_PlayerManager_0) return;

                List<Player> players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;

                if (players == null) return;

                foreach (Player player in players)
                {
                    if (player == null || player.field_Private_APIUser_0 == null || player.field_Private_APIUser_0.id == APIUser.CurrentUser.id) continue;

                    if (player.IsFriend() && !player.IsKOS())
                    {
                        player.field_Private_VRCPlayerApi_0.SetNamePlateColor(Utils.GetRainbow());
                    }
                }
            }
        }

        public void Enable(Player player)
        {
			if (player == null || player.field_Internal_VRCPlayer_0 == null) return;

			player.field_Private_VRCPlayerApi_0.SetNamePlateColor(player.GetNameplateColor());


            Transform nameplate = player.transform.Find("Canvas - Profile (1)/Text/Text - NameTag");

            UnityEngine.UI.Text text = nameplate.GetComponent<UnityEngine.UI.Text>();
            text.supportRichText = true;
            text.text = $"<color={player.GetTextColor().ToHex()}>{player.field_Private_APIUser_0.displayName}</color>";

            Transform rank = nameplate.Find("KiraiModRank");
            if (rank == null)
            {
                MelonCoroutines.Start(CreateRankText(nameplate, player));
            }
            else rank.gameObject.SetActive(true);
        }

        public void Disable(Player player)
        {
			player.field_Private_VRCPlayerApi_0.RestoreNamePlateColor();
			Transform nameplate = player.transform.Find("Canvas - Profile (1)/Text/Text - NameTag");
			nameplate.GetComponent<UnityEngine.UI.Text>().text = player.field_Private_APIUser_0.displayName;
			Transform rank = nameplate.Find("KiraiModRank");
			if (rank != null) rank.gameObject.SetActive(false);
		}

		public void Refresh()
        {
			if (!PlayerManager.field_Private_Static_PlayerManager_0) return;

			List<Player> players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;

			if (players == null) return;

			foreach (Player player in players)
			{
				if (player == null || player.field_Private_APIUser_0 == null || player.IsLocal()) continue;

				if (state) Enable(player);
                else Disable(player);
            }
        }

        public IEnumerator CreateRankText(Transform nameplate, Player player)
        {
            while (Shared.menu == null) yield return null;

            GameObject gameObject = new GameObject("KiraiModRank");
            UnityEngine.UI.Text text = gameObject.AddComponent<UnityEngine.UI.Text>();

            gameObject.transform.SetParent(nameplate, false);
            gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 30);
            gameObject.transform.localPosition = new Vector3(0, 100);

            text.color = player.field_Private_APIUser_0.GetTrustColor();
            text.fontStyle = FontStyle.Bold | FontStyle.Italic;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 72;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.supportRichText = true;

            text.text = player.field_Private_APIUser_0.GetTrustLevel();
        }
    }
}
