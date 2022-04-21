using MelonLoader;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class GodMode : ModuleBase
    {
        public new ModuleInfo[] info = {
            new ModuleInfo("God Mode", "Prevent dying in Murder 2/3", ButtonType.Toggle, 6, Shared.PageIndex.toggles2, nameof(state))
        };

        private VRC_Trigger cached;

        public override void OnStateChange(bool state)
        {
            if (cached == null)
            {
                foreach (VRC_Trigger trigger in UnityEngine.Object.FindObjectsOfType<VRC_Trigger>())
                {
                    if (trigger.name == "Death")
                    {
                        cached = trigger;
                    }
                }
            }

            if (cached == null) 
            { 
                MelonLogger.LogWarning("Failed to find Murder Logic 3 Death trigger.");
                return;
            }

            cached.gameObject.SetActive(!state);
        }

        public override void OnLevelWasLoaded()
        {
            cached = null;
            if (state) OnStateChange(true);
        }
    }
}
