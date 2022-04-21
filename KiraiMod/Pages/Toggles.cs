namespace KiraiMod.Pages
{
    public class Toggles
    {
        public Toggles()
        {
            KiraiLib.UI.Toggle.Create("p0/world-triggers", "World Triggers", "All local triggers become global.", -1f, 0f, false, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles1]].transform, new System.Action<bool>((state) =>
            {
                Shared.Options.bWorldTriggers = state;
            }));

            KiraiLib.UI.Toggle.Create("p0/loud-mic", "Loud Mic", "Makes your microphone louder.", 0f, 0f, false, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles1]].transform, new System.Action<bool>((state) =>
            {
                USpeaker.field_Internal_Static_Single_1 = state ? float.MaxValue : 1f;
            }));

            // MORE

            KiraiLib.UI.Button.Create("p0/open-p1", "More", "Reveal more options", -2f, 2f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.toggles2];
            }));

            KiraiLib.UI.Button.Create("p1/open-p2", "More", "Reveal more options", -2f, 2f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles2]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.toggles3];
            }));

            // BACK

            KiraiLib.UI.Button.Create("p1/close-p1", "Back", "Close KiraiMod's extra options", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles2]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.toggles1];
            }));

            KiraiLib.UI.Button.Create("p2/close-p2", "Back", "Close KiraiMod's extra options", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles3]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.toggles2];
            }));

            // NEXT

            KiraiLib.UI.Button.Create("p0/open-p3", "Next", "Opens KiraiMod's next page", -2f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.buttons1];
            }));

            KiraiLib.UI.Button.Create("p1/open-p3", "Next", "Opens KiraiMod's next page", -2f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles2]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.buttons1];
            }));

            KiraiLib.UI.Button.Create("p2/open-p3", "Next", "Opens KiraiMod's next page", -2f, 1f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles3]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = Shared.PageRemap[(int)Shared.PageIndex.buttons1];
            }));

            // CLOSE

            KiraiLib.UI.Button.Create("p0/close-p0", "Close\nKiraiMod", "Closes KiraiMod menus", -2f, 0f, KiraiLib.UI.pages[Shared.PageRemap[(int)Shared.PageIndex.toggles1]].transform, new System.Action(() =>
            {
                KiraiLib.UI.selected = -1;
            }));
        }
    }
}