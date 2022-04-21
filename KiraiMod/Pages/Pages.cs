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
                Shared.menu.selected = (int)Menu.PageIndex.udon1;
            }));

            Shared.menu.CreateButton("udon/goto-sliders1", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[(int)Menu.PageIndex.udon1].transform, new System.Action(() =>
            {
                Shared.menu.selected = (int)Menu.PageIndex.sliders1;
            }));

            Shared.menu.CreateButton("udon1/goto-udon2", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[(int)Menu.PageIndex.udon1].transform, new System.Action(() =>
            {
                Shared.menu.selected = (int)Menu.PageIndex.udon2;
            }));

            Shared.menu.CreateButton("udon2/goto-udon1", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[(int)Menu.PageIndex.udon2].transform, new System.Action(() =>
            {
                Shared.menu.selected = (int)Menu.PageIndex.udon1;
            }));

            Shared.menu.CreateToggle("toggles3/persistant-quickmenu", Config.General.bPersistantQuickMenu, "Persistant QuickMenu", "Keep the Quick Menu open even when moving around", 0f, 1f, Shared.menu.pages[(int)Menu.PageIndex.toggles3].transform, new System.Action<bool>((state) => {
                Config.General.bPersistantQuickMenu = state;
            }));
        }
    }
}