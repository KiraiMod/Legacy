using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: MelonInfo(typeof(KiraiMod.KiraiLog), "KiraiLog", KiraiMod.KiraiLib.Constants.VT_ERASE, "Kirai Chan#8315")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace KiraiMod
{
    public class KiraiLog : MelonMod
    {
        static KiraiLog()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
#if DEBUG
                "KiraiMod.Lib.KiraiLib.dll"
#else
                "KiraiMod.Lib.KiraiLibLoader.dll"
#endif
            );

            MemoryStream mem = new MemoryStream((int)stream.Length);
            stream.CopyTo(mem);

            Assembly.Load(mem.ToArray());

            new Action(() =>
#if DEBUG
                KiraiLib.NoOp()
#else
                KiraiLibLoader.Load()
#endif
            )();
        }

        public override void OnApplicationStart()
        {
            KiraiLib.Events.OnOwnershipTransferred += (player) =>
            {
                KiraiLib.Logger.Display($"<color=#5600a5>[<color=#ccf>Owner</color>]</color> <color={player.field_Private_APIUser_0.GetTrustColor().ToHex()}>{player.field_Private_APIUser_0.displayName}</color>", 1);
            };
        }
    }
}
