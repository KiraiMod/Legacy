namespace KiraiMod.Pages
{
    public class Sliders
    {
        public Sliders()
        {
            KiraiLib.UI.Button.Create("p5/close-p5", "Previous", "Opens KiraiMod's previous page", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.sliders1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.buttons1];
            }));

            KiraiLib.UI.Slider.Create("p5/rgb-speed", "RGB Speed", 1.25f, -0.25f, 0f, 4f, Config.General.fRGBSpeed, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.sliders1]].transform, new System.Action<float>(value =>
            {
                Config.General.fRGBSpeed = value;
            }));
        }
    }
}