using MelonLoader;
using VRC;

namespace KiraiMod.Modules
{
    public abstract class ModuleBase
    {
        public bool state = false;

        public void SetState(bool? n_state = null)
        {
            if ((n_state ?? !state) == state) return;

            state = n_state ?? !state;

            MelonModLogger.Log(GetType().Name + (state ? " On" : " Off"));

            OnStateChange(state);
        }

        public virtual void OnStateChange(bool state) { }

        public virtual void OnPlayerJoined(Player player) { }
        public virtual void OnPlayerLeft(Player player) { }
        public virtual void OnUpdate() { }
    }
}