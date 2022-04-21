using MelonLoader;
using System;
using System.IO;
using System.Reflection;

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
    }
}
