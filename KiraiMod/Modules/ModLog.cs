using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Core;

namespace KiraiMod.Modules
{
    public class ModLog : ModuleBase
    {
        UnityEngine.UI.Text text;
        List<string> names = new List<string>();

        public new ModuleInfo[] info = {
            new ModuleInfo("Mod Log", "Moderation log for block, mute, and avatars", ButtonType.Toggle, 0, Menu.PageIndex.options2, nameof(state))
        };

        public ModLog()
        {
            MelonCoroutines.Start(Initialize());
        }

        public IEnumerator Initialize()
        {
            while (Shared.menu == null) yield return null;

            GameObject gameObject = new GameObject("modlog-text");
            text = gameObject.AddComponent<UnityEngine.UI.Text>();

            gameObject.transform.SetParent(GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/NotificationDotParent").transform, false);
            gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 30);
            gameObject.transform.localPosition = new Vector3(480, 40);

            text.color = Utils.Colors.white;
            text.fontStyle = FontStyle.Bold;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignment = TextAnchor.LowerLeft;
            text.fontSize = 36;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.supportRichText = true;
        }

        public void Notify(APIUser source, APIUser target, ModerationAction action)
        {
            MelonLogger.Log($"[Mod Log] {(source.IsLocal() ? "I" : source.displayName)} {System.Enum.GetName(typeof(ModerationAction), action)} {(target.IsLocal() ? "Me" : target.displayName)}");

            Notify(
                $"<color={source.GetTrustColor().ToHex()}>{(source.IsLocal() ? "I" : source.displayName)}</color>" +
                $"<color={ModActToColor(action).ToHex()}> {System.Enum.GetName(typeof(ModerationAction), action)} </color>" + 
                $"<color={target.GetTrustColor().ToHex()}>{(target.IsLocal() ? "Me" : target.displayName)}</color>");
        }

        public void Notify(string name)
        {
            if (!state || text == null) return;

            names.Add(name);
            text.text = string.Join("\n", names);

            MelonCoroutines.Start(DelayedRemove(name));
        }

        public IEnumerator DelayedRemove(string name)
        {
            yield return new WaitForSeconds(3);

            names.Remove(name);
            text.text = string.Join("\n", names);
        }

        public enum ModerationAction
        {
            Blocked = 0,
            Unblocked,
            Muted,
            Unmuted,
            Hide,
            Shown,
            Reset,
        }

        private Color ModActToColor(ModerationAction act)
        {
            switch (act)
            {
                case ModerationAction.Blocked:
                case ModerationAction.Unblocked:
                    return Utils.Colors.red;

                case ModerationAction.Muted:
                case ModerationAction.Unmuted:
                    return Utils.Colors.green;

                case ModerationAction.Hide:
                case ModerationAction.Shown:
                case ModerationAction.Reset:
                    return Utils.Colors.aqua;

                default:
                    return Utils.Colors.black;
            }
        }
    }
}
