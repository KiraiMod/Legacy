using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace KiraiMod
{
    public class KiraiComms : MelonMod
    {
        private System.Action<int, string> SendRPC;
        private Dictionary<string, Aes> encryptors = new Dictionary<string, Aes>();
        private RSA ourRSA;

        public override void OnApplicationStart()
        {
            if (MelonHandler.Mods.Any(m => m.Assembly.GetName().Name == "KiraiRPC"))
            {
                ourRSA = RSA.Create(4096);

                new System.Action(() =>
                {
                    SendRPC = KiraiRPC.GetSendRPC("KiraiComms");
                    KiraiRPC.callbackChain += new Action<KiraiRPC.RPCData>((data) =>
                    {
                        if (data.target == "KiraiRPC")
                        {
                            if (data.id == (int)KiraiRPC.RPCEventIDs.OnInit)
                            {
                                MelonLogger.Log("recieved init");
                                RSAParameters rsadata = ourRSA.ExportParameters(false);
                                string exponent = Convert.ToBase64String(rsadata.Exponent);
                                string modulus = Convert.ToBase64String(rsadata.Modulus);
                                SendRPC(0x000, exponent.Length.ToString("X") + exponent + modulus);
                            }
                        }
                        else if (data.target == "KiraiComms")
                        {
                            switch (data.id)
                            {
                                case 0x000:
                                    {
                                        if (int.TryParse(data.payload.Substring(0, 1), out int len))
                                        {
                                            RSAParameters rsadata = new RSAParameters();
                                            rsadata.Exponent = Convert.FromBase64String(data.payload.Substring(1, len));
                                            rsadata.Modulus = Convert.FromBase64String(data.payload.Substring(1 + len));
                                            RSA rsa = RSA.Create(rsadata);

                                            Aes aes = Aes.Create();

                                            string safeKey = Convert.ToBase64String(rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA512));
                                            string safeIV = Convert.ToBase64String(rsa.Encrypt(aes.IV, RSAEncryptionPadding.OaepSHA512));

                                            MelonLogger.Log($"Sending to {data.sender} new aes with iv of {safeIV}");
                                            MelonLogger.Log($"Sending to {data.sender} new aes with key of {safeKey}");
                                            encryptors[data.sender] = aes;

                                            SendRPC(0x001, safeKey.Length.ToString("X").PadLeft(2, '0') + safeKey + safeIV);
                                        }
                                    }
                                    break;
                                case 0x001:
                                    {
                                        if (int.TryParse(data.payload.Substring(0, 2), out int len))
                                        {
                                            Aes aes = Aes.Create();

                                            aes.Key = ourRSA.Decrypt(Convert.FromBase64String(data.payload.Substring(2, len)), RSAEncryptionPadding.OaepSHA512);
                                            aes.IV = ourRSA.Decrypt(Convert.FromBase64String(data.payload.Substring(2 + len)), RSAEncryptionPadding.OaepSHA512);

                                            MelonLogger.Log($"got from {data.sender} aes with iv of {Convert.ToBase64String(aes.IV)}");
                                            MelonLogger.Log($"got from {data.sender} aes with key of {Convert.ToBase64String(aes.Key)}");
                                            encryptors[data.sender] = aes;
                                        }
                                    }
                                    break;
                            }
                        }
                    });
                }).Invoke();
            }
            else MelonLogger.LogError("Didn't find KiraiRPC, not continuing execution.");
        }
    }
}
