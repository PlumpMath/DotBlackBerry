﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

using Mono.Unix.Native;

namespace BlackBerry.BPS
{
    /// <summary>
    /// Structure that represents the payload of an event.
    /// </summary>
    [AvailableSince(10, 0)]
    public struct BPSEventPayload
    {
        /// <summary>
        /// Create a new instance of a event payload.
        /// </summary>
        /// <param name="d1">Payload data.</param>
        /// <param name="d2">Payload data.</param>
        /// <param name="d3">Payload data.</param>
        [AvailableSince(10, 0)]
        public BPSEventPayload(object d1 = null, object d2 = null, object d3 = null)
            : this()
        {
            Data1 = d1;
            Data2 = d2;
            Data3 = d3;
        }

        /// <summary>
        /// Get payload data.
        /// </summary>
        [AvailableSince(10, 0)]
        public object Data1 { get; private set; }

        /// <summary>
        /// Get payload data.
        /// </summary>
        [AvailableSince(10, 0)]
        public object Data2 { get; private set; }

        /// <summary>
        /// Get payload data.
        /// </summary>
        [AvailableSince(10, 0)]
        public object Data3 { get; private set; }

#if !ObjectPointersPinnedByDefault

        /// <summary>
        /// Data should be accessible to native code. Otherwise only calls to this library will be able to understand them.
        /// </summary>
        public bool DataIsAccessibleToNative { get; set; }

#endif

        #region Native Conversion

        private struct Payload
        {
            public IntPtr data1;
            public IntPtr data2;
            public IntPtr data3;
        }

        internal BPSEventPayload(IntPtr ptr)
            : this()
        {
            if (ptr != IntPtr.Zero)
            {
                var payload = new Payload();
                Marshal.PtrToStructure(ptr, payload);
                Data1 = Util.DeserializeFromPointer(payload.data1);
                Data2 = Util.DeserializeFromPointer(payload.data2);
                Data3 = Util.DeserializeFromPointer(payload.data3);
            }
        }

        internal IntPtr GetDataPointer()
        {
            if (Data1 == null && Data2 == null && Data3 == null)
            {
                return IntPtr.Zero;
            }

            var payload = new Payload();
#if !ObjectPointersPinnedByDefault
            if (!DataIsAccessibleToNative)
#endif
            {
                payload.data1 = Util.SerializeToPointer(Data1);
                payload.data2 = Util.SerializeToPointer(Data2);
                payload.data3 = Util.SerializeToPointer(Data3);
            }
#if !ObjectPointersPinnedByDefault
            else
            {
                payload.data1 = Util.SerializeToPointer(Data1, GCHandleType.Pinned);
                payload.data2 = Util.SerializeToPointer(Data2, GCHandleType.Pinned);
                payload.data3 = Util.SerializeToPointer(Data3, GCHandleType.Pinned);
            }
#endif

            var result = Stdlib.malloc((ulong)Marshal.SizeOf(payload));
            if (result == IntPtr.Zero)
            {
                Util.FreeSerializePointer(payload.data1);
                Util.FreeSerializePointer(payload.data2);
                Util.FreeSerializePointer(payload.data3);
                return IntPtr.Zero;
            }
            Marshal.StructureToPtr(payload, result, false);
            BPS.RegisterSerializedPointer(payload.data1);
            BPS.RegisterSerializedPointer(payload.data2);
            BPS.RegisterSerializedPointer(payload.data3);
            return result;
        }

        internal static void FreeDataPointer(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }
            var payload = new Payload();
            Marshal.PtrToStructure(ptr, payload);
            BPS.UnregisterSerializedPointer(payload.data1);
            BPS.UnregisterSerializedPointer(payload.data2);
            BPS.UnregisterSerializedPointer(payload.data3);
            Util.FreeSerializePointer(payload.data1);
            Util.FreeSerializePointer(payload.data2);
            Util.FreeSerializePointer(payload.data3);
        }

        #endregion
    }

    /// <summary>
    /// BPS-based events.
    /// </summary>
    [AvailableSince(10, 0)]
    public class BPSEvent : IDisposable
    {
        #region PInvoke

        [DllImport(BPS.BPS_LIBRARY)]
        private static extern void bps_event_destroy(IntPtr ev);

        [DllImport(BPS.BPS_LIBRARY)]
        private static extern int bps_event_get_domain(IntPtr ev);

        [DllImport(BPS.BPS_LIBRARY)]
        private static extern uint bps_event_get_code(IntPtr ev);

        [DllImport(BPS.BPS_LIBRARY)]
        private static extern IntPtr bps_event_get_payload(IntPtr ev);

        [DllImport(BPS.BPS_LIBRARY)]
        private static extern int bps_event_create(out IntPtr ev, uint domain, uint code, IntPtr payload_ptr, Action<IntPtr> completion_function);

        #endregion

        /// <summary>
        /// The maximum allowable domain of an event that you create using.
        /// </summary>
        [AvailableSince(10, 0)]
        public const int BPS_EVENT_DOMAIN_MAX = 0x00000FFF;

        private static IDictionary<IntPtr, Action<BPSEvent>> handleToCallback = new ConcurrentDictionary<IntPtr, Action<BPSEvent>>();

        private IntPtr handle;
        private CancellationToken token;

        internal BPSEvent(IntPtr hwnd, CancellationToken token, bool disposable = false)
            : this(hwnd, disposable)
        {
            this.token = token;
        }

        internal BPSEvent(IntPtr hwnd, bool disposable = false)
        {
            handle = hwnd;
            token = CancellationToken.None;
            IsDisposable = disposable;
        }

        internal BPSEvent(BPSEvent baseEvent)
        {
            handle = baseEvent.handle;
            token = baseEvent.token;
            IsDisposable = baseEvent.IsDisposable;
        }

        /// <summary>
        /// Finalize BPSEvent instance.
        /// </summary>
        ~BPSEvent()
        {
            if (IsDisposable)
            {
                Dispose(false);
            }
        }

        /// <summary>
        /// Create an event.
        /// </summary>
        /// <param name="domain">The domain of the event.</param>
        /// <param name="code">The code of the event.</param>
        [AvailableSince(10, 0)]
        public BPSEvent(int domain, uint code)
            : this(domain, code, new BPSEventPayload(), null, false)
        {
        }

        /// <summary>
        /// Create an event.
        /// </summary>
        /// <param name="domain">The domain of the event.</param>
        /// <param name="code">The code of the event.</param>
        /// <param name="payload">The event's payload.</param>
        [AvailableSince(10, 0)]
        public BPSEvent(int domain, uint code, BPSEventPayload payload)
            : this(domain, code, payload, null, false)
        {
        }

        /// <summary>
        /// Create an event.
        /// </summary>
        /// <param name="domain">The domain of the event.</param>
        /// <param name="code">The code of the event.</param>
        /// <param name="payload">The event's payload.</param>
        /// <param name="completionFunction">An optional completion function that will be invoked when the system is done with the event.</param>
        [AvailableSince(10, 0)]
        public BPSEvent(int domain, uint code, BPSEventPayload payload, Action<BPSEvent> completionFunction)
            : this(domain, code, payload, completionFunction, true)
        {
        }

        private BPSEvent(int domain, uint code, BPSEventPayload payload, Action<BPSEvent> completionFunction, bool recordCompletion)
            : this(IntPtr.Zero, true)
        {
            if (domain < 0 || domain > BPS_EVENT_DOMAIN_MAX)
            {
                throw new ArgumentOutOfRangeException("domain", domain, "0 <= domain < BPS_EVENT_DOMAIN_MAX");
            }
            if (code > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("code", code, "0 <= code < UInt16.MaxValue");
            }
            Util.GetBPSOrException();
            var payloadPtr = payload.GetDataPointer();
            var success = bps_event_create(out handle, (uint)domain, code, payloadPtr, EventCompletion) != BPS.BPS_SUCCESS;
            if (payloadPtr != IntPtr.Zero)
            {
                if (!success)
                {
                    BPSEventPayload.FreeDataPointer(payloadPtr);
                }
                Stdlib.free(payloadPtr);
            }
            if (success)
            {
                if (recordCompletion)
                {
                    handleToCallback.Add(handle, completionFunction);
                }
            }
            else
            {
                Util.ThrowExceptionForLastErrno();
            }
            IsDisposable = true;
        }

        private static void EventCompletion(IntPtr ptr)
        {
            if (handleToCallback.ContainsKey(ptr))
            {
                var ev = new BPSEvent(ptr, false);
                var callback = handleToCallback[ptr];
                handleToCallback.Remove(ptr);
                try
                {
                    callback(ev);
                }
                catch
                {
                }
            }
            BPSEventPayload.FreeDataPointer(bps_event_get_payload(ptr));
        }

        private void CheckState()
        {
            if (token.IsCancellationRequested)
            {
                handle = IntPtr.Zero;
            }
            if (handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException("BPSEvent");
            }
        }

        /// <summary>
        /// Get if the event is disposable.
        /// </summary>
        public bool IsDisposable { get; private set; }

        /// <summary>
        /// Get if the event is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return handle != IntPtr.Zero && !token.IsCancellationRequested;
            }
        }

        /// <summary>
        /// Get the code of an event.
        /// </summary>
        [AvailableSince(10, 0)]
        public uint Code
        {
            [AvailableSince(10, 0)]
            get
            {
                CheckState();
                return bps_event_get_code(handle);
            }
        }

        /// <summary>
        /// Get the domain of an event.
        /// </summary>
        [AvailableSince(10, 0)]
        public int Domain
        {
            [AvailableSince(10, 0)]
            get
            {
                CheckState();
                return bps_event_get_domain(handle);
            }
        }

        /// <summary>
        /// Get the payload of an event.
        /// </summary>
        [AvailableSince(10, 0)]
        public BPSEventPayload Payload
        {
            [AvailableSince(10, 0)]
            get
            {
                CheckState();
                return new BPSEventPayload(bps_event_get_payload(handle));
            }
        }

        /// <summary>
        /// Get the internal handle of the BPS event.
        /// </summary>
        /// <returns>The internal handle of the BPS event.</returns>
        internal IntPtr DangerousGetHandle()
        {
            CheckState();
            return handle;
        }

        /// <summary>
        /// Dispose of the BPS event.
        /// </summary>
        [AvailableSince(10, 0)]
        public void Dispose()
        {
            if (!IsDisposable)
            {
                throw new InvalidOperationException("BPSEvent cannot be disposed directly");
            }
            if (handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException("BPSEvent");
            }
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the BPSEvent instance.
        /// </summary>
        /// <param name="disposing">true if Dispose was called, false if the finalizer was triggered.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (handle != IntPtr.Zero)
            {
                bps_event_destroy(handle);
                handle = IntPtr.Zero;
                token = CancellationToken.None;
            }
        }
    }
}
