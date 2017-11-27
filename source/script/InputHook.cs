using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace TCO
{
    using static InputHookEnum;
    /// <summary>
    ///     Creates Global listeners for keyboard and mouse input. 
    ///     Run InputHook.SetHooks() before Application.Run() and 
    ///     InputHook.ReleaseHooks() before closing.
    /// </summary>
    public static class InputHook
    {
        //////////////////////////////////////
        //    Values and Methods for use    //
        //////////////////////////////////////
        
        /// <summary>
        /// is true if user activity was detected
        /// </summary>
        private static bool m_activityDetected = false;
        private static bool m_initialized = false;

        /// <summary>
        /// Returns true if user activity was detected. Sets value to false after reading.
        /// </summary>
        /// <returns></returns>
        public static bool getActivityDetected()
        {
            bool result = m_activityDetected;
            m_activityDetected = false;
            return result;
        }

        public static void Start()
        {
            SetHooks();
        }

        public static void End()
        {
            ReleaseHooks();
        }

        //////////////////////////////////////
        //    Setup and Shutdown methods    //
        //////////////////////////////////////

        /// <summary>
        /// Sets all the hooks defined within the InputHook class.
        /// </summary>
        private static void SetHooks()
        {
            if (m_initialized == true)
                return;

            m_initialized = true;

            // set input hooks
            _hookIDKey = RegisterHook(_procKey, HookId.KeyboardLowLevel);
            _hookIDMouse = RegisterHook(_procMouse, HookId.MouseLowLevel);
        }

        /// <summary>
        /// Releases all the hooks defined within the InputHook class.
        /// </summary>
        private static void ReleaseHooks()
        {
            UnhookWindowsHookEx(_hookIDKey);
            UnhookWindowsHookEx(_hookIDMouse);

            m_initialized = false;
        }

        //////////////////////////////////////
        //   Methods run on hook callback   //
        //////////////////////////////////////

        /// <summary>
        /// Method that runs when the keyboard hook is called.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr HookCallbackKey(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)HookAction.WM_KEYDOWN)
            {
                m_activityDetected = true;
                //int vkCode = Marshal.ReadInt32(lParam);
                //Console.WriteLine("keyboard");
            }
            return CallNextHookEx(_hookIDKey, nCode, wParam, lParam);
        }

        /// <summary>
        /// Method that runs when the mouse input hook is called.
        /// </summary>
        /// <param name="nCode">Data about how to process the hook information.</param>
        /// <param name="wParam">Data passed to the hook precedure. The data is hook-type specific</param>
        /// <param name="lParam">Data passed to the hook precedure. The data is hook-type specific</param>
        /// <returns></returns>
        private static IntPtr HookCallbackMouse(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 &&
                (wParam == (IntPtr)HookAction.WM_LBUTTONDOWN ||
                wParam == (IntPtr)HookAction.WM_RBUTTONDOWN ||
                wParam == (IntPtr)HookAction.WM_MOUSEWHEEL ||
                wParam == (IntPtr)HookAction.WM_XBUTTONDOWN ||
                wParam == (IntPtr)HookAction.WM_MOUSEHWHEEL ||
                wParam == (IntPtr)HookAction.WM_MOUSEMOVE))
            {

                m_activityDetected = true;
                //Console.WriteLine("mouse");
            }
            return CallNextHookEx(_hookIDMouse, nCode, wParam, lParam);
        }

        //////////////////////////////////////
        //   Internal Values and Methods    //
        //////////////////////////////////////

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // references the functions  that will be 
        // run for the different input hook types
        private static LowLevelKeyboardProc _procKey = HookCallbackKey;
        private static LowLevelKeyboardProc _procMouse = HookCallbackMouse;

        // used to tell windows which input the hook calls
        private static IntPtr _hookIDKey = IntPtr.Zero;
        private static IntPtr _hookIDMouse = IntPtr.Zero;

        private static IntPtr RegisterHook(LowLevelKeyboardProc proc, HookId hookType)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx((int)hookType, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        
        //////////////////////////////////////
        //   Imported Values and Methods    //
        //////////////////////////////////////

        /// <summary>
        /// Sets a hook for the Windows API.
        /// </summary>
        /// <param name="idHook">The type of hook to be set.</param>
        /// <param name="lpfn">The hook procedure. Pass the delegate of the callback function.</param>
        /// <param name="hMod">The handle of the module containing the hook procedure. Normally the one program the hook is set from</param>
        /// <param name="dwThreadId">The thread that the hook procedure is associated with. [0 = all]</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// Releases a hook for the Windows API.
        /// </summary>
        /// <param name="hhk">The id of the hook to be release. This is the pointer that was returned from SetWindowsHookEx(..) when the hook was created.</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// Pass the information to the next hook in the chain.
        /// </summary>
        /// <param name="hhk">The if of the hook that was called</param>
        /// <param name="nCode">Data about how to process the hook information.</param>
        /// <param name="wParam">Data passed to the hook precedure. The data is hook-type specific</param>
        /// <param name="lParam">Data passed to the hook precedure. The data is hook-type specific</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Gets the handle to the modlue with the specified name.
        /// </summary>
        /// <param name="lpModuleName">The name of the module to get the handle for</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
