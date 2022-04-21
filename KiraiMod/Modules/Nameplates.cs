using System.Collections.Generic;
using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;
using VRC.Core;

namespace KiraiMod.Modules
{
	public class Nameplates : ModuleBase
	{
		public bool RGB = false;

		public new ModuleInfo[] info = {
			new ModuleInfo("Nameplates", "Custom nameplates. Highlight for friends and red for KOS", ButtonType.Toggle, 1, 1, nameof(state)),
			new ModuleInfo("RGB Nameplates", "Rainbow nameplates for friends", ButtonType.Toggle, 2, 1, nameof(RGB)),
		};

		public Dictionary<string, string> users = new Dictionary<string, string>();

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
			users.Remove(player.field_Private_APIUser_0.displayName);

			if (state) MelonCoroutines.Start(DelayedRefresh());
		}

		public override void OnLevelWasLoaded()
		{
			users.Clear();
		}

		public override void OnAvatarInitialized(GameObject avatar, VRCAvatarManager manager)
		{
			if (state) Enable(manager.field_Private_VRCPlayer_0.field_Private_Player_0);
		}

		public override void OnUpdate()
		{
			if (state && RGB)
			{
				if (PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 == null) return;

				foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
				{
					if (player?.field_Private_APIUser_0?.IsLocal() ?? true || player?.field_Private_VRCPlayerApi_0 == null) continue;

					if (player.IsFriend() && !player.IsKOS())
						player.field_Private_VRCPlayerApi_0.SetNamePlateColor(Utils.GetRainbow());
				}
			}
		}

		private IEnumerator DelayedRefresh()
        {
			yield return new WaitForSeconds(1);

			Refresh();
        }

		public void Enable(Player player)
		{
			if (player?.field_Private_VRCPlayerApi_0 == null) return;

			player.field_Private_VRCPlayerApi_0.SetNamePlateColor(player.GetNameplateColor());

			Transform nameplate = player.transform.Find("Canvas - Profile (1)/Text/Text - NameTag");

			UnityEngine.UI.Text text = nameplate.GetComponent<UnityEngine.UI.Text>();
			text.supportRichText = true;
			text.text = $"<color={player.GetTextColor().ToHex()}>{player.field_Private_APIUser_0.displayName}</color>";

			Transform rank = nameplate.Find("KiraiModRank");
			if (rank == null) MelonCoroutines.Start(CreateText(nameplate, player));
            else
            {
                rank.gameObject.SetActive(true);
				UpdateText(rank, player);
            }
        }

		public void Disable(Player player)
		{
			if (player == null || player.field_Private_VRCPlayerApi_0 == null) return;

			player.field_Private_VRCPlayerApi_0.RestoreNamePlateColor();
			Transform nameplate = player.transform.Find("Canvas - Profile (1)/Text/Text - NameTag");
			nameplate.GetComponent<UnityEngine.UI.Text>().text = player.field_Private_APIUser_0.displayName;
			Transform rank = nameplate.Find("KiraiModRank");
			if (rank != null) rank.gameObject.SetActive(false);
		}

		public void Refresh()
		{
			if (PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 == null) return;

			foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
			{
				if (player?.field_Private_VRCPlayerApi_0 == null || player?.field_Private_APIUser_0 == null || player.IsLocal()) continue;

				if (state) Enable(player);
				else Disable(player);
			}
		}

		public IEnumerator CreateText(Transform nameplate, Player player)
		{
			if (nameplate == null || player == null) yield break;

			while (Shared.menu == null) yield return null;

			GameObject gameObject = new GameObject("KiraiModRank");
			UnityEngine.UI.Text text = gameObject.AddComponent<UnityEngine.UI.Text>();

			gameObject.transform.SetParent(nameplate, false);
			gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
			gameObject.transform.localPosition = new Vector3(0, 360);

			text.color = Color.white;
			text.fontStyle = FontStyle.Bold | FontStyle.Italic;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.verticalOverflow = VerticalWrapMode.Overflow;
			text.alignment = TextAnchor.LowerCenter;
			text.fontSize = 72;
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.supportRichText = true;

			UpdateText(gameObject.transform, player);
		}

		public void UpdateText(Transform nameplate, Player player)
        {
			if (nameplate == null || player == null) return;

			nameplate.GetComponent<UnityEngine.UI.Text>().text =
				(player.IsMaster() ? $"<color={Utils.Colors.highlight.ToHex()}>Master</color>\n" : "") + 
				$"<color={player.field_Private_APIUser_0.GetTrustColor().ToHex()}>{player.field_Private_APIUser_0.GetTrustLevel()}</color>";
		}

		public void OnStateChangeRGB(bool state)
        {
			if (!state) Refresh();
        }
	}
}
