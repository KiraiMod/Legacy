namespace KiraiMod.Modules
{
    public class Speed : ModuleBase
    {
        private float oSpeedRun;
        private float oSpeedWalk;
        private float oSpeedStrafe;

        public float speedRun = 10;
        public float speedWalk = 8;

        public new ModuleInfo[] info =
        {
            new ModuleInfo("Speed", "Change movement speed", ButtonType.Toggle, 0, 0, nameof(state)),
            new ModuleInfo("Run Speed", null, ButtonType.Slider, 0, 3, nameof(speedRun)),
            new ModuleInfo("Walk Speed", null, ButtonType.Slider, 1, 3, nameof(speedWalk))
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

        public void Enable(LocomotionInputController movement)
        {
            movement.runSpeed = speedRun;
            movement.walkSpeed = speedWalk;
            movement.strafeSpeed = speedWalk;
        }

        public void Disable(LocomotionInputController movement)
        {
            movement.runSpeed = oSpeedRun;
            movement.walkSpeed = oSpeedWalk;
            movement.strafeSpeed = oSpeedStrafe;
        }

        public void SetWalkSpeed(float value)
        {
            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            if (movement.walkSpeed != speedWalk) oSpeedWalk = movement.walkSpeed;
            if (movement.strafeSpeed != speedWalk) oSpeedStrafe = movement.strafeSpeed;

            speedWalk = value;
            if (state) Enable(movement);
        }

        public void SetRunSpeed(float value)
        {
            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            if (movement.runSpeed != speedRun) oSpeedRun = movement.runSpeed;

            speedRun = value;
            if (state) Enable(movement);
        }

        public void Reapply()
        {
            if (!state) return;

            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) return;

            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            Enable(movement);
        }
    }
}