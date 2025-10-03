/*
Un iniettore di DLL che inietta la DLL specificata in dll_path nel processo wesnoth. Questo iniettore richiede che Urban Terror sia in esecuzione.

Un processo diverso può essere specificato cambiando il nome dell'eseguibile nella chiamata a strcmp.

Per caricare librerie statiche e dinamiche, gli eseguibili Windows possono usare la funzione API LoadLibraryA. Questa funzione prende un singolo argomento,
che è il percorso completo della libreria da caricare:

HMODULE LoadLibraryA(
	LPCSTR lpLibFileName
);

Se chiamiamo LoadLibraryA nel codice del nostro iniettore, la DLL verrà caricata nella memoria del nostro iniettore. Invece, vogliamo che il nostro iniettore forzi il gioco
a chiamare LoadLibraryA. Per fare ciò, useremo l'API CreateRemoteThread per creare un nuovo thread nel gioco. Questo thread eseguirà quindi LoadLibraryA
all'interno del processo in esecuzione del gioco.

Tuttavia, poiché il thread viene eseguito all'interno della memoria del gioco, LoadLibraryA non sarà in grado di trovare il percorso della nostra DLL specificato nel nostro iniettore.
Per ovviare a questo, dobbiamo scrivere il percorso della nostra DLL nella memoria del gioco. Per assicurarci di non corrompere altra memoria, dovremo anche
allocare memoria aggiuntiva all'interno del gioco usando VirtualAllocEx.

La spiegazione completa di come funziona questo codice è disponibile su https://gamehacking.academy/lesson/25
*/
#include <windows.h>
#include <tlhelp32.h>


// Il percorso completo della DLL da iniettare.
//const char* dll_path = "C:\\Users\\francesco\\source\\repos\\InternalMemoryHack\\Debug\\InternalMemoryHack.dll";
const char* dll_path = "C:\\Users\\francesco\\source\\repos\\InternalMemoryHack\\Wesnoth_CodeCaveDLL\\Debug\\Wesnoth_CodeCaveDLL.dll";

int main(int argc, char** argv) {
	HANDLE snapshot = 0;
	PROCESSENTRY32 pe32 = { 0 };

	DWORD exitCode = 0;

	pe32.dwSize = sizeof(PROCESSENTRY32);

	// Il codice per lo snapshot è una versione ridotta dell'esempio fornito da Microsoft su 
	// https://docs.microsoft.com/en-us/windows/win32/toolhelp/taking-a-snapshot-and-viewing-processes
	snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	Process32First(snapshot, &pe32);

	do {
		// Vogliamo operare solamente sul processo di wesnoth
		if (wcscmp(pe32.szExeFile, L"wesnoth.exe") == 0) {
			// Per prima cosa, dobbiamo ottenere un handle del processo da usare per le chiamate successive
			HANDLE process = OpenProcess(PROCESS_ALL_ACCESS, true, pe32.th32ProcessID);

			// Per non corrompere memoria esistente, allocare memoria aggiuntiva per contenere il percorso della nostra DLL
			void* lpBaseAddress = VirtualAllocEx(process, NULL, strlen(dll_path) + 1, MEM_COMMIT, PAGE_READWRITE);

			// Scrivere il percorso della nostra DLL nella memoria che abbiamo appena allocato dentro il gioco
			WriteProcessMemory(process, lpBaseAddress, dll_path, strlen(dll_path) + 1, NULL);

			// Creare un thread remoto all'interno del gioco che eseguirà LoadLibraryA
			// A questa chiamata LoadLibraryA passeremo il percorso completo della nostra DLL che abbiamo scritto nel processo
			HMODULE kernel32base = GetModuleHandle(L"kernel32.dll");
			HANDLE thread = CreateRemoteThread(process, NULL, 0, (LPTHREAD_START_ROUTINE)GetProcAddress(kernel32base, "LoadLibraryA"), lpBaseAddress, 0, NULL);

			// Per assicurarci che la nostra DLL sia stata iniettata, possiamo usare le due chiamate seguenti per bloccare l'esecuzione del programma
			WaitForSingleObject(thread, INFINITE);
			GetExitCodeThread(thread, &exitCode);

			// Infine liberare la memoria e pulire gli handle di processo
			VirtualFreeEx(process, lpBaseAddress, 0, MEM_RELEASE);
			CloseHandle(thread);
			CloseHandle(process);
			break;
		}
	} while (Process32Next(snapshot, &pe32));

	return 0;
}