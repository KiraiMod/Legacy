using UnhollowerRuntimeLib;

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

            Shared.menu.CreateToggle("toggles1/clipboard", Config.General.bUseClipboard, "Clipboard", "Use the clipboard instead of a popup input", 1f, -1f, Shared.menu.pages[(int)Menu.PageIndex.options2].transform, new System.Action<bool>((state) =>
            {
                Config.General.bUseClipboard = state;
            }));

            Shared.menu.CreateButton("buttons2/change-pedestals", "Change\nPedestals", "Change all pedestals to an avatar ID", 2f, 1f, Shared.menu.pages[(int)Menu.PageIndex.buttons2].transform, new System.Action(() =>
            {
                if (Config.General.bUseClipboard)
                    Helper.SetPedestals(System.Windows.Forms.Clipboard.GetText().Trim());
                else
                    Utils.HUDInput("Avatar ID", "Set Pedestals", "avtr_????????-????-????-????-????????????", "", new System.Action<string>((resp) =>
                    {
                        Helper.SetPedestals(resp.Trim());
                    }));
            }));

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

            Shared.menu.CreateToggle("toggles3/persistant-quickmenu", Config.General.bPersistantQuickMenu, "Persistant QuickMenu", "Keep the Quick Menu open even when moving around", 0f, 1f, Shared.menu.pages[(int)Menu.PageIndex.options3].transform, new System.Action<bool>((state) => {
                Config.General.bPersistantQuickMenu = state;
            }));
        }
    }
}