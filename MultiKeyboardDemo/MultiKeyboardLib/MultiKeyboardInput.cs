using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using static MultiKeyboardLib.Win32;

namespace MultiKeyboardLib
{
    public class MultiKeyboardInput
    {
        private HwndSource _source;
        private Action<IntPtr, Win32.VK> _callbackRawInput;

        public MultiKeyboardInput(HwndSource source, Action<IntPtr, Win32.VK> callbackRawInput)
        {
            _source = source;
            _callbackRawInput = callbackRawInput;

            RegisterForRawInputDevices();    

            // add a window WndPro hook
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private void RegisterForRawInputDevices()
        {
            // register for raw input
            RAWINPUTDEVICE[] dev = new RAWINPUTDEVICE[1];

            dev[0].UsagePage = HIDUsagePage.Generic;
            dev[0].Usage = HIDUsage.Keyboard;
            dev[0].Flags = 0;
            dev[0].WindowHandle = _source.Handle;

            var rv = RegisterRawInputDevices(dev, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
        }

        ~MultiKeyboardInput()
        {
            // deregister (Flags == .Remove)
            RAWINPUTDEVICE[] dev = new RAWINPUTDEVICE[1];

            dev[0].UsagePage = HIDUsagePage.Generic;
            dev[0].Usage = HIDUsage.Keyboard;
            dev[0].Flags = RawInputDeviceFlags.Remove;
            dev[0].WindowHandle = _source.Handle;

            RegisterRawInputDevices(dev, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == (int)WM.INPUT)
            {
                OnInput(hwnd, wParam, lParam);

                return IntPtr.Zero;
            }

            // just in case you want to do other things...

            return IntPtr.Zero;

        }

        void OnInput(IntPtr hwnd, IntPtr code, IntPtr hRawInput)
        {
            uint dwSize = 0;

            GetRawInputData(hRawInput, (uint)RID.Input, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);

            if (GetRawInputData(hRawInput, (uint)RID.Input, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == dwSize)
            {
                RAWINPUT raw = (RAWINPUT)Marshal.PtrToStructure(buffer, typeof(RAWINPUT));

                Win32.VK vk = (Win32.VK)raw.keyboard.VKey;

                Console.WriteLine("RAWINPUT.header->hDevice: " + raw.header.hDevice + " key: " + raw.keyboard.VKey + " (" + vk.ToString() + ")");

                _callbackRawInput(raw.header.hDevice, (Win32.VK)raw.keyboard.VKey);
            }
        }
    }
}
