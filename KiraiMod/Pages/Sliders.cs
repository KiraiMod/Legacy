namespace KiraiMod.Pages
{
    public class Sliders
    {
        public Sliders()
        {
            Shared.menu.CreateButton("p4/close-p4", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[4].transform, new System.Action(() =>
            {
                Shared.menu.selected = 2;
            }));

            Shared.menu.CreateSlider("p4/walk-speed", "Walk Speed", -1f, 1.25f, 0f, 32f, Shared.modules.speed.speedWalk, Shared.menu.pages[4].transform, new System.Action<float>(value =>
            {
                Shared.modules.speed.SetWalkSpeed(value);
            }));

            Shared.menu.CreateSlider("p4/run-speed", "Run Speed", -1f, 0.75f, 0f, 32f, Shared.modules.speed.speedRun, Shared.menu.pages[4].transform, new System.Action<float>(value =>
            {
                Shared.modules.speed.SetRunSpeed(value);
            }));

            Shared.menu.CreateSlider("p4/flight-speed", "Flight Speed", -1f, 0.25f, 0, 32, Shared.modules.flight.speed, Shared.menu.pages[4].transform, new System.Action<float>(value =>
            {
                Shared.modules.flight.speed = value;
            }));

            Shared.menu.CreateSlider("p4/portal-distance", "Portal Distance", -1f, -0.25f, 1, 8f, Shared.modules.portal.distance, Shared.menu.pages[4].transform, new System.Action<float>(value =>
            {
                Shared.modules.portal.distance = value;
            }));

            Shared.menu.CreateSlider("p4/orbit-speed", "Orbit Speed", -1f, -0.75f, 0, 8f, Shared.modules.orbit.speed, Shared.menu.pages[4].transform, new System.Action<float>(value =>
            {
                Shared.modules.orbit.speed = value;
            }));

            Shared.menu.CreateSlider("p4/orbit-distance", "Orbit Distance", -1f, -1.25f, 0, 4f, Shared.modules.orbit.distance, Shared.menu.pages[4].transform, new System.Action<float>(value =>
            {
                Shared.modules.orbit.distance = value;
            }));

            Shared.menu.CreateSlider("p4/rgb-speed", "RGB Speed", 1.5f, 1.25f, 0f, 4f, Utils.fRGBSpeed, Shared.menu.pages[4].transform, new System.Action<float>(value =>
            {
                Utils.fRGBSpeed = value;
            }));
        }
    }
}