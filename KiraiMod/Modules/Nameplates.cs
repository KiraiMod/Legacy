using System.Collections.Generic;
using MelonLoader;
using System.Collections;
using UnityEngine;
using VRC;
using VRC.Core;
using UnityEngine.UI;

namespace KiraiMod.Modules
{
	public class Nameplates : ModuleBase
	{
		public bool RGB = false;

		public new ModuleInfo[] info = {
			new ModuleInfo("Nameplates", "Custom nameplates. Highlight for friends and red for KOS", ButtonType.Toggle, 1, Menu.PageIndex.toggles2, nameof(state)),
			new ModuleInfo("RGB Nameplates", "Rainbow nameplates for friends", ButtonType.Toggle, 2, Menu.PageIndex.toggles2, nameof(RGB)),
		};

		public List<string> kiraimodders = new List<string>();
		public List<string> daymodders = new List<string>();

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
			kiraimodders.Remove(player.field_Private_APIUser_0.displayName);

			if (state) MelonCoroutines.Start(DelayedRefresh());
		}

		public override void OnLevelWasLoaded()
		{
			kiraimodders.Clear();
		}

		public override void OnAvatarInitialized(GameObject avatar, VRCAvatarManager manager)
		{
			if (state) Enable(manager.field_Private_VRCPlayer_0.field_Private_Player_0);
		}

		private IEnumerator DelayedRefresh()
		{
			yield return new WaitForSeconds(1);

			Refresh();
		}

		public void Enable(Player player)
		{
			if (player?.field_Private_APIUser_0 is null) return;

			Transform contents = player.transform.Find("Player Nameplate/Canvas/Nameplate/Contents");
			Transform main = contents.Find("Main");
			Transform icon = contents.Find("Icon");
			Transform stats = contents.Find("Quick Stats");

			main.Find("Background").GetComponent<ImageThreeSlice>().color = player.field_Private_APIUser_0.GetTrustColor();
			main.Find("Pulse").GetComponent<ImageThreeSlice>().color = new Color(1, 0, 1);
			main.Find("Glow").GetComponent<ImageThreeSlice>().color = new Color(1, 0, 1);

			icon.Find("Background").GetComponent<Image>().color = player.field_Private_APIUser_0.GetTrustColor();
			icon.Find("Pulse").GetComponent<Image>().color = new Color(1, 0, 1);
			icon.Find("Glow").GetComponent<Image>().color = new Color(1, 0, 1);

			stats.GetComponent<ImageThreeSlice>().color = Utils.Colors.primary;

			int stack = 0;

			SetTag(ref stack, stats, contents, player.field_Private_APIUser_0.GetTrustColor(), player.field_Private_APIUser_0.GetTrustLevel());
			if (player.IsMod()) SetTag(ref stack, stats, contents, Utils.Colors.red, "Moderator");
			if (player.IsMaster()) SetTag(ref stack, stats, contents, Utils.Colors.highlight, "Master");
			if (player.IsKModder()) SetTag(ref stack, stats, contents, Utils.Colors.highlight, "KiraiMod");
			if (player.IsDModder()) SetTag(ref stack, stats, contents, Color.yellow, "DayClient");
			if (player.IsKOS()) SetTag(ref stack, stats, contents, Utils.Colors.red, "KOS");
			if (player == Shared.TargetPlayer) SetTag(ref stack, stats, contents, Utils.Colors.highlight, "Targeted");

			stats.localPosition = new Vector3(0, (stack + 1) * 30, 0);
		}

		public void Disable(Player player)
		{
			Transform contents = player.transform.Find("Player Nameplate/Canvas/Nameplate/Contents");
			Transform main = contents.Find("Main");
			Transform icon = contents.Find("Icon");
			Transform stats = contents.Find("Quick Stats");

			main.Find("Background").GetComponent<ImageThreeSlice>().color = Color.white;
			main.Find("Pulse").GetComponent<ImageThreeSlice>().color = Color.white; 
			main.Find("Glow").GetComponent<ImageThreeSlice>().color = Color.white;
			
			icon.Find("Background").GetComponent<Image>().color = Color.white;
			icon.Find("Pulse").GetComponent<Image>().color = Color.white; 
			icon.Find("Glow").GetComponent<Image>().color = Color.white;

			stats.GetComponent<ImageThreeSlice>().color = Color.white;

			int i = 0;
			for (;;)
            {
				Transform tag = contents.Find($"KiraiModTag{i}");
				if (tag is null) break;

				tag.gameObject.active = false;
				i++;
            }

			stats.localPosition = new Vector3(0, 30, 0);
		}

		public void Refresh()
		{
			if (PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0 == null) return;

			foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
			{
				if (player?.field_Private_VRCPlayerApi_0 == null || player?.field_Private_APIUser_0 == null) continue;

				if (state) Enable(player);
				else Disable(player);
			}
		}

		private Transform MakeTag(Transform stats, int index)
		{
			Transform rank = Object.Instantiate(stats, stats.parent, false);
			rank.name = $"KiraiModTag{index}";
			rank.localPosition = new Vector3(0, 30 * (index + 1), 0);
			rank.gameObject.active = true;
			Transform textGO = null;

			for (int i = rank.childCount; i > 0; i--)
			{
				Transform child = rank.GetChild(i - 1);

				if (child.name == "Trust Text")
				{
					textGO = child;
					continue;
				}

				Object.Destroy(child.gameObject);
			}

			return textGO;
		}

		private void SetTag(ref int stack, Transform stats, Transform contents, Color color, string content)
        {
			Transform tag = contents.Find($"KiraiModTag{stack}");

			Transform label = null;

			if (tag == null)
				label = MakeTag(stats, stack);
			else 
			{ 
				tag.gameObject.SetActive(true);
				label = tag.Find("Trust Text");
			}

			var text = label.GetComponent<TMPro.TextMeshProUGUI>();
			text.color = color;
			text.text = content;

			stack++;
		}

		public void OnStateChangeRGB(bool state)
		{
			if (!state) Refresh();
		}
	}
}
