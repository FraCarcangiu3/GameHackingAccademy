/*
Un iniettore di DLL che inietta la DLL specificata in dll_path nel processo wesnoth. Questo iniettore richiede che Urban Terror sia in esecuzione.

Un processo diverso pu� essere specificato cambiando il nome dell'eseguibile nella chiamata a strcmp.

Per caricare librerie statiche e dinamiche, gli eseguibili Windows possono usare la funzione API LoadLibraryA. Questa funzione prende un singolo argomento,
che � il percorso completo della libreria da caricare:

HMODULE LoadLibraryA(
	LPCSTR lpLibFileName
);

Se chiamiamo LoadLibraryA nel codice del nostro iniettore, la DLL verr� caricata nella memoria del nostro iniettore. Invece, vogliamo che il nostro iniettore forzi il gioco
a chiamare LoadLibraryA. Per fare ci�, useremo l'API CreateRemoteThread per creare un nuovo thread nel gioco. Questo thread eseguir� quindi LoadLibraryA
all'interno del processo in esecuzione del gioco.

Tuttavia, poich� il thread viene eseguito all'interno della memoria del gioco, LoadLibraryA non sar� in grado di trovare il percorso della nostra DLL specificato nel nostro iniettore.
Per ovviare a questo, dobbiamo scrivere il percorso della nostra DLL nella memoria del gioco. Per assicurarci di non corrompere altra memoria, dovremo anche
allocare memoria aggiuntiva all'interno del gioco usando VirtualAllocEx.

La spiegazione completa di come funziona questo codice � disponibile su https://gamehacking.academy/lesson/25
*/
// Inclusioni: usiamo le API Win32 e l'helper C per stampa diagnostica
#include <windows.h>
#include <tlhelp32.h>
#include <cstdio>


// Il percorso completo della DLL da iniettare.
//const char* dll_path = "C:\\Users\\francesco\\source\\repos\\InternalMemoryHack\\Debug\\InternalMemoryHack.dll";
const char* dll_path = "C:\\Users\\francesco\\source\\repos\\InternalMemoryHack\\Wesnoth_CodeCaveDLL\\Debug\\Wesnoth_CodeCaveDLL.dll";

// Helper di errore: centralizza la stampa di GetLastError con messaggio leggibile
static void stampaErrore(const char* dove) {
	DWORD e = GetLastError();
	LPSTR msg = NULL;
	FormatMessageA(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL, e, 0, (LPSTR)&msg, 0, NULL
	);
	fprintf(stderr, "%s fallita: %lu (%s)\n", dove, (unsigned long)e, msg ? msg : "");
	if (msg) LocalFree(msg);
}

int main(int argc, char** argv) {
	// Permettiamo di passare il percorso DLL da riga di comando, altrimenti usiamo il default
	const char* percorsoDll = (argc > 1) ? argv[1] : dll_path;
	// Usiamo stringhe wide per enumerare processi in modo robusto (evita problemi di locale)
	const wchar_t* target = L"wesnoth.exe"; // modifica se vuoi un altro processo

	// Creiamo uno snapshot dei processi correnti
	HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (snap == INVALID_HANDLE_VALUE) { stampaErrore("CreateToolhelp32Snapshot"); return 1; }

	// Usiamo la variante wide delle strutture/funzioni di enumerazione
	PROCESSENTRY32W pe = { 0 };
	pe.dwSize = sizeof(pe);
	if (!Process32FirstW(snap, &pe)) { stampaErrore("Process32FirstW"); CloseHandle(snap); return 1; }

	// Cerchiamo il PID del processo di destinazione (case-insensitive)
	DWORD pid = 0;
	do {
		if (_wcsicmp(pe.szExeFile, target) == 0) { pid = pe.th32ProcessID; break; }
	} while (Process32NextW(snap, &pe));
	CloseHandle(snap);

	if (!pid) { fwprintf(stderr, L"Processo %s non trovato.\n", target); return 1; }

	// Apriamo il processo con i privilegi minimi necessari (no PROCESS_ALL_ACCESS)
	HANDLE process = OpenProcess(
		PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
		FALSE,
		pid
	);
	if (!process) { stampaErrore("OpenProcess"); return 1; }

	// Allochiamo memoria nel processo remoto per il percorso DLL (in byte, ANSI per LoadLibraryA)
	SIZE_T sz = strlen(percorsoDll) + 1;
	LPVOID remote = VirtualAllocEx(process, NULL, sz, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (!remote) { stampaErrore("VirtualAllocEx"); CloseHandle(process); return 1; }

	// Scriviamo il percorso della DLL nella memoria remota
	if (!WriteProcessMemory(process, remote, percorsoDll, sz, NULL)) {
		stampaErrore("WriteProcessMemory");
		VirtualFreeEx(process, remote, 0, MEM_RELEASE);
		CloseHandle(process);
		return 1;
	}

	// Risolviamo l'indirizzo di LoadLibraryA da kernel32 nel nostro processo
	HMODULE k32 = GetModuleHandleA("kernel32.dll");
	if (!k32) {
		stampaErrore("GetModuleHandleA(kernel32.dll)");
		VirtualFreeEx(process, remote, 0, MEM_RELEASE);
		CloseHandle(process);
		return 1;
	}
	FARPROC pLoadLibA = GetProcAddress(k32, "LoadLibraryA");
	if (!pLoadLibA) {
		stampaErrore("GetProcAddress(LoadLibraryA)");
		VirtualFreeEx(process, remote, 0, MEM_RELEASE);
		CloseHandle(process);
		return 1;
	}

	// Creiamo il thread remoto che eseguirà LoadLibraryA(percorso DLL) nel target
	HANDLE th = CreateRemoteThread(process, NULL, 0, (LPTHREAD_START_ROUTINE)pLoadLibA, remote, 0, NULL);
	if (!th) {
		stampaErrore("CreateRemoteThread");
		VirtualFreeEx(process, remote, 0, MEM_RELEASE);
		CloseHandle(process);
		return 1;
	}

	// Attendiamo che il thread finisca e leggiamo il codice di uscita (HMODULE della DLL se successo)
	WaitForSingleObject(th, INFINITE);

	DWORD exitCode = 0;
	if (!GetExitCodeThread(th, &exitCode)) {
		stampaErrore("GetExitCodeThread");
	} else {
		printf("Remote thread exit code (HMODULE) = 0x%08lX\n", (unsigned long)exitCode);
	}

	// Pulizia risorse nel target e nel nostro processo
	VirtualFreeEx(process, remote, 0, MEM_RELEASE);
	CloseHandle(th);
	CloseHandle(process);

	puts("Fatto.");
	return 0;
}