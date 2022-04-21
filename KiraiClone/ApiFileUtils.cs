using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using VRC;
using VRC.Core;
using MelonLoader;
using UnhollowerBaseLib;
using System.Security.Cryptography;
using UnhollowerRuntimeLib;
using System.Runtime.InteropServices;

namespace KiraiMod
{

    public class ApiFileUtils : MonoBehaviour
    {
        public Il2CppSystem.Collections.Generic.List<MonoBehaviour> AntiGcList;
        public System.Delegate ReferencedDelegate;
        public System.IntPtr MethodInfo;

        public ApiFileUtils(System.IntPtr intptr_1) : base(intptr_1)
        {
            AntiGcList = new Il2CppSystem.Collections.Generic.List<MonoBehaviour>(1);
            AntiGcList.Add(this);
        }

        public ApiFileUtils(System.Delegate refdelegate, System.IntPtr methodinfo) : base(ClassInjector.DerivedConstructorPointer<ApiFileUtils>())
        {
            ClassInjector.DerivedConstructorBody(this);
            ReferencedDelegate = refdelegate;
            MethodInfo = methodinfo;
        }

        ~ApiFileUtils()
        {
            Marshal.FreeHGlobal(MethodInfo);
            MethodInfo = Il2CppSystem.IntPtr.Zero;
            ReferencedDelegate = null;
            AntiGcList.Remove(this);
            AntiGcList = null;
        }

        private readonly int kMultipartUploadChunkSize = 10 * 1024 * 1024;
        private readonly int SERVER_PROCESSING_WAIT_TIMEOUT_CHUNK_SIZE = 50 * 1024 * 1024;
        private readonly float SERVER_PROCESSING_WAIT_TIMEOUT_PER_CHUNK_SIZE = 120.0f;
        private readonly float SERVER_PROCESSING_MAX_WAIT_TIMEOUT = 600.0f;
        private readonly float SERVER_PROCESSING_INITIAL_RETRY_TIME = 2.0f;
        private readonly float SERVER_PROCESSING_MAX_RETRY_TIME = 10.0f;

        public bool EnableDeltaCompression = false;

        private readonly Regex[] kUnityPackageAssetNameFilters = new Regex[]
        {
            new Regex(@"/LightingData\.asset$"),                    // lightmap base asset
            new Regex(@"/Lightmap-.*(\.png|\.exr)$"),               // lightmaps
            new Regex(@"/ReflectionProbe-.*(\.exr|\.png)$"),        // reflection probes
            new Regex(@"/Editor/Data/UnityExtensions/")             // anything that looks like part of the Unity installation
        };

        public delegate void OnFileOpSuccess(VRC.Core.ApiFile apiFile, string message);
        public delegate void OnFileOpError(VRC.Core.ApiFile apiFile, string error);
        public delegate void OnFileOpProgress(VRC.Core.ApiFile apiFile, string status, string subStatus, float pct);
        public delegate bool FileOpCancelQuery(VRC.Core.ApiFile apiFile);

        public static ApiFileUtils Instance
        {
            get
            {
                CheckInstance();
                return mInstance;
            }
        }

        private static ApiFileUtils mInstance = null;

        public static RemoteConfig config;

        const float kPostWriteDelay = 0.75f;

        public enum FileOpResult
        {
            Success,
            Unchanged
        }

        public static void UploadFileAsync(string filename, string existingFileId, string friendlyName,
            OnFileOpSuccess onSuccess, OnFileOpError onError, OnFileOpProgress onProgress, FileOpCancelQuery cancelQuery)
        {
            try
            {
                string extension = Path.GetExtension(filename);
                MelonLogger.Log("Uploading " + extension + "..");
            }
            catch (System.Exception ex)
            {
                MelonLogger.LogError(ex.ToString());
            }
            MelonCoroutines.Start(Instance.UploadFile(filename, existingFileId, friendlyName, onSuccess, onError,
                onProgress, cancelQuery));
        }

        public static string GetMimeTypeFromExtension(string extension)
        {
            if (extension == ".vrcw")
                return "application/x-world";
            if (extension == ".vrca")
                return "application/x-avatar";
            if (extension == ".dll")
                return "application/x-msdownload";
            if (extension == ".unitypackage")
                return "application/gzip";
            if (extension == ".gz")
                return "application/gzip";
            if (extension == ".jpg")
                return "image/jpg";
            if (extension == ".png")
                return "image/png";
            if (extension == ".sig")
                return "application/x-rsync-signature";
            if (extension == ".delta")
                return "application/x-rsync-delta";

            MelonLogger.LogWarning("Unknown file extension for mime-type: " + extension);
            return "application/octet-stream";
        }

        public static bool IsGZipCompressed(string filename)
        {
            return GetMimeTypeFromExtension(Path.GetExtension(filename)) == "application/gzip";
        }


        public IEnumerator UploadFile(string filename, string existingFileId, string friendlyName,
            OnFileOpSuccess onSuccess, OnFileOpError onError, OnFileOpProgress onProgress, FileOpCancelQuery cancelQuery)
        {
            if (!config.IsInitialized())
            {
                
                bool bool_ = false;
                config.Init((System.Action)delegate
                {
                    bool_ = true;
                }, (System.Action)delegate
                {
                    bool_ = true;
                });
                while (!bool_)
                {
                    yield return null;
                }
                if (!config.IsInitialized())
                {
                    Error(onError, null, "Failed to fetch configuration.");
                    yield break;
                }
            }
            // validate input file
            
            Instance.EnableDeltaCompression = config.GetBool("sdkEnableDeltaCompression");
            Progress(onProgress, null, "Checking file...");

            if (string.IsNullOrEmpty(filename))
            {
                Error(onError, null, "Upload filename is empty!");
                yield break;
            }
            

            if (!System.IO.Path.HasExtension(filename))
            {
                Error(onError, null, "Upload filename must have an extension: " + filename);
                yield break;
            }
            


            string whyNot;
            if (!VRC.Tools.FileCanRead(filename, out whyNot))
            {
                Error(onError, null, "Could not read file to upload!", filename + "\n" + whyNot);
                yield break;
            }
            

            // get or create ApiFile
            Progress(onProgress, null, string.IsNullOrEmpty(existingFileId) ? "Creating file record..." : "Getting file record...");

            bool wait = true;
            bool wasError = false;
            bool worthRetry = false;
            string errorStr = "";
            

            if (string.IsNullOrEmpty(friendlyName))
                friendlyName = filename;
            

            string extension = System.IO.Path.GetExtension(filename);
            string mimeType = GetMimeTypeFromExtension(extension);

            VRC.Core.ApiFile apiFile = null;

            System.Action<ApiContainer> fileSuccess = (ApiContainer c) =>
            {
                apiFile = c.Model.Cast<ApiFile>();
                wait = false;
            };
            

            System.Action<ApiContainer> fileFailure = (ApiContainer c) =>
            {
                errorStr = c.Error;
                wait = false;

                if (c.Code == 400)
                    worthRetry = true;
            };
            

            while (true)
            {
                apiFile = null;
                wait = true;
                worthRetry = false;
                errorStr = "";

                if (string.IsNullOrEmpty(existingFileId))
                    VRC.Core.ApiFile.Create(friendlyName, mimeType, extension, fileSuccess, fileFailure);
                else
                    API.Fetch<VRC.Core.ApiFile>(existingFileId, fileSuccess, fileFailure);

                while (wait)
                {
                    if (apiFile != null && CheckCancelled(cancelQuery, onError, apiFile))
                        yield break;

                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (errorStr.Contains("File not found"))
                    {
                        existingFileId = "";
                        continue;
                    }

                    string msg = string.IsNullOrEmpty(existingFileId) ? "Failed to create file record." : "Failed to get file record.";
                    Error(onError, null, msg, errorStr);

                    if (!worthRetry)
                        yield break;
                }

                if (!worthRetry)
                    break;
                else
                    yield return new WaitForSecondsRealtime(kPostWriteDelay);
            }
            

            if (apiFile == null)
                yield break;
            

            while (apiFile.HasQueuedOperation(EnableDeltaCompression))
            {
                wait = true;

                apiFile.DeleteLatestVersion((System.Action<ApiContainer>)delegate
                {
                    wait = false;
                }, (System.Action<ApiContainer>)delegate
                {
                    wait = false;
                });
                while (wait)
                {
                    if (apiFile != null && CheckCancelled(cancelQuery, onError, apiFile))
                        yield break;

                    yield return null;
                }
            }
            

            // delay to let write get through servers
            yield return new WaitForSecondsRealtime(kPostWriteDelay);

            

            // check for server side errors from last upload
            if (apiFile.IsInErrorState())
            {
                MelonLogger.LogWarning("ApiFile: " + apiFile.id + ": server failed to process last uploaded, deleting failed version");

                while (true)
                {
                    // delete previous failed version
                    Progress(onProgress, apiFile, "Preparing file for upload...", "Cleaning up previous version");

                    wait = true;
                    errorStr = "";
                    worthRetry = false;

                    apiFile.DeleteLatestVersion(fileSuccess, fileFailure);

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onError, null))
                        {
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        Error(onError, apiFile, "Failed to delete previous failed version!", errorStr);
                        if (!worthRetry)
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }
                    }

                    if (worthRetry)
                        yield return new WaitForSecondsRealtime(kPostWriteDelay);
                    else
                        break;
                }
            }
            

            // delay to let write get through servers
            yield return new WaitForSecondsRealtime(kPostWriteDelay);

            // verify previous file op is complete
            if (apiFile.HasQueuedOperation(EnableDeltaCompression))
            {
                Error(onError, apiFile, "A previous upload is still being processed. Please try again later.");
                yield break;
            }
            

            // prepare file for upload
            Progress(onProgress, apiFile, "Preparing file for upload...", "Optimizing file");
            

            string uploadFilename = VRC.Tools.GetTempFileName(Path.GetExtension(filename), out errorStr, apiFile.id);
            if (string.IsNullOrEmpty(uploadFilename))
            {
                Error(onError, apiFile, "Failed to optimize file for upload.", "Failed to create temp file: \n" + errorStr);
                yield break;
            }
            

            wasError = false;
            yield return MelonCoroutines.Start(CreateOptimizedFileInternal(filename, uploadFilename,
                delegate (FileOpResult res)
                {
                    if (res == FileOpResult.Unchanged)
                        uploadFilename = filename;
                },
                delegate (string error)
                {
                    Error(onError, apiFile, "Failed to optimize file for upload.", error);
                    CleanupTempFiles(apiFile.id);
                    wasError = true;
                })
            );
            

            if (wasError)
                yield break;

            // generate md5 and check if file has changed
            Progress(onProgress, apiFile, "Preparing file for upload...", "Generating file hash");

            string fileMD5Base64 = "";
            wait = true;
            errorStr = "";
            fileMD5Base64 = System.Convert.ToBase64String(MD5.Create().ComputeHash(File.ReadAllBytes(uploadFilename)));
            
            wait = false;
            while (wait)
            {
                if (CheckCancelled(cancelQuery, onError, apiFile))
                {
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
                yield return null;
            }
            

            if (!string.IsNullOrEmpty(errorStr))
            {
                Error(onError, apiFile, "Failed to generate MD5 hash for upload file.", errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }
            
            // check if file has been changed
            Progress(onProgress, apiFile, "Preparing file for upload...", "Checking for changes");
            

            bool isPreviousUploadRetry = false;
            if (apiFile.HasExistingOrPendingVersion())
            {
                // uploading the same file?
                if (string.Compare(fileMD5Base64, apiFile.GetFileMD5(apiFile.GetLatestVersionNumber())) == 0)
                {
                    // the previous operation completed successfully?
                    if (!apiFile.IsWaitingForUpload())
                    {
                        Success(onSuccess, apiFile, "The file to upload is unchanged.");
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }
                    else
                    {
                        isPreviousUploadRetry = true;
                    }
                }
                else
                {
                    // the file has been modified
                    if (apiFile.IsWaitingForUpload())
                    {
                        // previous upload failed, and the file is changed
                        while (true)
                        {
                            // delete previous failed version
                            Progress(onProgress, apiFile, "Preparing file for upload...", "Cleaning up previous version");

                            wait = true;
                            worthRetry = false;
                            errorStr = "";

                            apiFile.DeleteLatestVersion(fileSuccess, fileFailure);

                            while (wait)
                            {
                                if (CheckCancelled(cancelQuery, onError, apiFile))
                                {
                                    yield break;
                                }
                                yield return null;
                            }

                            if (!string.IsNullOrEmpty(errorStr))
                            {
                                Error(onError, apiFile, "Failed to delete previous incomplete version!", errorStr);
                                if (!worthRetry)
                                {
                                    CleanupTempFiles(apiFile.id);
                                    yield break;
                                }
                            }

                            // delay to let write get through servers
                            yield return new WaitForSecondsRealtime(kPostWriteDelay);

                            if (!worthRetry)
                                break;
                        }
                    }
                }
            }
            


            // generate signature for new file

            Progress(onProgress, apiFile, "Preparing file for upload...", "Generating signature");

            string signatureFilename = VRC.Tools.GetTempFileName(".sig", out errorStr, apiFile.id);
            

            if (string.IsNullOrEmpty(signatureFilename))
            {
                Error(onError, apiFile, "Failed to generate file signature!", "Failed to create temp file: \n" + errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            wasError = false;
            yield return MelonCoroutines.Start(CreateFileSignatureInternal(uploadFilename, signatureFilename,
                delegate ()
                {
                    // success!
                },
                delegate (string error)
                {
                    Error(onError, apiFile, "Failed to generate file signature!", error);
                    CleanupTempFiles(apiFile.id);
                    wasError = true;
                })
            );
            

            if (wasError)
                yield break;


            // generate signature md5 and file size
            Progress(onProgress, apiFile, "Preparing file for upload...", "Generating signature hash");
            

            string sigMD5Base64 = "";
            wait = true;
            errorStr = "";
            sigMD5Base64 = System.Convert.ToBase64String(MD5.Create().ComputeHash(File.ReadAllBytes(signatureFilename)));
            wait = false;
            while (wait)
            {
                if (CheckCancelled(cancelQuery, onError, apiFile))
                {
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
                yield return null;
            }

            if (!string.IsNullOrEmpty(errorStr))
            {
                Error(onError, apiFile, "Failed to generate MD5 hash for signature file.", errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }
            

            long sigFileSize = 0;
            if (!VRC.Tools.GetFileSize(signatureFilename, out sigFileSize, out errorStr))
            {
                Error(onError, apiFile, "Failed to generate file signature!", "Couldn't get file size:\n" + errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }
            


            // download previous version signature (if exists)
            string existingFileSignaturePath = null;
            if (EnableDeltaCompression && apiFile.HasExistingVersion())
            {
                Progress(onProgress, apiFile, "Preparing file for upload...", "Downloading previous version signature");

                wait = true;
                errorStr = "";

                apiFile.DownloadSignature((System.Action<Il2CppStructArray<byte>>)delegate (Il2CppStructArray<byte> il2CppStructArray_0)
                {
                    existingFileSignaturePath = Tools.GetTempFileName(".sig", out errorStr, apiFile.id);
                    if (!string.IsNullOrEmpty(existingFileSignaturePath))
                    {
                        try
                        {
                            File.WriteAllBytes(existingFileSignaturePath, il2CppStructArray_0);
                        }
                        catch (System.Exception ex)
                        {
                            existingFileSignaturePath = null;
                            errorStr = "Failed to write signature temp file:\n" + ex.Message;
                        }
                        wait = false;
                    }
                    else
                    {
                        errorStr = "Failed to create temp file: \n" + errorStr;
                        wait = false;
                    }
                }, (System.Action<string>)delegate (string error)
                {
                    errorStr = error;
                    wait = false;
                }, (System.Action<long, long>)delegate (long downloaded, long length)
                {
                    Progress(onProgress, apiFile, "Preparing file for upload...", "Downloading previous version signature", Tools.DivideSafe(downloaded, length));
                });
                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onError, apiFile))
                    {
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }
                    yield return null;
                }
                

                if (!string.IsNullOrEmpty(errorStr))
                {
                    Error(onError, apiFile, "Failed to download previous file version signature.", errorStr);
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
            }
            

            // create delta if needed
            string deltaFilename = null;

            if (EnableDeltaCompression && !string.IsNullOrEmpty(existingFileSignaturePath))
            {
                Progress(onProgress, apiFile, "Preparing file for upload...", "Creating file delta");

                deltaFilename = VRC.Tools.GetTempFileName(".delta", out errorStr, apiFile.id);
                if (string.IsNullOrEmpty(deltaFilename))
                {
                    Error(onError, apiFile, "Failed to create file delta for upload.", "Failed to create temp file: \n" + errorStr);
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }

                wasError = false;
                yield return MelonCoroutines.Start(CreateFileDeltaInternal(uploadFilename, existingFileSignaturePath, deltaFilename,
                    delegate ()
                    {
                        // success!
                    },
                    delegate (string error)
                    {
                        Error(onError, apiFile, "Failed to create file delta for upload.", error);
                        CleanupTempFiles(apiFile.id);
                        wasError = true;
                    })
                );

                if (wasError)
                    yield break;
            }
            

            // upload smaller of delta and new file
            long fullFizeSize = 0;
            long deltaFileSize = 0;
            if (!VRC.Tools.GetFileSize(uploadFilename, out fullFizeSize, out errorStr) ||
                (!string.IsNullOrEmpty(deltaFilename) && !VRC.Tools.GetFileSize(deltaFilename, out deltaFileSize, out errorStr)))
            {
                Error(onError, apiFile, "Failed to create file delta for upload.", "Couldn't get file size: " + errorStr);
                CleanupTempFiles(apiFile.id);
                yield break;
            }
            
            bool uploadDeltaFile = EnableDeltaCompression && deltaFileSize > 0 && deltaFileSize < fullFizeSize;

            string deltaMD5Base64 = "";
            if (uploadDeltaFile)
            {
                Progress(onProgress, apiFile, "Preparing file for upload...", "Generating file delta hash");

                wait = true;
                errorStr = "";

                deltaMD5Base64 = System.Convert.ToBase64String(MD5.Create().ComputeHash(File.ReadAllBytes(deltaFilename)));
                wait = false;

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onError, apiFile))
                    {
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    Error(onError, apiFile, "Failed to generate file delta hash.", errorStr);
                    CleanupTempFiles(apiFile.id);
                    yield break;
                }
            }
            

            // validate existing pending version info, if this is a retry
            bool versionAlreadyExists = false;

            if (isPreviousUploadRetry)
            {
                bool isValid = true;

                VRC.Core.ApiFile.Version v = apiFile.GetVersion(apiFile.GetLatestVersionNumber());
                if (v != null)
                {
                    if (uploadDeltaFile)
                    {
                        isValid = deltaFileSize == v.delta.sizeInBytes &&
                            deltaMD5Base64.CompareTo(v.delta.md5) == 0 &&
                            sigFileSize == v.signature.sizeInBytes &&
                            sigMD5Base64.CompareTo(v.signature.md5) == 0;
                    }
                    else
                    {
                        isValid = fullFizeSize == v.file.sizeInBytes &&
                            fileMD5Base64.CompareTo(v.file.md5) == 0 &&
                            sigFileSize == v.signature.sizeInBytes &&
                            sigMD5Base64.CompareTo(v.signature.md5) == 0;
                    }
                }
                else
                {
                    isValid = false;
                }

                if (isValid)
                {
                    versionAlreadyExists = true;
  
              }
                else
                {
                    // delete previous invalid version
                    Progress(onProgress, apiFile, "Preparing file for upload...", "Cleaning up previous version");

                    while (true)
                    {
                        wait = true;
                        errorStr = "";
                        worthRetry = false;

                        apiFile.DeleteLatestVersion(fileSuccess, fileFailure);

                        while (wait)
                        {
                            if (CheckCancelled(cancelQuery, onError, null))
                            {
                                yield break;
                            }
                            yield return null;
                        }

                        if (!string.IsNullOrEmpty(errorStr))
                        {
                            Error(onError, apiFile, "Failed to delete previous incomplete version!", errorStr);
                            if (!worthRetry)
                            {
                                CleanupTempFiles(apiFile.id);
                                yield break;
                            }
                        }

                        // delay to let write get through servers
                        yield return new WaitForSecondsRealtime(kPostWriteDelay);

                        if (!worthRetry)
                            break;
                    }
                }
            }

            // create new version of file
            if (!versionAlreadyExists)
            {
                while (true)
                {
                    Progress(onProgress, apiFile, "Creating file version record...");

                    wait = true;
                    errorStr = "";
                    worthRetry = false;

                    if (uploadDeltaFile)
                        // delta file
                        apiFile.CreateNewVersion(VRC.Core.ApiFile.Version.FileType.Delta, deltaMD5Base64, deltaFileSize, sigMD5Base64, sigFileSize, fileSuccess, fileFailure);
                    else
                        // full file
                        apiFile.CreateNewVersion(VRC.Core.ApiFile.Version.FileType.Full, fileMD5Base64, fullFizeSize, sigMD5Base64, sigFileSize, fileSuccess, fileFailure);

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onError, apiFile))
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        Error(onError, apiFile, "Failed to create file version record.", errorStr);
                        if (!worthRetry)
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }
                    }

                    // delay to let write get through servers
                    yield return new WaitForSecondsRealtime(kPostWriteDelay);

                    if (!worthRetry)
                        break;
                }
            }
            

            // upload components


            // upload delta
            if (uploadDeltaFile)
            {
                if (apiFile.GetLatestVersion().delta.status == VRC.Core.ApiFile.Status.Waiting)
                {
                    Progress(onProgress, apiFile, "Uploading file delta...");

                    wasError = false;
                    yield return MelonCoroutines.Start(UploadFileComponentInternal(apiFile,
                        VRC.Core.ApiFile.Version.FileDescriptor.Type.delta, deltaFilename, deltaMD5Base64, deltaFileSize,
                        delegate (VRC.Core.ApiFile file)
                        {
                            apiFile = file;
                        },
                        delegate (string error)
                        {
                            Error(onError, apiFile, "Failed to upload file delta.", error);
                            CleanupTempFiles(apiFile.id);
                            wasError = true;
                        },
                        delegate (long downloaded, long length)
                        {
                            Progress(onProgress, apiFile, "Uploading file delta...", "", Tools.DivideSafe(downloaded, length));
                        },
                        cancelQuery)
                    );

                    if (wasError)
                        yield break;
                }
            }
            // upload file
            else
            {
                if (apiFile.GetLatestVersion().file.status == VRC.Core.ApiFile.Status.Waiting)
                {
                    Progress(onProgress, apiFile, "Uploading file...");

                    wasError = false;
                    yield return MelonCoroutines.Start(UploadFileComponentInternal(apiFile,
                        VRC.Core.ApiFile.Version.FileDescriptor.Type.file, uploadFilename, fileMD5Base64, fullFizeSize,
                        delegate (VRC.Core.ApiFile file)
                        {
                            apiFile = file;
                        },
                        delegate (string error)
                        {
                            Error(onError, apiFile, "Failed to upload file.", error);
                            CleanupTempFiles(apiFile.id);
                            wasError = true;
                        },
                        delegate (long downloaded, long length)
                        {
                            Progress(onProgress, apiFile, "Uploading file...", "", Tools.DivideSafe(downloaded, length));
                        },
                        cancelQuery)
                    );

                    if (wasError)
                        yield break;
                }
            }
            


            // upload signature
            if (apiFile.GetLatestVersion().signature.status == VRC.Core.ApiFile.Status.Waiting)
            {
                Progress(onProgress, apiFile, "Uploading file signature...");

                wasError = false;
                yield return MelonCoroutines.Start(UploadFileComponentInternal(apiFile,
                    VRC.Core.ApiFile.Version.FileDescriptor.Type.signature, signatureFilename, sigMD5Base64, sigFileSize,
                    delegate (VRC.Core.ApiFile file)
                    {
                        apiFile = file;
                    },
                    delegate (string error)
                    {
                        Error(onError, apiFile, "Failed to upload file signature.", error);
                        CleanupTempFiles(apiFile.id);
                        wasError = true;
                    },
                    delegate (long downloaded, long length)
                    {
                        Progress(onProgress, apiFile, "Uploading file signature...", "", Tools.DivideSafe(downloaded, length));
                    },
                    cancelQuery)
                );

                if (wasError)
                    yield break;
            }
            


            // Validate file records queued or complete
            Progress(onProgress, apiFile, "Validating upload...");

            bool isUploadComplete = (uploadDeltaFile
                ? apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), VRC.Core.ApiFile.Version.FileDescriptor.Type.delta).status == VRC.Core.ApiFile.Status.Complete
                : apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), VRC.Core.ApiFile.Version.FileDescriptor.Type.file).status == VRC.Core.ApiFile.Status.Complete);
            isUploadComplete = isUploadComplete &&
                               apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), VRC.Core.ApiFile.Version.FileDescriptor.Type.signature).status == VRC.Core.ApiFile.Status.Complete;

            if (!isUploadComplete)
            {
                Error(onError, apiFile, "Failed to upload file.", "Record status is not 'complete'");
                CleanupTempFiles(apiFile.id);
                yield break;
            }

            bool isServerOpQueuedOrComplete = (uploadDeltaFile
                ? apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), VRC.Core.ApiFile.Version.FileDescriptor.Type.file).status != VRC.Core.ApiFile.Status.Waiting
                : apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), VRC.Core.ApiFile.Version.FileDescriptor.Type.delta).status != VRC.Core.ApiFile.Status.Waiting);

            if (!isServerOpQueuedOrComplete)
            {
                Error(onError, apiFile, "Failed to upload file.", "Record is still in 'waiting' status");
                CleanupTempFiles(apiFile.id);
                yield break;
            }
            


            // wait for server processing to complete
            Progress(onProgress, apiFile, "Processing upload...");
            float checkDelay = SERVER_PROCESSING_INITIAL_RETRY_TIME;
            float maxDelay = SERVER_PROCESSING_MAX_RETRY_TIME;
            float timeout = GetServerProcessingWaitTimeoutForDataSize(apiFile.GetLatestVersion().file.sizeInBytes);
            double initialStartTime = Time.realtimeSinceStartup;
            double startTime = initialStartTime;
            while (apiFile.HasQueuedOperation(uploadDeltaFile))
            {
                // wait before polling again
                Progress(onProgress, apiFile, "Processing upload...", "Checking status in " + Mathf.CeilToInt(checkDelay) + " seconds");

                while (Time.realtimeSinceStartup - startTime < checkDelay)
                {
                    if (CheckCancelled(cancelQuery, onError, apiFile))
                    {
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }

                    if (Time.realtimeSinceStartup - initialStartTime > timeout)
                    {

                        Error(onError, apiFile, "Timed out waiting for upload processing to complete.");
                        CleanupTempFiles(apiFile.id);
                        yield break;
                    }

                    yield return null;
                }

                while (true)
                {
                    // check status
                    Progress(onProgress, apiFile, "Processing upload...", "Checking status...");

                    wait = true;
                    worthRetry = false;
                    errorStr = "";
                    API.Fetch<VRC.Core.ApiFile>(apiFile.id, fileSuccess, fileFailure);

                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onError, apiFile))
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        Error(onError, apiFile, "Checking upload status failed.", errorStr);
                        if (!worthRetry)
                        {
                            CleanupTempFiles(apiFile.id);
                            yield break;
                        }
                    }

                    if (!worthRetry)
                        break;
                }

                checkDelay = Mathf.Min(checkDelay * 2, maxDelay);
                startTime = Time.realtimeSinceStartup;
            }
            

            // cleanup and wait for it to finish
            yield return MelonCoroutines.Start(CleanupTempFilesInternal(apiFile.id));

            Success(onSuccess, apiFile, "Upload complete!");
            

        }

        public IEnumerator CreateOptimizedFileInternal(string filename, string outputFilename, Action<FileOpResult> onSuccess, Action<string> onError)
        {

          // assume it's a .gz, or a .unitypackage
            // else nothing to do

#if !UNITY_ANDROID

            if (!IsGZipCompressed(filename))
            {
                // nothing to do
                if (onSuccess != null)
                    onSuccess(FileOpResult.Unchanged);
                yield break;
            }

            bool isUnityPackage = string.Compare(Path.GetExtension(filename), ".unitypackage", true) == 0;

            yield return null;

            // open file
            const int kGzipBufferSize = 256 * 1024;
            Stream inStream = null;
            try
            {
                inStream = new DotZLib.GZipStream(filename, kGzipBufferSize);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't read file: " + filename + "\n" + e.Message);
                yield break;
            }

            yield return null;

            // create output
            DotZLib.GZipStream outStream = null;
            try
            {
                outStream = new DotZLib.GZipStream(outputFilename, DotZLib.CompressLevel.Best, true, kGzipBufferSize);    // this lib supports rsyncable output
            }
            catch (Exception e)
            {
                if (inStream != null)
                    inStream.Close();
                if (onError != null)
                    onError("Couldn't create output file: " + outputFilename + "\n" + e.Message);
                yield break;
            }

            yield return null;

            // copy / filter file
            if (isUnityPackage)
            {
                try
                {
                    // discard files in the package we don't need

                    // scan package and make list of asset guids we don't want
                    List<string> assetGuidsToStrip = new List<string>();
                    {
                        byte[] filenameBuf = new byte[4096];
                        MelonLoader.ICSharpCode.SharpZipLib.Tar.TarInputStream tarInputStream = new MelonLoader.ICSharpCode.SharpZipLib.Tar.TarInputStream(inStream);
                        MelonLoader.ICSharpCode.SharpZipLib.Tar.TarEntry tarEntry = tarInputStream.GetNextEntry();
                        while (tarEntry != null)
                        {
                            if (tarEntry.Size > 0 && tarEntry.Name.EndsWith("/pathname", StringComparison.OrdinalIgnoreCase))
                            {
                                int bytesRead = tarInputStream.Read(filenameBuf, 0, (int)tarEntry.Size);
                                if (bytesRead > 0)
                                {
                                    string assetFilename = System.Text.ASCIIEncoding.ASCII.GetString(filenameBuf, 0, bytesRead);
                                    if (kUnityPackageAssetNameFilters.Any(r => r.IsMatch(assetFilename)))
                                    {
                                        string assetGuid = assetFilename.Substring(0, assetFilename.IndexOf('/'));
                                        // DebugLog("-- stripped file from package: " + assetGuid + " - " + assetFilename);
                                        assetGuidsToStrip.Add(assetGuid);
                                    }
                                }
                            }

                            tarEntry = tarInputStream.GetNextEntry();
                        }

                        tarInputStream.Close();
                    }

                    // rescan input .tar and copy only entries we want to the output
                    {
                        inStream.Close();
                        inStream = new DotZLib.GZipStream(filename, kGzipBufferSize);

                        MelonLoader.ICSharpCode.SharpZipLib.Tar.TarOutputStream tarOutputStream = new MelonLoader.ICSharpCode.SharpZipLib.Tar.TarOutputStream(outStream);

                        MelonLoader.ICSharpCode.SharpZipLib.Tar.TarInputStream tarInputStream = new MelonLoader.ICSharpCode.SharpZipLib.Tar.TarInputStream(inStream);
                        MelonLoader.ICSharpCode.SharpZipLib.Tar.TarEntry tarEntry = tarInputStream.GetNextEntry();
                        while (tarEntry != null)
                        {
                            string assetGuid = tarEntry.Name.Substring(0, tarEntry.Name.IndexOf('/'));
                            bool strip = assetGuidsToStrip.Any(s => string.Compare(s, assetGuid) == 0);
                            if (!strip)
                            {
                                tarOutputStream.PutNextEntry(tarEntry);
                                tarInputStream.CopyEntryContents(tarOutputStream);
                                tarOutputStream.CloseEntry();
                            }

                            tarEntry = tarInputStream.GetNextEntry();
                        }

                        tarInputStream.Close();
                        tarOutputStream.Close();
                    }
                }
                catch (Exception e)
                {
                    if (inStream != null)
                        inStream.Close();
                    if (outStream != null)
                        outStream.Close();
                    if (onError != null)
                        onError("Failed to strip and recompress file." + "\n" + e.Message);
                    yield break;
                }
            }
            else
            {
                // not a unitypackage

                // straight stream copy
                try
                {
                    const int bufSize = 256 * 1024;
                    byte[] buf = new byte[bufSize];
                    MelonLoader.ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(inStream, outStream, buf);
                }
                catch (Exception e)
                {
                    if (inStream != null)
                        inStream.Close();
                    if (outStream != null)
                        outStream.Close();
                    if (onError != null)
                        onError("Failed to recompress file." + "\n" + e.Message);
                    yield break;
                }
            }

            yield return null;

            if (inStream != null)
                inStream.Close();
            inStream = null;
            if (outStream != null)
                outStream.Close();
            outStream = null;
            yield return null;

            if (onSuccess != null)
                onSuccess(FileOpResult.Success);
#else
            yield return null;
            //if (onError != null)
            //    onError("Not supported on ANDROID platform.");

            DebugLog("CreateOptimizedFile: Android unsupported");
            if (onSuccess != null)
                onSuccess(FileOpResult.Unchanged);
            yield break;
#endif
        }

        public IEnumerator CreateFileSignatureInternal(string filename, string outputSignatureFilename, Action onSuccess, Action<string> onError)
        {
            yield return null;

            Stream inStream = null;
            FileStream outStream = null;
            byte[] buf = new byte[64 * 1024];
            IAsyncResult asyncRead = null;
            IAsyncResult asyncWrite = null;

            try
            {
                inStream = librsync.net.Librsync.ComputeSignature(File.OpenRead(filename));
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't open input file: " + e.Message);
                yield break;
            }

            try
            {
                outStream = File.Open(outputSignatureFilename, FileMode.Create, FileAccess.Write);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't create output file: " + e.Message);
                yield break;
            }

            while (true)
            {
                try
                {
                    asyncRead = inStream.BeginRead(buf, 0, buf.Length, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                while (!asyncRead.IsCompleted)
                    yield return null;

                int read = 0;
                try
                {
                    read = inStream.EndRead(asyncRead);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                if (read <= 0)
                    break;

                try
                {
                    asyncWrite = outStream.BeginWrite(buf, 0, read, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }

                while (!asyncWrite.IsCompleted)
                    yield return null;

                try
                {
                    outStream.EndWrite(asyncWrite);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }
            }

            inStream.Close();
            outStream.Close();

            yield return null;

            if (onSuccess != null)
                onSuccess();
        }

        public IEnumerator CreateFileDeltaInternal(string newFilename, string existingFileSignaturePath, string outputDeltaFilename, Action onSuccess, Action<string> onError)
        {
            yield return null;

            Stream inStream = null;
            FileStream outStream = null;
            byte[] buf = new byte[64 * 1024];
            IAsyncResult asyncRead = null;
            IAsyncResult asyncWrite = null;

            try
            {
                inStream = librsync.net.Librsync.ComputeDelta(File.OpenRead(existingFileSignaturePath), File.OpenRead(newFilename));
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't open input file: " + e.Message);
                yield break;
            }

            try
            {
                outStream = File.Open(outputDeltaFilename, FileMode.Create, FileAccess.Write);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't create output file: " + e.Message);
                yield break;
            }

            while (true)
            {
                try
                {
                    asyncRead = inStream.BeginRead(buf, 0, buf.Length, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                while (!asyncRead.IsCompleted)
                    yield return null;

                int read = 0;
                try
                {
                    read = inStream.EndRead(asyncRead);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                if (read <= 0)
                    break;

                try
                {
                    asyncWrite = outStream.BeginWrite(buf, 0, read, null, null);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }

                while (!asyncWrite.IsCompleted)
                    yield return null;

                try
                {
                    outStream.EndWrite(asyncWrite);
                }
                catch (Exception e)
                {
                    if (onError != null)
                        onError("Couldn't write file: " + e.Message);
                    yield break;
                }
            }

            inStream.Close();
            outStream.Close();

            yield return null;

            if (onSuccess != null)
                onSuccess();
        }

        protected static void Success(OnFileOpSuccess onSuccess, VRC.Core.ApiFile apiFile, string message)
        {
            if (apiFile == null)
                apiFile = new VRC.Core.ApiFile();

            if (onSuccess != null)
                onSuccess(apiFile, message);
        }

        protected static void Error(OnFileOpError onError, VRC.Core.ApiFile apiFile, string error, string moreInfo = "")
        {
            if (apiFile == null)
                apiFile = new VRC.Core.ApiFile();

            if (onError != null)
                onError(apiFile, error);
        }

        protected static void Progress(OnFileOpProgress onProgress, VRC.Core.ApiFile apiFile, string status, string subStatus = "", float pct = 0.0f)
        {
            if (apiFile == null)
                apiFile = new VRC.Core.ApiFile();

            if (onProgress != null)
                onProgress(apiFile, status, subStatus, pct);
        }

        protected static bool CheckCancelled(FileOpCancelQuery cancelQuery, OnFileOpError onError, VRC.Core.ApiFile apiFile)
        {
            if (apiFile == null)
            {
                return true;
            }

            if (cancelQuery != null && cancelQuery(apiFile))
            {
                if (onError != null)
                    onError(apiFile, "Cancelled by user.");
                return true;
            }

            return false;
        }

        protected static void CleanupTempFiles(string subFolderName)
        {
            MelonCoroutines.Start(Instance.CleanupTempFilesInternal(subFolderName));
        }

        protected IEnumerator CleanupTempFilesInternal(string subFolderName)
        {
            if (!string.IsNullOrEmpty(subFolderName))
            {
                string folder = VRC.Tools.GetTempFolderPath(subFolderName);

                while (Directory.Exists(folder))
                {
                    try
                    {
                        if (Directory.Exists(folder))
                            Directory.Delete(folder, true);
                    }
                    catch (System.Exception)
                    {
                    }

                    yield return null;
                }
            }
        }

        private static void CheckInstance()
        {
            if (mInstance == null)
            {
                GameObject go = new GameObject("ApiFileInstance");
                mInstance = go.AddComponent<ApiFileUtils>();
                config = new RemoteConfig();
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
        }

        private float GetServerProcessingWaitTimeoutForDataSize(int size)
        {
            float timeoutMultiplier = Mathf.Ceil((float)size / (float)SERVER_PROCESSING_WAIT_TIMEOUT_CHUNK_SIZE);
            return Mathf.Clamp(timeoutMultiplier * SERVER_PROCESSING_WAIT_TIMEOUT_PER_CHUNK_SIZE, SERVER_PROCESSING_WAIT_TIMEOUT_PER_CHUNK_SIZE, SERVER_PROCESSING_MAX_WAIT_TIMEOUT);
        }

        private bool uploadFileComponentValidateFileDesc(VRC.Core.ApiFile apiFile, string filename, string md5Base64, long fileSize, VRC.Core.ApiFile.Version.FileDescriptor fileDesc, Action<VRC.Core.ApiFile> onSuccess, Action<string> onError)
        {
            if (fileDesc.status != VRC.Core.ApiFile.Status.Waiting)
            {
                // nothing to do (might be a retry)
                if (onSuccess != null)
                    onSuccess(apiFile);
                return false;
            }

            if (fileSize != fileDesc.sizeInBytes)
            {
                if (onError != null)
                    onError("File size does not match version descriptor");
                return false;
            }
            if (string.Compare(md5Base64, fileDesc.md5) != 0)
            {
                if (onError != null)
                    onError("File MD5 does not match version descriptor");
                return false;
            }

            // make sure file is right size
            long tempSize = 0;
            string errorStr = "";
            if (!VRC.Tools.GetFileSize(filename, out tempSize, out errorStr))
            {
                if (onError != null)
                    onError("Couldn't get file size");
                return false;
            }
            if (tempSize != fileSize)
            {
                if (onError != null)
                    onError("File size does not match input size");
                return false;
            }

            return true;
        }

        private IEnumerator uploadFileComponentDoSimpleUpload(VRC.Core.ApiFile apiFile, VRC.Core.ApiFile.Version.FileDescriptor.Type fileDescriptorType, string filename, string md5Base64, long fileSize, Action<VRC.Core.ApiFile> onSuccess, Action<string> onError, Action<long, long> onProgess, FileOpCancelQuery cancelQuery)
        {
            OnFileOpError onCancelFunc = delegate (VRC.Core.ApiFile file, string s)
            {
                if (onError != null)
                    onError(s);
            };

            string uploadUrl = "";
            while (true)
            {
                bool wait = true;
                string errorStr = "";
                bool worthRetry = false;



                apiFile.StartSimpleUpload(fileDescriptorType, (System.Action<ApiContainer>)delegate (ApiContainer c)
                {
                    uploadUrl = IL2CPP.Il2CppStringToManaged(c.Cast<ApiDictContainer>().ResponseDictionary["url"].Pointer);
                    wait = false;
                }, (System.Action<ApiContainer>)delegate (ApiContainer c)
                {
                    errorStr = "Failed to start upload: " + c.Error;
                    wait = false;
                    if (c.Code == 400)
                    {
                        worthRetry = true;
                    }

                });

                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (onError != null)
                        onError(errorStr);
                    if (!worthRetry)
                        yield break;
                }

                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                if (!worthRetry)
                    break;
            }

            // PUT file
            {
                bool wait = true;
                string errorStr = "";
                HttpRequest req = VRC.Core.ApiFile.PutSimpleFileToURL(uploadUrl, filename, GetMimeTypeFromExtension(Path.GetExtension(filename)), md5Base64, (System.Action)delegate
                {
                    wait = false;
                }, (System.Action<string>)delegate (string error)
                {
                    errorStr = "Failed to upload file: " + error;
                    wait = false;
                }, (System.Action<long, long>)delegate (long uploaded, long length)
                {
                    if (onProgess != null)
                    {
                        onProgess(uploaded, length);
                    }
                });
                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        if (req != null)
                            req.Abort();
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (onError != null)
                        onError(errorStr);
                    yield break;
                }
            }

            // finish upload
            while (true)
            {
                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                bool wait = true;
                string errorStr = "";
                bool worthRetry = false;
                apiFile.FinishUpload(fileDescriptorType, null, (System.Action<ApiContainer>)delegate (ApiContainer c)
                {
                    MelonLogger.LogWarning("YOU CAN IGNORE THE FOLLOWING CASTING ERROR!");
                    apiFile = c.Model.Cast<VRC.Core.ApiFile>();
                }, (System.Action<ApiContainer>)delegate (ApiContainer c)
                {
                    errorStr = "Failed to finish upload: " + c.Error;
                    wait = false;
                    if (c.Code == 400)
                    {
                        worthRetry = false;
                    }
                });
                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    if (onError != null)
                        onError(errorStr);
                    if (!worthRetry)
                        yield break;
                }

                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                if (!worthRetry)
                    break;
            }

        }

        private IEnumerator uploadFileComponentDoMultipartUpload(VRC.Core.ApiFile apiFile, VRC.Core.ApiFile.Version.FileDescriptor.Type fileDescriptorType, string filename, string md5Base64, long fileSize, Action<VRC.Core.ApiFile> onSuccess, Action<string> onError, Action<long, long> onProgess, FileOpCancelQuery cancelQuery)
        {
            FileStream fs = null;
            OnFileOpError onCancelFunc = delegate (VRC.Core.ApiFile file, string s)
            {
                if (fs != null)
                    fs.Close();
                if (onError != null)
                    onError(s);
            };

            // query multipart upload status.
            // we might be resuming a previous upload
            VRC.Core.ApiFile.UploadStatus uploadStatus = null;
            {
                while (true)
                {
                    bool wait = true;
                    string errorStr = "";
                    bool worthRetry = false;
                    apiFile.GetUploadStatus(apiFile.GetLatestVersionNumber(), fileDescriptorType, (System.Action<ApiContainer>)delegate (ApiContainer c)
                    {
                        uploadStatus = c.Model.Cast<VRC.Core.ApiFile.UploadStatus>();
                        wait = false;
                    }, (System.Action<ApiContainer>)delegate (ApiContainer c)
                    {
                        errorStr = "Failed to query multipart upload status: " + c.Error;
                        wait = false;
                        if (c.Code == 400)
                        {
                            worthRetry = true;
                        }
                    });
                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            yield break;
                        }
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        if (onError != null)
                            onError(errorStr);
                        if (!worthRetry)
                            yield break;
                    }

                    if (!worthRetry)
                        break;
                }
            }

            // split file into chunks
            try
            {
                fs = File.OpenRead(filename);
            }
            catch (Exception e)
            {
                if (onError != null)
                    onError("Couldn't open file: " + e.Message);
                yield break;
            }

            byte[] buffer = new byte[kMultipartUploadChunkSize * 2];

            long totalBytesUploaded = 0;
            List<string> etags = new List<string>();
            if (uploadStatus != null)
                etags = uploadStatus.etags.ToArray().ToList();

            int numParts = Mathf.Max(1, Mathf.FloorToInt((float)fs.Length / (float)kMultipartUploadChunkSize));
            for (int partNumber = 1; partNumber <= numParts; partNumber++)
            {
                // read chunk
                int bytesToRead = partNumber < numParts ? kMultipartUploadChunkSize : (int)(fs.Length - fs.Position);
                int bytesRead = 0;
                try
                {
                    bytesRead = fs.Read(buffer, 0, bytesToRead);
                }
                catch (Exception e)
                {
                    fs.Close();
                    if (onError != null)
                        onError("Couldn't read file: " + e.Message);
                    yield break;
                }

                if (bytesRead != bytesToRead)
                {
                    fs.Close();
                    if (onError != null)
                        onError("Couldn't read file: read incorrect number of bytes from stream");
                    yield break;
                }

                // check if this part has been upload already
                // NOTE: uploadStatus.nextPartNumber == number of parts already uploaded
                if (uploadStatus != null && partNumber <= uploadStatus.nextPartNumber)
                {
                    totalBytesUploaded += bytesRead;
                    continue;
                }

                // start upload
                string uploadUrl = "";

                while (true)
                {
                    bool wait = true;
                    string errorStr = "";
                    bool worthRetry = false;
                    apiFile.StartMultipartUpload(fileDescriptorType, partNumber, (System.Action<ApiContainer>)delegate (ApiContainer c)
                    {
                        uploadUrl = IL2CPP.Il2CppStringToManaged(c.Cast<ApiDictContainer>().ResponseDictionary["url"].Pointer);
                        wait = false;
                    }, (System.Action<ApiContainer>)delegate (ApiContainer c)
                    {
                        errorStr = "Failed to start part upload: " + c.Error;
                        wait = false;
                    });
                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            yield break;
                        }
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        fs.Close();
                        if (onError != null)
                            onError(errorStr);
                        if (!worthRetry)
                            yield break;
                    }

                    // delay to let write get through servers
                    yield return new WaitForSecondsRealtime(kPostWriteDelay);

                    if (!worthRetry)
                        break;
                }

                // PUT file part
                {
                    bool wait = true;
                    string errorStr = "";

                    VRC.HttpRequest req = VRC.Core.ApiFile.PutMultipartDataToURL(uploadUrl, buffer, bytesRead, GetMimeTypeFromExtension(Path.GetExtension(filename)), (System.Action<string>)delegate (string etag)
                    {
                        if (!string.IsNullOrEmpty(etag))
                        {
                            etags.Add(etag);
                        }
                        totalBytesUploaded += bytesRead;
                        wait = false;
                    }, (System.Action<string>)delegate (string error)
                 {
                     errorStr = "Failed to upload data: " + error;
                     wait = false;
                 }, (System.Action<long, long>)delegate (long uploaded, long length)
                 {
                     if (onProgess != null)
                     {
                        onProgess(totalBytesUploaded + uploaded, fileSize);
                     }
                 });
            while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            if (req != null)
                                req.Abort();
                            yield break;
                        }
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        fs.Close();
                        if (onError != null)
                            onError(errorStr);
                        yield break;
                    }
                }
            }

            // finish upload
            while (true)
            {
                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                bool wait = true;
                string errorStr = "";
                bool worthRetry = false;

                Il2CppSystem.Collections.Generic.List<string> _etags = new Il2CppSystem.Collections.Generic.List<string>();

                foreach (string val in etags)
                    _etags.Add(val);

                apiFile.FinishUpload(fileDescriptorType, _etags, (System.Action<ApiContainer>)delegate (ApiContainer c)
                {
                    MelonLogger.LogWarning("YOU CAN IGNORE THE FOLLOWING CASTING ERROR!");
                    apiFile = c.Model.Cast<VRC.Core.ApiFile>();
                    wait = false;
                }, (System.Action<ApiContainer>)delegate (ApiContainer c)
                {
                    errorStr = "Failed to finish upload: " + c.Error;
                    wait = false;
                    if (c.Code == 400)
                    {
                        worthRetry = true;
                    }
                });
                while (wait)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorStr))
                {
                    fs.Close();
                    if (onError != null)
                        onError(errorStr);
                    if (!worthRetry)
                        yield break;
                }

                // delay to let write get through servers
                yield return new WaitForSecondsRealtime(kPostWriteDelay);

                if (!worthRetry)
                    break;
            }

            fs.Close();
        }

        private IEnumerator uploadFileComponentVerifyRecord(VRC.Core.ApiFile apiFile, VRC.Core.ApiFile.Version.FileDescriptor.Type fileDescriptorType, string filename, string md5Base64, long fileSize, VRC.Core.ApiFile.Version.FileDescriptor fileDesc, Action<VRC.Core.ApiFile> onSuccess, Action<string> onError, Action<long, long> onProgess, FileOpCancelQuery cancelQuery)
        {
            OnFileOpError onCancelFunc = delegate (VRC.Core.ApiFile file, string s)
            {
                if (onError != null)
                    onError(s);
            };

            float initialStartTime = Time.realtimeSinceStartup;
            float startTime = initialStartTime;
            float timeout = GetServerProcessingWaitTimeoutForDataSize(fileDesc.sizeInBytes);
            float waitDelay = SERVER_PROCESSING_INITIAL_RETRY_TIME;
            float maxDelay = SERVER_PROCESSING_MAX_RETRY_TIME;

            while (true)
            {
                if (apiFile == null)
                {
                    if (onError != null)
                        onError("ApiFile is null");
                    yield break;
                }

                var desc = apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), fileDescriptorType);
                if (desc == null)
                {
                    if (onError != null)
                        onError("File descriptor is null ('" + fileDescriptorType + "')");
                    yield break;
                }

                if (desc.status != VRC.Core.ApiFile.Status.Waiting)
                {
                    // upload completed or is processing
                    break;
                }

                // wait for next poll
                while (Time.realtimeSinceStartup - startTime < waitDelay)
                {
                    if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                    {
                        yield break;
                    }

                    if (Time.realtimeSinceStartup - initialStartTime > timeout)
                    {
                        if (onError != null)
                            onError("Couldn't verify upload status: Timed out wait for server processing");
                        yield break;
                    }

                    yield return null;
                }


                while (true)
                {
                    bool wait = true;
                    string errorStr = "";
                    bool worthRetry = false;
                    apiFile.Refresh((System.Action<ApiContainer>)delegate
                    {
                        wait = false;
                    }, (System.Action<ApiContainer>)delegate (ApiContainer c)
                    {
                        errorStr = "Couldn't verify upload status: " + c.Error;
                        wait = false;
                        if (c.Code == 400)
                        {
                            worthRetry = true;
                        }
                    });
                    while (wait)
                    {
                        if (CheckCancelled(cancelQuery, onCancelFunc, apiFile))
                        {
                            yield break;
                        }

                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(errorStr))
                    {
                        if (onError != null)
                            onError(errorStr);
                        if (!worthRetry)
                            yield break;
                    }

                    if (!worthRetry)
                        break;
                }

                waitDelay = Mathf.Min(waitDelay * 2, maxDelay);
                startTime = Time.realtimeSinceStartup;
            }

            if (onSuccess != null)
                onSuccess(apiFile);
        }

        private IEnumerator UploadFileComponentInternal(VRC.Core.ApiFile apiFile, VRC.Core.ApiFile.Version.FileDescriptor.Type fileDescriptorType, string filename, string md5Base64, long fileSize, Action<VRC.Core.ApiFile> onSuccess, Action<string> onError, Action<long, long> onProgess, FileOpCancelQuery cancelQuery)
        {
            VRC.Core.ApiFile.Version.FileDescriptor fileDesc = apiFile.GetFileDescriptor(apiFile.GetLatestVersionNumber(), fileDescriptorType);

            if (!uploadFileComponentValidateFileDesc(apiFile, filename, md5Base64, fileSize, fileDesc, onSuccess, onError))
                yield break;

            switch (fileDesc.category)
            {
                case VRC.Core.ApiFile.Category.Simple:
                    yield return uploadFileComponentDoSimpleUpload(apiFile, fileDescriptorType, filename, md5Base64, fileSize, onSuccess, onError, onProgess, cancelQuery);
                    break;
                case VRC.Core.ApiFile.Category.Multipart:
                    yield return uploadFileComponentDoMultipartUpload(apiFile, fileDescriptorType, filename, md5Base64, fileSize, onSuccess, onError, onProgess, cancelQuery);
                    break;
                default:
                    if (onError != null)
                        onError("Unknown file category type: " + fileDesc.category);
                    yield break;
            }

            yield return uploadFileComponentVerifyRecord(apiFile, fileDescriptorType, filename, md5Base64, fileSize, fileDesc, onSuccess, onError, onProgess, cancelQuery);
        }
    }
}
