namespace KiraiMod.Pages
{
    public class Toggles
    {
        public Toggles()
        {
            Shared.menu.CreateButton("p0/open-p1", "More", "Reveal more options", -2f, 2f, Shared.menu.pages[0].transform, new System.Action(() =>
            {
                Shared.menu.selected = 1;
            }));

            Shared.menu.CreateButton("p0/open-p2", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[0].transform, new System.Action(() =>
            {
                Shared.menu.selected = 2;
            }));

            Shared.menu.CreateButton("p0/close-p0", "Close\nKiraiMod", "Closes KiraiMod menus", -2f, 0f, Shared.menu.pages[0].transform, new System.Action(() =>
            {
                Shared.menu.selected = -1;
            }));

            Shared.menu.CreateToggle("p0/world-triggers", false, "World Triggers", "All local triggers become global.", -1f, 0f, Shared.menu.pages[0].transform, new System.Action<bool>((state) =>
            {
                Shared.Options.bWorldTriggers = state;
            }));

            Shared.menu.CreateToggle("p0/loud-mic", false, "Loud Mic", "Makes your microphone louder.", 0f, 0f, Shared.menu.pages[0].transform, new System.Action<bool>((state) =>
            {
                USpeaker.field_Internal_Static_Single_1 = state ? float.MaxValue : 1f;
            }));

            Shared.menu.CreateButton("p1/open-p2", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 2;
            }));

            Shared.menu.CreateButton("p1/close-p1", "Back", "Close KiraiMod's extra options", -2f, 0f, Shared.menu.pages[1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 0;
            }));
        }
    }
}