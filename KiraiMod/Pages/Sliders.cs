namespace KiraiMod.Pages
{
    public class Sliders
    {
        public Sliders()
        {
            Shared.menu.CreateButton("p5/close-p5", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[(int)Menu.PageIndex.sliders1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 3;
            }));

            Shared.menu.CreateSlider("p5/rgb-speed", "RGB Speed", 1.25f, -0.25f, 0f, 4f, Config.General.fRGBSpeed, Shared.menu.pages[(int)Menu.PageIndex.sliders1].transform, new System.Action<float>(value =>
            {
                Config.General.fRGBSpeed = value;
            }));
        }
    }
}