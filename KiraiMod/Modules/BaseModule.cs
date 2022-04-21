using MelonLoader;
using UnityEngine;
using VRC;

namespace KiraiMod.Modules
{
    public abstract class ModuleBase
    {
        public bool state = false;
        public ModuleInfo[] info = new ModuleInfo[0];

        public void SetState(bool? n_state = null)
        {
            if ((n_state ?? !state) == state) return;

            state = n_state ?? !state;

            MelonLogger.Msg(GetType().Name + (state ? " On" : " Off"));

            OnStateChange(state);
        }

        public virtual void OnStateChange(bool state) { }
        public virtual void OnConfigLoaded() { }

        public virtual void OnUpdate() { }
        public virtual void OnLevelWasLoaded() { }
        public virtual void OnPlayerJoined(Player player) { }
        public virtual void OnPlayerLeft(Player player) { }
        public virtual void OnAvatarInitialized(GameObject avatar, VRCAvatarManager manager) { }
    }

    public class ModuleInfo
    {
        public string label = "No Label";
        public string description = "No Description";
        public ButtonType type = ButtonType.Undefined;
        public float x = -1;
        public float y = -1;
        public Shared.PageIndex page;
        public string reference = null;
        public float min;
        public float max;

        public ModuleInfo (string label, string description, ButtonType type, int index, Shared.PageIndex page, string reference) {
            this.label = label;
            this.description = description;
            this.type = type;
            Utils.GetGenericLayout(index, out int x, out int y);
            this.x = x;
            this.y = y;
            this.page = page;
            this.reference = reference;
        }

        public ModuleInfo(string label, string description, ButtonType type, int x, int y, Shared.PageIndex page, string reference)
        {
            this.label = label;
            this.description = description;
            this.type = type;
            this.x = x;
            this.y = y;
            this.page = page;
            this.reference = reference;
        }

        public ModuleInfo(string label, ButtonType type, int index, Shared.PageIndex page, string reference, float min, float max)
        {
            this.label = label;
            this.type = type;
            Utils.GetSliderLayout(index, out x, out y);
            this.page = page;
            this.reference = reference;
            this.min = min;
            this.max = max;
        }
    }

    public enum ButtonType 
    {
        Toggle,
        Button,
        Slider,
        Undefined
    }
}