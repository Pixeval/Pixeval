using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace Pixeval.Misc
{
    internal static class HookInjector
    {
        public static void Inject(uint processId)
        {
            // geting the handle of the process - with required privileges
            Kernel32.SafeHPROCESS procHandle = Kernel32.OpenProcess(ACCESS_MASK.GENERIC_ALL, false, processId);

            // searching for the address of LoadLibraryA and storing it in a pointer
            Kernel32.ThreadProc procLoadLibrary = Marshal.GetDelegateForFunctionPointer<Kernel32.ThreadProc>(
                Kernel32.GetProcAddress(Kernel32.GetModuleHandle("kernel32.dll"), "LoadLibraryA"));

            // name of the dll we want to inject
            string dllName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Pixeval.Hooks.dll");

            // alocating some memory on the target process - enough to store the name of the dll
            // and storing its address in a pointer
            IntPtr allocMemAddress = Kernel32.VirtualAllocEx(procHandle, IntPtr.Zero,
                (dllName.Length + 1) * sizeof(char), Kernel32.MEM_ALLOCATION_TYPE.MEM_COMMIT|Kernel32.MEM_ALLOCATION_TYPE.MEM_RESERVE,Kernel32.MEM_PROTECTION.PAGE_READWRITE);

            // writing the name of the dll there
            SizeT bytesWritten;
            Kernel32.WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(dllName),
                (dllName.Length + 1) * sizeof(char),out bytesWritten);

            if (bytesWritten != (dllName.Length + 1) * sizeof(char))
            {
                Console.WriteLine("Failed to write remote DLL path name");
                procHandle.Close();
            }
            // creating a thread that will call LoadLibraryA with allocMemAddress as argument
            Kernel32.SafeHTHREAD remoteThread = Kernel32.CreateRemoteThread(procHandle, null, 0, procLoadLibrary, allocMemAddress, 0, out _);
            if (remoteThread is not null)
            {
                // Explicitly wait for LoadLibraryW to complete before releasing memory
                // avoids causing a remote memory leak
                Kernel32.WaitForSingleObject(remoteThread, Kernel32.INFINITE);
                remoteThread.Close();
            }
            Kernel32.VirtualFreeEx(procHandle, allocMemAddress, 0, Kernel32.MEM_ALLOCATION_TYPE.MEM_RELEASE);
            procHandle.Close();
        }
    }
}
