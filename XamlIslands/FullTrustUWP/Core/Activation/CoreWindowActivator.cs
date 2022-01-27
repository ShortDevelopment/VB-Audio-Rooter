﻿using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using static FullTrustUWP.Core.InteropHelper;

namespace FullTrustUWP.Core.Activation
{
    public static class CoreWindowActivator
    {
        public enum WindowType : long
        {
            IMMERSIVE_DOCK = 1,
            IMMERSIVE_HOSTED,
            IMMERSIVE_BODY_ACTIVE = 4,
            IMMERSIVE_DOCK_ACTIVE,
            NOT_IMMERSIVE
        }

        private delegate int PrivateCreateCoreWindow_Sig(WindowType windowType, [MarshalAs(UnmanagedType.BStr)] string windowTitle, int x, int y, uint width, uint height, uint dwAttributes, IntPtr hOwnerWindow, Guid riid, out ICoreWindowInterop windowRef);
        private static PrivateCreateCoreWindow_Sig PrivateCreateCoreWindow_Ref;

        public static CoreWindow CreateCoreWindow(WindowType windowType, string windowTitle, IntPtr hOwnerWindow, int x = 0, int y = 0, uint width = 10, uint height = 10, uint dwAttributes = 0)
        {
            if (PrivateCreateCoreWindow_Ref == null)
                PrivateCreateCoreWindow_Ref = DynamicLoad<PrivateCreateCoreWindow_Sig>("windows.ui.dll", 0x5dc);

            Marshal.ThrowExceptionForHR(PrivateCreateCoreWindow_Ref(windowType, windowTitle, x, y, width, height, dwAttributes, hOwnerWindow, typeof(ICoreWindowInterop).GUID, out ICoreWindowInterop windowRef));
            return windowRef as object as CoreWindow;
        }

        private delegate int CreateCoreApplicationViewTitleBar_Sig(CoreWindow titleBarClientAdapter, IntPtr hWnd, out CoreApplicationViewTitleBar titleBar);
        private static CreateCoreApplicationViewTitleBar_Sig CreateCoreApplicationViewTitleBar_Ref;

        public static CoreApplicationViewTitleBar CreateCoreApplicationViewTitleBar(CoreWindow coreWindow, IntPtr hWnd)
        {
            if (CreateCoreApplicationViewTitleBar_Ref == null)
                CreateCoreApplicationViewTitleBar_Ref = DynamicLoad<CreateCoreApplicationViewTitleBar_Sig>("twinapi.appcore.dll", 501);

            Marshal.ThrowExceptionForHR(CreateCoreApplicationViewTitleBar_Ref(coreWindow, hWnd, out var titleBar));
            return titleBar;
        }

        [DllImport("twinapi.appcore.dll", SetLastError = true)]
        private static extern int CreateCoreApplicationViewTitleBar(CoreWindow titleBarClientAdapter, IntPtr hWnd, out CoreApplicationViewTitleBar titleBar);

        [DllImport("twinapi.appcore", SetLastError = true)]
        private static extern int CreateApplicationViewTitleBar(AppWindow titleBarClientAdapter, IntPtr hWnd, out ApplicationViewTitleBar titleBar);
    }

    [ComImport, Guid(Const.IID_ICoreWindowInterop)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICoreWindowInterop
    {
        IntPtr WindowHandle { get; }
        bool MessageHandled { get; }
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    [Guid("CD292360-2763-4085-8A9F-74B224A29175")]
    public interface ICoreWindowFactory
    {
        void CreateCoreWindow([MarshalAs(UnmanagedType.HString)] string windowTitle, out CoreWindow window);
        bool WindowReuseAllowed { get; }
    }

    public sealed class CoreWindowFactory : ICoreWindowFactory
    {
        public bool WindowReuseAllowed => true;

        public void CreateCoreWindow([MarshalAs(UnmanagedType.HString)] string windowTitle, out CoreWindow window)
        {
            window = CoreWindowActivator.CreateCoreWindow(
                CoreWindowActivator.WindowType.IMMERSIVE_BODY_ACTIVE,
                windowTitle,
                IntPtr.Zero,
                width: 1024,
                height: 768
            );
        }
    }
}
