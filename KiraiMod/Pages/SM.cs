namespace KiraiMod.Pages
{
    public class SM
    {
        public SM()
        {
            KiraiLib.UI.Button.Create("um/open-p0", "Open\nKiraiMod", "Opens KiraiMod menus", -2f, 1f, KiraiLib.UI.ShortcutMenu.transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.toggles1];
            }));

            {
                KiraiLib.UI.Button.Create("p0/xutils-activate", "XUtils", "Starts raycast utilities", -2f, 0f, KiraiLib.UI.ShortcutMenu.transform, new System.Action(() =>
                {
                    Shared.modules.xutils.SetState(true);
                }));
            }
        }
    }
}