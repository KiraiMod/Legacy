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

            //for (int i = 0; i < 12; i++) // page 1
            //{
            //    int x = i % 4 - 1;
            //    int y = 1 - i / 4;

            //    switch (i) {
            //        case 5:
            //            break;
            //        case 8:
            //            break;
            //        default:
            //            ModuleBase current = Shared.modules.modules[i];
            //            break;
            //    }
            //}

            Shared.menu.CreateToggle( "Speed",     "Toggle movement speed", -1, 1, 0, Shared.modules.speed );
            Shared.menu.CreateToggle("Flight",             "Toggle flight",  0, 1, 0, Shared.modules.flight);
            Shared.menu.CreateToggle("Noclip",             "Toggle noclip",  1, 1, 0, Shared.modules.noclip);
            Shared.menu.CreateToggle(   "ESP", "See players through walls",  2, 1, 0, Shared.modules.esp   );

            Shared.menu.CreateToggle("p0/loud-mic", false, "Loud Mic", "Makes your microphone louder.", 0f, 0f, Shared.menu.pages[0].transform, new System.Action<bool>((state) =>
            {
                USpeaker.field_Internal_Static_Single_1 = state ? float.MaxValue : 1f;
            }));
            Shared.menu.CreateToggle("Auto Portal", "Drops portals on target every second", 1f, 0f, 0, Shared.modules.portal);
            Shared.menu.CreateToggle("Orbit", "Orbits the selected player", 2f, 0f, 0, Shared.modules.orbit);
            Shared.menu.CreateToggle("p0/infinite-portals", false, "Infinite Portals", "Dropped portals will not be deleted", -1f, -1f, Shared.menu.pages[0].transform, new System.Action<bool>((state) =>
            {
                Shared.modules.portal.infinite = state;
            }));
            Shared.menu.CreateToggle("Auto KOS", "Auto targets and portals players on the KOS list", 0f, -1f, 0, Shared.modules.kos);


            Shared.menu.CreateButton("p1/open-p2", "Next", "Opens KiraiMod's next page", -2f, 1f, Shared.menu.pages[1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 2;
            }));

            Shared.menu.CreateButton("p1/close-p1", "Back", "Close KiraiMod's extra options", -2f, 0f, Shared.menu.pages[1].transform, new System.Action(() =>
            {
                Shared.menu.selected = 0;
            }));

            Shared.menu.CreateToggle("Mod Log", "Moderation log for block, mute, and avatars", -1f, 1f, 1, Shared.modules.modlog);
            Shared.menu.CreateToggle("Nameplates", "Custom nameplates. Highlight for friends and red for KOS", 0f, 1f, 1, Shared.modules.nameplates);
            Shared.menu.CreateToggle("p1/nameplate-rgb", Shared.modules.nameplates.rgb, "RGB Nameplates", "Rainbow nameplates for friends", 1f, 1f, Shared.menu.pages[1].transform, new System.Action<bool>((state) =>
            {
                Shared.modules.nameplates.rgb = state;
                if (!state) Shared.modules.nameplates.Refresh();
            }));
        }
    }
}