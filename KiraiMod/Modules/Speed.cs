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
                oSpeedRun = movement.field_Public_Single_0;
                oSpeedWalk = movement.field_Public_Single_2;
                oSpeedStrafe = movement.field_Public_Single_1;

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
            movement.field_Public_Single_0 = SpeedRun;
            movement.field_Public_Single_2 = SpeedWalk;
            movement.field_Public_Single_1 = SpeedWalk;
        }

        public void Disable(LocomotionInputController movement)
        {
            movement.field_Public_Single_0 = oSpeedRun;
            movement.field_Public_Single_2 = oSpeedWalk;
            movement.field_Public_Single_1 = oSpeedStrafe;
        }

        public void OnValueChangeSpeedWalk(float value)
        {
            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0?.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            if (movement.field_Public_Single_2 != SpeedWalk) oSpeedWalk = movement.field_Public_Single_2;
            if (movement.field_Public_Single_1 != SpeedWalk) oSpeedStrafe = movement.field_Public_Single_1;

            SpeedWalk = value;
            if (state) Enable(movement);
        }

        public void OnValueChangeSpeedRun(float value)
        {
            LocomotionInputController movement = VRCPlayer.field_Internal_Static_VRCPlayer_0?.GetComponentInChildren<LocomotionInputController>();

            if (movement == null) return;

            if (movement.field_Public_Single_0 != SpeedRun) oSpeedRun = movement.field_Public_Single_0;

            SpeedRun = value;
            if (state) Enable(movement);
        }

    }
}