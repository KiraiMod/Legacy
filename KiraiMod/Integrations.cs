using KiraiLibs;

namespace KiraiMod
{
    public static class Integrations
    {
        public static void Initialize()
        {
#if DEBUG
            KiraiLib.Logger.console = KiraiLib.Logger.LogLevel.TRACE;
            KiraiLib.Logger.display = KiraiLib.Logger.LogLevel.TRACE;
#endif
            if (Config.options.bUseRPC)
                KiraiLib.Libraries.LoadLibrary("KiraiRPC").ContinueWith((res) => Offload.IntegrateRPC(res.Result));
        }

        internal static class Offload
        {
            internal static void IntegrateRPC(int res)
            {
                if (res > 0)
                {
                    KiraiRPC.Callback += (data) =>
                    {
                        if (data.target == "KiraiRPC")
                        {
                            switch (data.id)
                            {
                                case (int)KiraiRPC.RPCEventIDs.OnInit:
                                    KiraiRPC.SendRPC("KiraiMod", 0x000, data.sender);
                                    break;
                            }
                        }
                        else if (data.target == "KiraiMod")
                        {
                            switch (data.id)
                            {
                                case 0x000:
                                case 0x001:
                                    if (data.parameters[0] == VRC.Player.prop_Player_0.field_Private_APIUser_0.displayName)
                                    {
                                        Shared.modules.nameplates.kmodders.Add(data.sender);
                                        Shared.modules.nameplates.Refresh();
                                        Shared.modules.playerlist.Refresh();
                                        if (data.id == 0x000)
                                            KiraiRPC.SendRPC("KiraiMod", 0x001, data.sender);
                                    }
                                    break;
                                case 0x002:
                                    if (data.parameters[0] == VRC.Player.prop_Player_0.field_Private_APIUser_0.displayName)
                                        KiraiRPC.SendRPC("KiraiMod", 0x001, data.sender);
                                    break;
                            }
                        }
                    };
                }

                if (res == 1) Shared.Options.bOSLPush = true;
            }
        }
    }
}
