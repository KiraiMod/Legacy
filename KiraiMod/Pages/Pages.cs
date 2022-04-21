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
                Shared.menu.selected = (int)Menu.PageIndex.udon;
            }));

            Shared.menu.CreateButton("udon/goto-sliders1", "Previous", "Opens KiraiMod's previous page", -2f, 0f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.menu.selected = (int)Menu.PageIndex.sliders1;
            }));

            Shared.menu.CreateButton("udon/refresh", "Refresh", "Fetch all UdonBehaviours again", 3f, 2f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.modules.udon.OnLevelWasLoaded();
            }));

            Shared.menu.CreateButton("udon/more", "Up", "See less UdonBheaviours", 3f, 1f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.modules.udon.CurrentPage--;
            }));

            Shared.menu.CreateButton("udon/less", "Down", "See more UdonBehaviours", 3f, 0f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                Shared.modules.udon.CurrentPage++;
            }));

            Shared.menu.CreateButton("udon/broadcast", "Broadcast", "Broadcast an event to every UdonBehaviour", 3f, -1f, Shared.menu.pages[(int)Menu.PageIndex.udon].transform, new System.Action(() =>
            {
                if (Config.General.bUseClipboard)
                    Helper.BroadcastCustomEvent(System.Windows.Forms.Clipboard.GetText().Trim());
                else
                    Utils.HUDInput("Custom event name", "Execute", "_interact", "", new System.Action<string>((resp) =>
                    {
                        Helper.BroadcastCustomEvent(resp.Trim());
                    }));
            }));
        }
    }
}