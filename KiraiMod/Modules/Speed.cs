namespace KiraiMod.Modules
{
    public class Speed : ModuleBase
    {
        private float oSpeedRun;
        private float oSpeedWalk;
        private float oSpeedStrafe;

        public float SpeedRun = 10;
        public float SpeedWalk = 8;

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Speed", "Change movement speed", ButtonType.Toggle, 0, Shared.PageIndex.toggles1, nameof(state)),
            new ModuleInfo("Run Speed", ButtonType.Slider, 0, Shared.PageIndex.sliders1, nameof(SpeedRun), 0, 32),
            new ModuleInfo("Walk Speed", ButtonType.Slider, 1, Shared.PageIndex.sliders1, nameof(SpeedWalk), 0, 32)
        };

        public override void OnStateChange(bool state)
        {
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            if (state)
            {
                oSpeedRun = movement.runSpeed;
                oSpeedWalk = movement.walkSpeed;
                oSpeedStrafe = movement.strafeSpeed;

                Enable(movement);
            } else Disable(movement);
        }

        public override void OnLevelWasLoaded()
        {
            if (!state) return;

            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            Enable(movement);
        }

        public void Enable(LocomotionInputController movement)
        {
            movement.runSpeed = SpeedRun;
            movement.walkSpeed = SpeedWalk;
            movement.strafeSpeed = SpeedWalk;
        }

        public void Disable(LocomotionInputController movement)
        {
            movement.runSpeed = oSpeedRun;
            movement.walkSpeed = oSpeedWalk;
            movement.strafeSpeed = oSpeedStrafe;
        }

        public void OnValueChangeSpeedWalk(float value)
        {
            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0?.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            if (movement.walkSpeed != SpeedWalk) oSpeedWalk = movement.walkSpeed;
            if (movement.strafeSpeed != SpeedWalk) oSpeedStrafe = movement.strafeSpeed;

            SpeedWalk = value;
            if (state) Enable(movement);
        }

        public void OnValueChangeSpeedRun(float value)
        {
            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0?.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            if (movement.runSpeed != SpeedRun) oSpeedRun = movement.runSpeed;

            SpeedRun = value;
            if (state) Enable(movement);
        }
    }
}