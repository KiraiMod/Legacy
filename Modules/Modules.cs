using System.Collections.Generic;
using VRC;

namespace KiraiMod.Modules
{
    public class Modules
    {
        public Speed speed;
        public Flight flight;
        public Noclip noclip;
        public ESP esp;
        public KOS kos;
        public Portal portal;
        public Orbit orbit;
        public XUtils xutils;
        public Nameplates nameplates;
        public ModLog modlog;

        public List<ModuleBase> modules = new List<ModuleBase>();

        public Modules()
        {
            modules.Add(speed = new Speed());
            modules.Add(flight = new Flight());
            modules.Add(noclip = new Noclip());
            modules.Add(esp = new ESP());
            modules.Add(kos = new KOS());
            modules.Add(portal = new Portal());
            modules.Add(orbit = new Orbit());
            modules.Add(xutils = new XUtils());
            modules.Add(nameplates = new Nameplates());
            modules.Add(modlog = new ModLog());
        }

        public void OnPlayerJoined(Player player)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].OnPlayerJoined(player);
            }
        }

        public void OnPlayerLeft(Player player)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].OnPlayerLeft(player);
            }
        }

        public void OnUpdate()
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].OnUpdate();
            }
        }
    }
}