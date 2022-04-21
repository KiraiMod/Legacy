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
			new ModuleInfo("Nameplates", "Custom nameplates. Highlight for friends and red for KOS", ButtonType.Toggle, 1, Menu.PageIndex.options2, nameof(state)),
			new ModuleInfo("RGB Nameplates", "Rainbow nameplates for friends", ButtonType.Toggle, 2, Menu.PageIndex.options2, nameof(RGB)),
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
			
			int stack = 1;

			Transform rank = contents.Find("KiraiModTag0");
			if (rank is null)
            {
				rank = MakeTag(stats, 0);
				var text = rank.GetComponent<TMPro.TextMeshProUGUI>();
				text.text = player.field_Private_APIUser_0.GetTrustLevel();
				text.color = player.field_Private_APIUser_0.GetTrustColor();
			}
			else rank.gameObject.SetActive(true);

			if (player.IsMaster())
			{
				Transform master = contents.Find($"KiraiModTag{stack}");
				if (master == null)
                {
					master = MakeTag(stats, stack);
					var text = master.GetComponent<TMPro.TextMeshProUGUI>();
					text.text = "Master";
					text.color = Utils.Colors.highlight;
				}

				else master.gameObject.SetActive(true);

				stack++;
			}

			if (player.IsKOS())
            {
				Transform kos = contents.Find($"KiraiModTag{stack}");
				if (kos == null)
				{
					kos = MakeTag(stats, stack);
					var text = kos.GetComponent<TMPro.TextMeshProUGUI>();
					text.text = "KOS";
					text.color = Utils.Colors.red;
				}

				else kos.gameObject.SetActive(true);

				stack++;
			}

			if (player.IsKModder())
			{
				Transform kmodder = contents.Find($"KiraiModTag{stack}");
				if (kmodder == null)
				{
					kmodder = MakeTag(stats, stack);
					var text = kmodder.GetComponent<TMPro.TextMeshProUGUI>();
					text.text = "KModder";
					text.color = Utils.Colors.highlight;
				}

				else kmodder.gameObject.SetActive(true);

				stack++;
			}

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

		public void OnStateChangeRGB(bool state)
		{
			if (!state) Refresh();
		}
	}
}
