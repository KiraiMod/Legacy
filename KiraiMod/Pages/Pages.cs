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

            Shared.menu.CreateButton("sliders1/goto-udon", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[(int)Menu.PageIndex.sliders1].transform, new System.Action(() =>
            {
                Shared.menu.selected = (int)Menu.PageIndex.udon;
            }));

            Shared.menu.CreateButton("udon/goto-sliders1", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.menu.selected = (int)Menu.PageIndex.sliders1;
            }));

            Shared.menu.CreateButton("udon/more", "Up", "See less UdonBheaviours", 3f, 1f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.modules.udon.CurrentPage--;
            }));

            Shared.menu.CreateButton("udon/less", "Down", "See more UdonBehaviours", 3f, 0f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.modules.udon.CurrentPage++;
            }));

            Shared.menu.CreateButton("udon/refresh", "Refresh", "Fetch all UdonBehaviours again", 3f, -1f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.modules.udon.OnLevelWasLoaded();
            }));
        }
    }
}