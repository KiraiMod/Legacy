using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
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
        public Aliases aliases;
        public ItemOrbit itemOrbit;
        public Tracers tracers;
        public FreeCam freecam;
        public PlayerList playerlist;
        public HideSelf hideself;
        public Udon udon;
        public Misc misc;
        public WorldCrash worldcrash;

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
            modules.Add(aliases = new Aliases());
            modules.Add(itemOrbit = new ItemOrbit());
            modules.Add(tracers = new Tracers());
            modules.Add(freecam = new FreeCam());
            modules.Add(playerlist = new PlayerList());
            modules.Add(hideself = new HideSelf());
            modules.Add(udon = new Udon());
            modules.Add(misc = new Misc());
            modules.Add(worldcrash = new WorldCrash());
        }

        public void StartCoroutines()
        {
            MelonCoroutines.Start(Shared.modules.portal.AutoPortal());
        }

        public void OnPlayerJoined(Player player)
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnPlayerJoined(player);
        }

        public void OnPlayerLeft(Player player)
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnPlayerLeft(player);
        }

        public void OnUpdate()
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnUpdate();
        }

        public void OnConfigLoaded()
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnConfigLoaded();
        }

        public void OnLevelWasLoaded()
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnLevelWasLoaded();
        }

        public void OnAvatarInitialized(GameObject avatar, VRCAvatarManager instance)
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnAvatarInitialized(avatar, instance);
        }

        public void OnUnload()
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnUnload();
        }

        public void OnReload()
        {
            for (int i = 0; i < modules.Count; i++)
                modules[i].OnReload();
        }
    }
}