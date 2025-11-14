// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using UnityEngine;

namespace Mediapipe.Unity
{
    public static class AsyncGlContext
    {
        public static AsyncGlContextRequest Request(Action<AsyncGlContextRequest> callback)
        {
            return new AsyncGlContextRequest(callback);
        }
    }

    public class AsyncGlContextRequest
    {
        private static int _Counter;
        private static readonly GlobalInstanceTable<int, AsyncGlContextRequest> _InstanceTable = new(5);
        private readonly Action<AsyncGlContextRequest> _callback;

        private readonly int _id;

        internal AsyncGlContextRequest(Action<AsyncGlContextRequest> callback)
        {
            _id = Interlocked.Increment(ref _Counter);
            _callback = callback;
            _InstanceTable.Add(_id, this);

            GLEventCallback gLEventCallback = PluginCallback;
            var fp = Marshal.GetFunctionPointerForDelegate(gLEventCallback);

            GL.IssuePluginEvent(fp, _id);
        }

        public IntPtr platformGlContext { get; private set; }
        public bool done { get; private set; }
        public Exception error { get; private set; }

        [MonoPInvokeCallback(typeof(GLEventCallback))]
        private static void PluginCallback(int eventId)
        {
            if (!_InstanceTable.TryGetValue(eventId, out var request))
            {
                Logger.LogWarning($"AsyncGlContextRequest with id {eventId} is not found, maybe already GCed");
                return;
            }

            try
            {
#if UNITY_ANDROID
        // Currently, it works only on Android
        request.platformGlContext = Egl.GetCurrentContext();
#endif

                request._callback?.Invoke(request);
            }
            catch (Exception e)
            {
                request.error = e;
            }
            finally
            {
                request.done = true;
                _InstanceTable.Remove(eventId);
            }
        }

        private delegate void GLEventCallback(int eventId);
    }
}