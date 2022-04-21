using System;
using System.Linq;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public class Misc : ModuleBase
    {
        public bool bUseClipboard;
        public bool bAnnoyance;
        public bool bPersistantQuickMenu;
        public string newAvtr;
        public float throwSpeed;

        public bool WorldCrash;
        public bool BindsNumpad;
        public bool BindsTab;
        public bool BindsAlt;
        public bool HighStep;

        private VRC_SceneDescriptor descriptor;
        private string[] cachedPrefabs;
        private string oldAvtr;

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Force\nPickups", "Enabled theft on all pickups", ButtonType.Button, 0, Shared.PageIndex.buttons1, nameof(ForcePickups)),
            new ModuleInfo("Fast\nPickups", "Thrown pickups are very fast", ButtonType.Button, 1, Shared.PageIndex.buttons1, nameof(FastPickups)),
            new ModuleInfo("Nuke\nVideoSync", "Overrides all video players to a custom URL", ButtonType.Button, 5, Shared.PageIndex.buttons1, nameof(NukeVideoSync)),
            new ModuleInfo("Bring\nPickups", "Brings all pickups in the scene", ButtonType.Button, 6, Shared.PageIndex.buttons1, nameof(BringPickups)),
            new ModuleInfo("Drop\nTarget", "Forget the current target", ButtonType.Button, 7, Shared.PageIndex.buttons1, nameof(DropTarget)),
            new ModuleInfo("Change\nPedestals", "Change all pedestals to an avatar ID", ButtonType.Button, 3, Shared.PageIndex.buttons2, nameof(ChangePedestals)),
            new ModuleInfo("Join World\nvia ID", "Join a world using a full instance id", ButtonType.Button, 6, Shared.PageIndex.buttons2, nameof(JoinWorldByID)),
            new ModuleInfo("Clipboard", "Use the clipboard instead of a popup input", ButtonType.Toggle, 10, Shared.PageIndex.toggles2, nameof(bUseClipboard)),
            new ModuleInfo("Annoyance Mode", "Orbit things around the targets head instead of their feet", ButtonType.Toggle, 9, Shared.PageIndex.toggles2, nameof(bAnnoyance)),
            new ModuleInfo("Persistant QuickMenu", "Keep the Quick Menu open when moving around", ButtonType.Toggle, 2, Shared.PageIndex.toggles3, nameof(bPersistantQuickMenu)),
            new ModuleInfo("Numpad Binds", "Use Numlock + Keypad to activate binds", ButtonType.Toggle, 1, Shared.PageIndex.toggles3, nameof(BindsNumpad)),
            new ModuleInfo("Tab Binds", "Use Tab + Alphabetical to activate binds", ButtonType.Toggle, 5, Shared.PageIndex.toggles3, nameof(BindsTab)),
            new ModuleInfo("Alt Binds", "Use Alt + Numerical to activate binds", ButtonType.Toggle, 9, Shared.PageIndex.toggles3, nameof(BindsAlt)),
            new ModuleInfo("High Step", "Step up objects larger than you normally can", ButtonType.Toggle, 0, Shared.PageIndex.toggles2, nameof(HighStep)),
            new ModuleInfo("Spawn\nPrefab", "Spawns the first dynamic prefab in the world", ButtonType.Button, 8, Shared.PageIndex.buttons2, nameof(SpawnDynamicPrefab)),
            new ModuleInfo("World\nCrash", "Change your avatar to a world crasher safely", ButtonType.Toggle, 6, Shared.PageIndex.toggles3, nameof(WorldCrash)),
            new ModuleInfo("Throw Speed", ButtonType.Slider, 9, Shared.PageIndex.sliders1, nameof(throwSpeed), 1, 20),
        };

        public override void OnConfigLoaded()
        {
            state = true;
            MelonLoader.MelonCoroutines.Start(WaitForLoaded());
        }

        public override void OnLevelWasLoaded()
        {
            MelonLoader.MelonCoroutines.Start(WaitForPlayerToLoad());
        }

        public System.Collections.IEnumerator WaitForPlayerToLoad()
        {
            while (VRC.Player.prop_Player_0?.gameObject is null) yield return null;

            if (HighStep)
            {
                CharacterController c = VRC.Player.prop_Player_0.GetComponent<CharacterController>();
                c.stepOffset = 1.6f;
                c.slopeLimit = 90;
            }
        }

        public System.Collections.IEnumerator WaitForLoaded()
        {
            while (Shared.unloaded) yield return new WaitForSeconds(1);

            ReenableDefault();
        }

        public void ForcePickups()
        {
            VRC_Pickup[] pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
            for (int i = 0; i < pickups.Length; i++)
            {
                pickups[i].DisallowTheft = false;
            }
        }

        public void FastPickups()
        {
            VRC_Pickup[] pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
            for (int i = 0; i < pickups.Length; i++)
            {
                pickups[i].ThrowVelocityBoostScale = throwSpeed;
            }
        }

        public void NukeVideoSync()
        {
            if (bUseClipboard)
                Helper.OverrideVideoPlayers(Utils.GetClipboard().Trim());
            else
                KiraiLib.HUDInput("Video URL", "Override", "https://www.youtube.com/watch?v=LhCYW9dKC5s", new Action<string>((value) =>
                {
                    Helper.OverrideVideoPlayers(value.Trim());
                }));
        }

        public void BringPickups()
        {
            Helper.BringPickups();
        }

        public void DropTarget()
        {
            Shared.TargetPlayer = null;
        }

        public void ChangePedestals()
        {
            if (bUseClipboard)
                Helper.SetPedestals(System.Windows.Forms.Clipboard.GetText().Trim());
            else
                KiraiLib.HUDInput("Avatar ID", "Set Pedestals", "avtr_8-4-4-4-12", new Action<string>((resp) =>
                {
                    Helper.SetPedestals(resp.Trim());
                }));
        }

        public void JoinWorldByID()
        {
            if (bUseClipboard)
                Helper.JoinWorldById(System.Windows.Forms.Clipboard.GetText().Trim());
            else
                KiraiLib.HUDInput("World ID", "Join", "wrld_*:?????~*()~nonce()", new Action<string>((resp) =>
                {
                    Helper.JoinWorldById(resp.Trim());
                }));
        }

        public void OnStateChangeBindsNumpad(bool state)
        {
            if (state)
            {
                DisableOthers(0);
            }
            else ReenableDefault();
        }

        public void OnStateChangeBindsTab(bool state)
        {
            if (state)
            {
                DisableOthers(1);
            }
            else ReenableDefault();
        }

        public void OnStateChangeBindsAlt(bool state)
        {
            if (state)
            {
                DisableOthers(2);
            }
            else ReenableDefault();
        }

        private void DisableOthers(int target)
        {
            if (target > 0)
                (KiraiLib.UI.elements[Utils.CreateID("Numpad Binds", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle)?.SetState(false);

            if (target != 1)
                (KiraiLib.UI.elements[Utils.CreateID("Tab Binds", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle)?.SetState(false);

            if (target < 2)
                (KiraiLib.UI.elements[Utils.CreateID("Alt Binds", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle)?.SetState(false);
        }

        private void ReenableDefault()
        {
            if (BindsNumpad || BindsTab || BindsAlt) return;

            BindsNumpad = true;
            (KiraiLib.UI.elements[Utils.CreateID("Numpad Binds", (int)Shared.PageIndex.toggles3)] as KiraiLib.UI.Toggle)?.SetState(true);
        }

        public void OnStateChangeHighStep(bool state)
        {
            CharacterController c = VRC.Player.prop_Player_0.GetComponent<CharacterController>();
            c.stepOffset = state ? 1.6f : 0.5f;
            c.slopeLimit = state ? 90 : 60;
        }

        public void SpawnDynamicPrefab()
        {
            if (descriptor is null)
            {
                descriptor = UnityEngine.Object.FindObjectOfType<VRC_SceneDescriptor>();
                cachedPrefabs = descriptor.DynamicPrefabs.ToArray().Where(p => p?.name != null).Select(p => p.name).ToArray();
            }

            if (descriptor is null)
                KiraiLib.Logger.Log("Failed to find scene descriptor");
            else
            {
                VRC.Player target = Shared.TargetPlayer ?? VRC.Player.prop_Player_0;

                if (cachedPrefabs.Length > 0)
                    Networking.Instantiate(VRC_EventHandler.VrcBroadcastType.Always,
                        cachedPrefabs[0], 
                        bAnnoyance 
                            ? target.field_Private_VRCPlayerApi_0.GetBonePosition(HumanBodyBones.Head)
                            : target.transform.position,
                        target.transform.rotation);
                else KiraiLib.Logger.Log("World has no dynamic prefabs.");
            }
        }

        public void OnStateChangeWorldCrash(bool state)
        {
            if (state) oldAvtr = VRC.Core.APIUser.CurrentUser.avatarId;

            VRC.Player.prop_Player_0.transform.Find("ForwardDirection").gameObject.active = !state;
            AssetBundleDownloadManager.prop_AssetBundleDownloadManager_0.gameObject.active = !state;

            new VRC.UI.PageAvatar
            {
                field_Public_SimpleAvatarPedestal_0 = new VRC.SimpleAvatarPedestal
                {
                    field_Internal_ApiAvatar_0 = new VRC.Core.ApiAvatar
                    {
                        id = state ? newAvtr : oldAvtr
                    }
                }
            }.ChangeToSelectedAvatar();
        }
    }
}
