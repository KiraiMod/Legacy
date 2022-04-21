namespace KiraiMod.Pages
{
    public class Pages
    {
        public SM qm;
        public UIM uim;
        public Toggles toggles;
        public Buttons buttons;
        public Sliders sliders;

        public Pages()
        {
            qm = new SM();
            uim = new UIM();
            toggles = new Toggles();
            buttons = new Buttons();
            sliders = new Sliders();

            KiraiLib.UI.Button.Create("sliders1/goto-udon", "Next", "Opens KiraiMod's next page", -2f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.sliders1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.udon1];
            }));

            KiraiLib.UI.Button.Create("udon/goto-sliders1", "Previous", "Opens KiraiMod's previous page", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.udon1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.sliders1];
            }));

            KiraiLib.UI.Button.Create("udon1/goto-udon2", "Next", "Opens KiraiMod's next page", -2f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.udon1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.udon2];
            }));

            KiraiLib.UI.Button.Create("udon2/goto-udon1", "Previous", "Opens KiraiMod's previous page", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.udon2]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.udon1];
            }));
        }
    }
}