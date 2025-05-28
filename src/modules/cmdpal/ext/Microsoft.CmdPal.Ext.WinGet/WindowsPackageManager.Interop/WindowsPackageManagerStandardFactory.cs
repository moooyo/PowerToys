// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.CmdPal.Ext.WinGet.Helpers;

namespace WindowsPackageManager.Interop;

public class WindowsPackageManagerStandardFactory : WindowsPackageManagerFactory
{
    public WindowsPackageManagerStandardFactory(ClsidContext clsidContext = ClsidContext.Prod)
        : base(clsidContext)
    {
    }

    protected override T CreateInstance<T>(Guid clsid, Guid iid)
    {
        var pUnknown = IntPtr.Zero;
        try
        {
            var cw = new StrategyBasedComWrappers();
            const int clsctxAll = 0x17;

            var hr = Native.CoCreateInstance(clsid, IntPtr.Zero, clsctxAll, iid, out pUnknown);
            Marshal.ThrowExceptionForHR((int)hr);
            var instance = (T)cw.GetOrCreateObjectForComInstance(pUnknown, CreateObjectFlags.None);

            if (instance == null)
            {
                throw new ArgumentException("Failed to get IApplicationActivationManager interface");
            }

            return instance;
        }
        finally
        {
            // CoCreateInstance and FromAbi both AddRef on the native object.
            // Release once to prevent memory leak.
            if (pUnknown != IntPtr.Zero)
            {
                Marshal.Release(pUnknown);
            }
        }
    }
}
