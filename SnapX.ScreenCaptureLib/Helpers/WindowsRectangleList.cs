
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ShareX.ScreenCaptureLib
{
    public class WindowsRectangleList
    {
        public IntPtr IgnoreHandle { get; set; }
        public bool IncludeChildWindows { get; set; }
        public int Timeout { get; set; }

        private List<SimpleWindowInfo> windows;
        private HashSet<IntPtr> parentHandles;
        private CancellationTokenSource cts;

        public List<SimpleWindowInfo> GetWindowInfoList()
        {
            windows = new List<SimpleWindowInfo>();
            parentHandles = new HashSet<IntPtr>();

            try
            {
                if (Timeout > 0)
                {
                    cts = new CancellationTokenSource();
                    cts.CancelAfter(Timeout);
                }

                EnumWindowsProc ewp = EvalWindow;
                NativeMethods.EnumWindows(ewp, IntPtr.Zero);
            }
            catch
            {
            }
            finally
            {
                cts?.Dispose();
            }

            List<SimpleWindowInfo> result = new List<SimpleWindowInfo>();

            foreach (SimpleWindowInfo window in windows)
            {
                bool rectVisible = true;

                if (!window.IsWindow)
                {
                    foreach (SimpleWindowInfo window2 in result)
                    {
                        if (window2.Rectangle.Contains(window.Rectangle))
                        {
                            rectVisible = false;
                            break;
                        }
                    }
                }

                if (rectVisible)
                {
                    result.Add(window);
                }
            }

            return result;
        }

        private bool EvalWindow(IntPtr hWnd, IntPtr lParam)
        {
            return CheckHandle(hWnd, true);
        }

        private bool EvalControl(IntPtr hWnd, IntPtr lParam)
        {
            return CheckHandle(hWnd, false);
        }

        private bool CheckHandle(IntPtr handle, bool isWindow)
        {
            if (cts != null && cts.IsCancellationRequested)
            {
                return false;
            }

            if (handle == IgnoreHandle || !NativeMethods.IsWindowVisible(handle) || (isWindow && NativeMethods.IsWindowCloaked(handle)))
            {
                return true;
            }

            SimpleWindowInfo windowInfo = new SimpleWindowInfo(handle);

            if (isWindow)
            {
                windowInfo.IsWindow = true;
                windowInfo.Rectangle = CaptureHelpers.GetWindowRectangle(handle);
            }
            else
            {
                windowInfo.Rectangle = NativeMethods.GetWindowRect(handle);
            }

            if (!windowInfo.Rectangle.IsValid())
            {
                return true;
            }

            if (IncludeChildWindows && !parentHandles.Contains(handle))
            {
                parentHandles.Add(handle);

                EnumWindowsProc ewp = EvalControl;
                NativeMethods.EnumChildWindows(handle, ewp, IntPtr.Zero);
            }

            if (isWindow)
            {
                Rectangle clientRect = NativeMethods.GetClientRect(handle);

                if (clientRect.IsValid() && clientRect != windowInfo.Rectangle)
                {
                    windows.Add(new SimpleWindowInfo(handle, clientRect));
                }
            }

            windows.Add(windowInfo);

            return true;
        }
    }
}