﻿using System;
using System.Runtime.InteropServices;
using MClam.Native;
using MClam.Shared;

namespace MClam
{
    /// <summary>
    /// Provides database changes check.
    /// </summary>
    public sealed class DatabaseStateChecker : IDisposable
    {
        private IntPtr _stat;
        private readonly string _dirPath;

        #region Constructor
        /// <summary>
        /// Initialize new instance of <see cref="DatabaseStateChecker"/> using specified directory to watch.
        /// </summary>
        /// <param name="dirPath">Full path to database directory.</param>
        public DatabaseStateChecker(string dirPath)
        {
            ArgValidate.NotEmptyString(dirPath, nameof(dirPath));
            ArgValidate.DirectoryExist(dirPath, nameof(dirPath));

            _dirPath = dirPath;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating a state has been captured.
        /// </summary>
        public bool IsStateCaptured => _stat != IntPtr.Zero;

        /// <summary>
        /// Gets a value indicating directory to watch.
        /// </summary>
        public string WatchDirectory => _dirPath;
        #endregion

        #region Methods
        /// <summary>
        /// Capture current database file(s) state.
        /// </summary>
        public void CaptureState()
        {
            _stat = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(cl_stat)));
            NativeMethods.cl_statinidir(_dirPath, _stat);
        }

        /// <summary>
        /// Checks for state changes on directory.
        /// </summary>
        /// <returns></returns>
        public bool HasChanges()
        {
            return NativeMethods.cl_statchkdir(_stat) == 1;
        }

        /// <summary>
        /// Release old capture state and capture a fresh one.
        /// </summary>
        public void Recapture()
        {
            ReleaseCapture();
            CaptureState();
        }
        
        /// <summary>
        /// Releases current capture state.
        /// </summary>
        public void ReleaseCapture()
        {
            if (_stat == IntPtr.Zero) return;

            NativeMethods.cl_statfree(_stat);
            Marshal.FreeHGlobal(_stat);
            _stat = IntPtr.Zero;
        }
        #endregion

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
            }

            ReleaseCapture();

            _disposedValue = true;
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~DatabaseStateChecker()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
