/*
A DLL injector that injects the DLL specified in dll_path into the wesnoth process. This injector requires Urban Terror to be running.

A different process can be specified by changing the executable name in the strcmp call.

To load static and dynamic libraries, Windows executables can use the LoadLibraryA API function. This function takes a single argument,
which is the full path of the library to load:

HMODULE LoadLibraryA(
	LPCSTR lpLibFileName
);

If we call LoadLibraryA in the code of our injector, the DLL will be loaded into the memory of our injector. Instead, we want our injector to force the game
to call LoadLibraryA. To do this, we will use the CreateRemoteThread API to create a new thread in the game. That thread will then execute LoadLibraryA
inside the running game process.

However, since the thread runs inside the game's memory, LoadLibraryA will not be able to find the path to our DLL specified in our injector.
To work around this, we must write the path of our DLL into the game's memory. To ensure we do not corrupt other memory, we will also
allocate additional memory inside the game using VirtualAllocEx.

The full explanation of how this code works is available at https://gamehacking.academy/lesson/25
*/
#include <windows.h>
#include <tlhelp32.h>


// The full path of the DLL to inject.
const char* dll_path = "C:\\Users\\francesco\\source\\repos\\InternalMemoryHack\\Debug\\InternalMemoryHack.dll";

int main(int argc, char** argv) {
	HANDLE snapshot = 0;
	PROCESSENTRY32 pe32 = { 0 };

	DWORD exitCode = 0;

	pe32.dwSize = sizeof(PROCESSENTRY32);

	// The snapshot code is a reduced version of the Microsoft example at
	// https://docs.microsoft.com/en-us/windows/win32/toolhelp/taking-a-snapshot-and-viewing-processes
	snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	Process32First(snapshot, &pe32);

	do {
		// We only want to operate on the wesnoth process
		if (wcscmp(pe32.szExeFile, L"wesnoth.exe") == 0) {
			// First, we need to obtain a handle to the process to use for subsequent calls
			HANDLE process = OpenProcess(PROCESS_ALL_ACCESS, true, pe32.th32ProcessID);

			// To avoid corrupting existing memory, allocate extra memory to hold the path to our DLL
			void* lpBaseAddress = VirtualAllocEx(process, NULL, strlen(dll_path) + 1, MEM_COMMIT, PAGE_READWRITE);

			// Write the path of our DLL into the memory we just allocated inside the game
			WriteProcessMemory(process, lpBaseAddress, dll_path, strlen(dll_path) + 1, NULL);

			// Create a remote thread inside the game that will execute LoadLibraryA
			// We will pass to this LoadLibraryA call the full path of our DLL that we wrote into the process
			HMODULE kernel32base = GetModuleHandle(L"kernel32.dll");
			HANDLE thread = CreateRemoteThread(process, NULL, 0, (LPTHREAD_START_ROUTINE)GetProcAddress(kernel32base, "LoadLibraryA"), lpBaseAddress, 0, NULL);

			// To ensure our DLL was injected, we can use the two calls below to block program execution
			WaitForSingleObject(thread, INFINITE);
			GetExitCodeThread(thread, &exitCode);

			// Finally free the memory and clean up the process handles
			VirtualFreeEx(process, lpBaseAddress, 0, MEM_RELEASE);
			CloseHandle(thread);
			CloseHandle(process);
			break;
		}
	} while (Process32Next(snapshot, &pe32));

	return 0;
}