namespace KiraiMod.Pages
{
    public class SM
    {
        public SM()
        {
            Shared.menu.CreateButton("um/open-p0", "Open\nKiraiMod", "Opens KiraiMod menus", -2f, 1f, Shared.menu.sm.transform, new System.Action(() =>
            {
                Shared.menu.selected = 0;
            }));

            Shared.menu.CreateButton("p0/xutils-activate", "XUtils", "Starts raycast utilities", -2f, 0f, Shared.menu.sm.transform, new System.Action(() =>
            {
                Shared.modules.xutils.SetState(true);
            }));
        }
    }
}