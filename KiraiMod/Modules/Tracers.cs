using MelonLoader;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using System.Linq;
using VRC.Udon;

namespace KiraiMod.Modules
{
    public class Tracers : ModuleBase
    {
        public bool Players;
        public bool Pickups;
        public bool Triggers;

        public new ModuleInfo[] info = {
            new ModuleInfo("Tracers", "Draw lines from your hand to selected types", ButtonType.Toggle, 10, Shared.PageIndex.toggles1, nameof(state)),
            new ModuleInfo("Player Tracers", "Draw lines from your hands to players", ButtonType.Toggle, 0, Shared.PageIndex.toggles3, nameof(Players)),
            new ModuleInfo("Pickup Tracers", "Draw lines from your hands to pickups", ButtonType.Toggle, 4, Shared.PageIndex.toggles3, nameof(Pickups)),
            new ModuleInfo("Trigger Tracers", "Draw lines from your hands to triggers", ButtonType.Toggle, 8, Shared.PageIndex.toggles3, nameof(Triggers))
        };

        private Transform[] cache1;
        private Transform[] cache2;
        private Transform[] cache3;


        private GameObject go1;
        private GameObject go2;
        private GameObject go3;
        private LineRenderer lr1;
        private LineRenderer lr2;
        private LineRenderer lr3;

        public override void OnStateChange(bool state)
        {
            if (state) Refresh();
            if (lr1 != null) lr1.enabled = state && Players;
            if (lr2 != null) lr2.enabled = state && Pickups;
            if (lr3 != null) lr3.enabled = state && Triggers;
        }

        public override void OnUpdate()
        {
            if (!state) return;
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            Vector3 src = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;

            if (src == null) return;

            if (Players) Draw(lr1, src, cache1);
            if (Pickups) Draw(lr2, src, cache2);
            if (Triggers) Draw(lr3, src, cache3);
        }

        public override void OnLevelWasLoaded()
        {
            if (!state) return;

            Refresh();
        }

        public override void OnPlayerJoined(Player player)
        {
            if (Players) Refresh1();
        }

        public override void OnPlayerLeft(Player player)
        {
            if (Players) Refresh1();
        }

        public void OnStateChangePlayers(bool _state)
        {
            if (lr1 != null) lr1.enabled = state && Players;
            if (_state) Refresh1();
        }

        public void OnStateChangePickups(bool _state)
        {
            if (lr2 != null) lr2.enabled = state && Pickups;
            if (_state) Refresh2();
        }

        public void OnStateChangeTriggers(bool _state)
        {
            if (lr3 != null) lr3.enabled = state && Triggers;
            if (_state) Refresh3();
        }

        private void Refresh()
        {
            if (Triggers) Refresh1();
            if (Pickups) Refresh2();
            if (Players) Refresh3();

            if (go1 == null) go1 = new GameObject();
            if (go2 == null) go2 = new GameObject();
            if (go3 == null) go3 = new GameObject();

            if (lr1 == null)
            {
                lr1 = go1.AddComponent<LineRenderer>();
                SetupLineRenderer(lr1, Utils.Colors.red);
            }

            if (lr2 == null)
            {
                lr2 = go2.AddComponent<LineRenderer>();
                SetupLineRenderer(lr2, Utils.Colors.green);
            }

            if (lr3 == null)
            {
                lr3 = go3.AddComponent<LineRenderer>();
                SetupLineRenderer(lr3, Utils.Colors.aqua);
            }
        }

        private void Refresh1() => cache1 = GameObject.FindGameObjectsWithTag("Player").Select(u => u.transform).ToArray();
        private void Refresh2()
        {
            var temp = Object.FindObjectsOfType<VRC_Pickup>().Select(u => u.transform).ToArray();

            if (temp.Length == 0) cache2 = Object.FindObjectsOfType<UdonBehaviour>()
                .Where(u => u._eventTable.ContainsKey("_onPickupUseDown") || u._eventTable.ContainsKey("_onPickupUseUp"))
                .Select(u => u.transform).ToArray();
            else cache2 = temp.Select(u => u.transform).ToArray();
        }
        private void Refresh3()
        {
            VRC_Trigger[] temp = Object.FindObjectsOfType<VRC_Trigger>();

            if (temp.Length == 0) cache3 = Object.FindObjectsOfType<UdonBehaviour>()
                    .Where(u => u.IsInteractive && !u._eventTable.ContainsKey("_onPickupUseDown") && !u._eventTable.ContainsKey("_onPickupUseUp"))
                    .Select(u => u.transform).ToArray();
            else cache3 = temp.Select(u => u.transform).ToArray();
        }

        private void SetupLineRenderer(LineRenderer lr, Color color)
        {
            lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            lr.startWidth = 0.002f;
            lr.endWidth = 0.002f;
            lr.useWorldSpace = false;
            lr.endColor = color;
            lr.startColor = color;
        }

        private void Draw(LineRenderer lr, Vector3 src, Transform[] cache)
        {
            if (cache != null)
            {
                lr.positionCount = cache.Length * 2;
                for (int i = 0; i < cache.Length; i++)
                {
                    if (cache[i] != null)
                    {
                        lr.SetPosition(i * 2, src); //src
                        lr.SetPosition(i * 2 + 1, cache[i].transform.position); //dest
                    }
                }
            }
        }
    }
}
