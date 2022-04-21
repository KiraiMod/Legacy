using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace KiraiMod
{
    public class KiraiComms : MelonMod
    {
        private System.Action<int, string> SendRPC;
        private Dictionary<string, RSA> encryptors;
        private RSA rsa;

        public override void OnApplicationStart()
        {
            rsa = RSA.Create(4096);

            if (MelonHandler.Mods.Any(m => m.Assembly.GetName().Name == "KiraiRPC"))
            {
                new System.Action(() =>
                {
                    SendRPC = KiraiRPC.GetSendRPC("KiraiComms");
                    KiraiRPC.callbackChain += new System.Action<KiraiRPC.RPCData>((data) =>
                    {
                        if (data.target == "KiraiRPC")
                        {
                            if (data.id == (int)KiraiRPC.RPCEventIDs.OnInit)
                            {

                            }
                        }
                        else if (data.target == "KiraiComms")
                        {
                            switch (data.id)
                            {

                            }
                        }
                    });
                }).Invoke();
            }
        }
    }
}
