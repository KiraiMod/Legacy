using MelonLoader;
using System;
using System.IO;
using VRC.Core;

namespace KiraiMod
{
    static class SDK
    {
        public static unsafe TDest ReinterpretCast<TSource, TDest>(TSource source)
        {
            var sourceRef = __makeref(source);
            var dest = default(TDest);
            var destRef = __makeref(dest);
            *(IntPtr*)&destRef = *(IntPtr*)&sourceRef;
            return __refvalue(destRef, TDest);
        }

        public static void UploadFileAsync(
            string path, 
            string id, 
            string name,
            Action<ApiFile, string> OnSuccess, 
            Action<ApiFile, string> OnError, 
            Action<ApiFile, string, string, float> OnProgress)
        {
            try
            {
                string extension = Path.GetExtension(path);
            }
            catch (System.Exception ex)
            {
                MelonLogger.LogError(ex.ToString());
            }
            MelonCoroutines.Start(ApiFileUtils.Instance.UploadFile(path, id, name, OnSuccess, OnError, OnProgress));
        }
    }
}
