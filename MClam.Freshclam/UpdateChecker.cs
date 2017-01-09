﻿using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using MClam.Native;
using MClam.Shared;
using MClam.Sigtool;

namespace MClam.Freshclam
{
    /// <summary>
    /// Database update checker, only for CVDs.
    /// </summary>
    public class UpdateChecker
    {
        /// <summary>
        /// Checks for database update from specified local database and remote database.
        /// </summary>
        /// <param name="cvdPath">Full path to local CVD file.</param>
        /// <param name="url">URI path to remote CVD file.</param>
        /// <returns><c>True</c> when update is availiable, otherwise <c>False</c>.</returns>
        public bool HasUpdate(string cvdPath, string url)
        {
            ArgValidate.NotEmptyString(cvdPath, nameof(cvdPath));
            ArgValidate.FileExist(cvdPath, nameof(cvdPath));

            var remoteTime = GetRemoteVersion(url);
            var local = SigtoolMain.GetCvdMetadata(cvdPath);

            return remoteTime > local.Version;
        }

        /// <summary>
        /// Gets remote CVD file version.
        /// </summary>
        /// <param name="url">URI path to remote CVD file.</param>
        /// <returns>Version number.</returns>
        public uint GetRemoteVersion(string url)
        {
            ArgValidate.NotEmptyString(url, nameof(url));

            var head = GetRemoteResponse(url);
            var meta = NativeMethods.cl_cvdparse(head);
            if (meta == IntPtr.Zero) throw new Exception("Malformed CVD header.");

            var data = (cl_cvd)Marshal.PtrToStructure(meta, typeof(cl_cvd));
            NativeMethods.cl_cvdfree(meta);

            return data.version;
        }


        private string GetRemoteResponse(string url)
        {
            var req = (HttpWebRequest) WebRequest.Create(url);
            req.Connection = "close";
            req.AddRange("bytes", 0, 511);

            var resp = (HttpWebResponse) req.GetResponse();
            var buff = new byte[resp.ContentLength + 3];

            using (var returnStream = resp.GetResponseStream())
            {
                if (returnStream == null) throw new Exception("Empty response!");
                returnStream.Read(buff, 0, buff.Length);
            }

            return Encoding.UTF8.GetString(buff);
        }
    }
}
