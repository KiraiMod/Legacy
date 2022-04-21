using MelonLoader;
using System;
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
        public Headlight headlight;
        public Aliases aliases;
        public Mute mute;

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
            modules.Add(headlight = new Headlight());
            modules.Add(aliases = new Aliases());
            modules.Add(mute = new Mute());
        }

        public void StartCoroutines()
        {
            MelonCoroutines.Start(Shared.modules.portal.AutoPortal());
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

        public void OnConfigLoaded()
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].OnConfigLoaded();
            }
        }

        public void OnLevelWasLoaded()
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].OnLevelWasLoaded();
            }
        }

        internal void OnAvatarInitialized(VRCAvatarManager instance)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].OnAvatarInitialized(instance);
            }
        }
    }
}