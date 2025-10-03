#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>

// Percorso della DLL da iniettare
const char* dll_path = "C:\\Users\\francesco\\source\\repos\\InternalMemoryHack\\Wesnoth_CodeCaveDLL\\Debug\\Wesnoth_CodeCaveDLL.dll";

// Nome dell'evento (deve corrispondere in DLL)
const char* EVENT_NAME = "WesnothHookEvent";

int main(int argc, char** argv) {
    HANDLE snapshot = NULL;
    PROCESSENTRY32 pe32 = { 0 };
    pe32.dwSize = sizeof(pe32);

    // Creiamo l'evento prima di iniettare. Evento manual-reset (TRUE), inizialmente non segnato (FALSE)
    HANDLE hEvent = CreateEventA(NULL, TRUE, FALSE, EVENT_NAME);
    if (!hEvent) {
        printf("CreateEvent fallita: %lu\n", GetLastError());
        return 1;
    }

    snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snapshot == INVALID_HANDLE_VALUE) {
        printf("Snapshot fallito: %lu\n", GetLastError());
        CloseHandle(hEvent);
        return 1;
    }

    if (!Process32First(snapshot, &pe32)) {
        printf("Process32First fallito: %lu\n", GetLastError());
        CloseHandle(snapshot);
        CloseHandle(hEvent);
        return 1;
    }

    do {
        if (wcscmp(pe32.szExeFile, L"wesnoth.exe") == 0) {
            HANDLE process = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pe32.th32ProcessID);
            if (!process) {
                printf("OpenProcess fallito: %lu\n", GetLastError());
                break;
            }

            // Allocare spazio e scrivere il path della DLL
            void* lpBaseAddress = VirtualAllocEx(process, NULL, strlen(dll_path) + 1, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (!lpBaseAddress) {
                printf("VirtualAllocEx fallito: %lu\n", GetLastError());
                CloseHandle(process);
                break;
            }

            if (!WriteProcessMemory(process, lpBaseAddress, dll_path, strlen(dll_path) + 1, NULL)) {
                printf("WriteProcessMemory fallito: %lu\n", GetLastError());
                VirtualFreeEx(process, lpBaseAddress, 0, MEM_RELEASE);
                CloseHandle(process);
                break;
            }

            HMODULE kernel32base = GetModuleHandle(L"kernel32.dll");
            LPTHREAD_START_ROUTINE pLoadLibraryA = (LPTHREAD_START_ROUTINE)GetProcAddress(kernel32base, "LoadLibraryA");
            if (!pLoadLibraryA) {
                printf("GetProcAddress LoadLibraryA fallito: %lu\n", GetLastError());
                VirtualFreeEx(process, lpBaseAddress, 0, MEM_RELEASE);
                CloseHandle(process);
                break;
            }

            HANDLE thread = CreateRemoteThread(process, NULL, 0, pLoadLibraryA, lpBaseAddress, 0, NULL);
            if (!thread) {
                printf("CreateRemoteThread fallito: %lu\n", GetLastError());
                VirtualFreeEx(process, lpBaseAddress, 0, MEM_RELEASE);
                CloseHandle(process);
                break;
            }

            // Aspettiamo che LoadLibrary termini (opzionale, conferma l'iniezione)
            WaitForSingleObject(thread, INFINITE);

            // Pulizie
            DWORD exitCode = 0;
            GetExitCodeThread(thread, &exitCode);
            VirtualFreeEx(process, lpBaseAddress, 0, MEM_RELEASE);
            CloseHandle(thread);
            CloseHandle(process);

            // ORA aspettiamo che la DLL segnali il nostro evento (cioè finché la codecave non chiama SetEvent)
            printf("Iniettato. In attesa che la DLL setti l'evento \"%s\"...\n", EVENT_NAME);
            DWORD waitResult = WaitForSingleObject(hEvent, INFINITE);
            if (waitResult == WAIT_OBJECT_0) {
                printf("Evento ricevuto: la DLL ha segnalato l'azione.\n");
            }
            else {
                printf("WaitForSingleObject fallito/timeout: %lu\n", GetLastError());
            }

            // eventualmente resettiamo l'evento (se vogliamo usarlo di nuovo)
            ResetEvent(hEvent);

            break;
        }
    } while (Process32Next(snapshot, &pe32));

    CloseHandle(snapshot);
    CloseHandle(hEvent);
    return 0;
}