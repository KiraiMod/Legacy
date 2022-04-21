using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class FreezePickups
    {
        [ModuleLoader.UIToggle("Freeze\nPickups", "Prevent anyone from moving pickups", Shared.PageIndex.toggles2, 3, nameof(OnStateChange))]
        public static bool active;

        private static VRC_Pickup[] pickups;

        public static void OnStateChange(bool state)
        {
            if (state)
            {
                pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
                Shared.Events.OnUpdate += Freeze;
            }
            else Shared.Events.OnUpdate -= Freeze;
        }

        private static void Freeze()
        {
            foreach (VRC_Pickup pickup in pickups)
                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
        }
    }
}
