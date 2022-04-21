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

            MelonLogger.Log(GetType().Name + (state ? " On" : " Off"));

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
        public int index = -1;
        public int page = -1;
        public string reference = null;

        public ModuleInfo (string label, string description, ButtonType type, int index, int page, string reference) {
            this.label = label;
            this.description = description;
            this.type = type;
            this.index = index;
            this.page = page;
            this.reference = reference;
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