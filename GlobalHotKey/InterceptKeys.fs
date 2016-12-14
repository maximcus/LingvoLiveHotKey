module InterceptKeys

open System.Runtime.InteropServices
open System
open System.Diagnostics
open System.Windows.Forms

type LowLevelKeyboardProc = delegate of int * IntPtr * IntPtr -> IntPtr

let private _keyPressed = new Event<Keys>();
let internal KeyPressed = _keyPressed.Publish

[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, UInt32 dwThreadId)

[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern [<MarshalAs(UnmanagedType.Bool)>] bool UnhookWindowsHookEx(IntPtr hhk);

[<DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

[<DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
extern IntPtr GetModuleHandle(string lpModuleName);

let mutable private _hookID = IntPtr.Zero
let private WH_KEYBOARD_LL = 13
let private WM_KEYDOWN = IntPtr 0x0100

let private HookCallback nCode wParam (lParam : IntPtr) =    
    if (nCode >= 0 && wParam = WM_KEYDOWN) then
        let vkCode = Marshal.ReadInt32(lParam)
        let pressedKey : Keys  = enum vkCode
        _keyPressed.Trigger pressedKey

    CallNextHookEx(_hookID, nCode, wParam, lParam)

let private hookCallback = LowLevelKeyboardProc HookCallback

let private SetHook callback =
    use curProcess = Process.GetCurrentProcess()
    use curModule = curProcess.MainModule
    SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, GetModuleHandle(curModule.ModuleName), 0u)

let internal Start() =
    _hookID = SetHook hookCallback

let internal Stop() = UnhookWindowsHookEx _hookID