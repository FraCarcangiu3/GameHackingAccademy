## Iniettore di DLL per principianti (Windows)

Spiegazione didattica e completa di tutto il file `DLL_Injector/DLL_Injector/main.cpp`, suddivisa in blocchi. L’obiettivo è capire ogni riga come un principiante, senza dare nulla per scontato.

Importante: usa questo progetto solo in contesti legali e con software su cui hai i diritti. Le tecniche di injection possono violare ToS o policy anti‑cheat.

### Blocco 1 — Commento introduttivo (righe 1–22)
Nel commento iniziale spieghiamo l’idea generale: per caricare una DLL dentro un altro processo (il gioco), scriviamo il percorso della DLL nella memoria del gioco e poi creiamo un thread remoto che chiama `LoadLibraryA` con quel percorso. Così la DLL viene caricata nel processo target.

### Blocco 2 — Inclusioni (righe 23–27)
```cpp
// Inclusioni: usiamo le API Win32 e l'helper C per stampa diagnostica
#include <windows.h>
#include <tlhelp32.h>
#include <cstdio>
```
- `windows.h`: include principale per le API Win32 (processi, thread, memoria, ecc.).
- `tlhelp32.h`: API per fare snapshot dei processi (`CreateToolhelp32Snapshot`, `Process32FirstW`, `Process32NextW`).
- `cstdio`: per funzioni di stampa (`printf`, `fprintf`).

### Blocco 3 — Percorso DLL di default (righe 29–31)
```cpp
// Il percorso completo della DLL da iniettare.
//const char* dll_path = "...InternalMemoryHack.dll";
const char* dll_path = "...Wesnoth_CodeCaveDLL.dll";
```
- `dll_path` è un fallback: se non passi la DLL da riga di comando, useremo questo percorso.
- Consiglio pratico: passa sempre il percorso assoluto via CLI per evitare errori di cartella/piattaforma.

### Blocco 4 — Helper errori (righe 33–43)
```cpp
static void stampaErrore(const char* dove) { /* GetLastError + FormatMessageA */ }
```
- Chiama `GetLastError()` e lo traduce in testo umano con `FormatMessageA`.
- Ogni volta che un’API critica fallisce, chiamiamo questo helper per capire subito il motivo.

### Blocco 5 — Verifica file esistente (righe 45–49)
```cpp
static bool fileEsisteA(const char* percorso) { /* GetFileAttributesA */ }
```
- Controlla che il percorso della DLL punti a un file reale. Se non esiste, fermiamo l’esecuzione prima di tentare l’injection.

### Blocco 6 — Ingresso `main` e setup IO (righe 51–66)
```cpp
int main(int argc, char** argv) {
  const char* percorsoDll = (argc > 1) ? argv[1] : dll_path;
  const wchar_t* target = L"wesnoth.exe";
  bool pausaFinale = true; // teniamo aperta la console
  setvbuf(stdout, NULL, _IONBF, 0);
  setvbuf(stderr, NULL, _IONBF, 0);
  printf("[Injector] Target: "); wprintf(L"%s\n", target);
  printf("[Injector] DLL: %s\n", percorsoDll);
```
- Recuperiamo la DLL da CLI, altrimenti usiamo il default.
- Impostiamo `pausaFinale=true` per lasciare aperta la console e leggere l’output.
- Disabilitiamo il buffering così i messaggi appaiono immediatamente.

### Blocco 7 — Validazione percorso DLL (righe 67–72)
```cpp
if (!fileEsisteA(percorsoDll)) { /* stampa errore e return 1 */ }
```
- Evita subito l’errore più comune: path errato o DLL non compilata dove pensi.

### Blocco 8 — Attesa del processo target (righe 74–98)
```cpp
DWORD pid = 0; DWORD atteso = 0; const DWORD attesaMaxMs = 30000;
while (!pid && atteso <= attesaMaxMs) {
  HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
  PROCESSENTRY32W pe = {0}; pe.dwSize = sizeof(pe);
  if (Process32FirstW(snap, &pe)) {
    do { if (_wcsicmp(pe.szExeFile, target) == 0) { pid = pe.th32ProcessID; break; } } while (Process32NextW(snap, &pe));
  }
  CloseHandle(snap);
  if (!pid) { Sleep(500); atteso += 500; }
}
```
- Fa polling ogni 500 ms fino a 30 s per trovare il processo `wesnoth.exe`.
- Variante “W” (Unicode) per robustezza con nomi non ASCII.

### Blocco 9 — Apertura del processo (righe 102–109)
```cpp
HANDLE process = OpenProcess(
  PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
  PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
  FALSE, pid);
```
- Diritti minimi per creare thread e leggere/scrivere memoria. Niente `PROCESS_ALL_ACCESS`.
- Se fallisce: esegui come Amministratore o verifica che il processo sia davvero attivo.

### Blocco 10 — Allocazione memoria remota (righe 110–116)
```cpp
SIZE_T sz = strlen(percorsoDll) + 1;
LPVOID remote = VirtualAllocEx(process, NULL, sz, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
printf("[Injector] Memoria remota allocata a: %p (size=%zu)\n", remote, (size_t)sz);
```
- Prenota memoria nel target per contenere il percorso della DLL (stringa ANSI con terminatore `\0`).

### Blocco 11 — Scrittura del percorso nel target (righe 117–125)
```cpp
if (!WriteProcessMemory(process, remote, percorsoDll, sz, NULL)) { /* errore */ }
printf("[Injector] Percorso DLL scritto in memoria remota.\n");
```
- Copia i byte della stringa nel processo target. Fallisce se i diritti sono insufficienti.

### Blocco 12 — Risoluzione di `LoadLibraryA` (righe 127–143)
```cpp
HMODULE k32 = GetModuleHandleA("kernel32.dll");
FARPROC pLoadLibA = GetProcAddress(k32, "LoadLibraryA");
printf("[Injector] Indirizzo LoadLibraryA: %p\n", pLoadLibA);
```
- Ottiene l’indirizzo della funzione di caricamento DLL.
- L’indirizzo è valido anche per il target (stesso modulo di sistema mappato in tutti i processi).

### Blocco 13 — Creazione del thread remoto (righe 145–155)
```cpp
HANDLE th = CreateRemoteThread(process, NULL, 0, (LPTHREAD_START_ROUTINE)pLoadLibA, remote, 0, NULL);
printf("[Injector] Thread remoto creato: %p\n", th);
```
- Crea un thread nel processo target, che esegue `LoadLibraryA(remote)`.
- `remote` è l’indirizzo dove abbiamo scritto la stringa con il percorso DLL.

### Blocco 14 — Attesa e lettura risultato (righe 156–164)
```cpp
WaitForSingleObject(th, INFINITE);
DWORD exitCode = 0; GetExitCodeThread(th, &exitCode);
printf("Remote thread exit code (HMODULE) = 0x%08lX\n", (unsigned long)exitCode);
```
- L’exit code del thread è l’`HMODULE` restituito da `LoadLibraryA`: diverso da 0 = successo.
- 0 significa fallimento: vedi la sezione Troubleshooting.

### Blocco 15 — Pulizia risorse e pausa (righe 166–174)
```cpp
VirtualFreeEx(process, remote, 0, MEM_RELEASE);
CloseHandle(th); CloseHandle(process);
puts("Fatto."); puts("Premi Invio per uscire..."); getchar();
```
- Libera memoria remota e chiude gli handle. Mantiene aperta la console per leggere l’output.

---

## Uso pratico
- Compila in Win32 se il gioco è 32‑bit, in x64 se è 64‑bit. La DLL deve avere la stessa architettura del gioco.
- Avvia il gioco, poi esegui:
```bat
DLL_Injector.exe "C:\\Users\\tuoUtente\\source\\repos\\Wesnoth_CodeCaveDLL\\Debug\\Wesnoth_CodeCaveDLL.dll"
```
- Se non passi argomenti, verrà usato `dll_path` hard‑coded.

## Troubleshooting mirato
- **Percorso DLL errato**: ricontrolla cartelle `Debug` vs `x64\\Debug`. Usa “Copia come percorso”.
- **`HMODULE = 0`**: mancano dipendenze della DLL; percorso con caratteri non ASCII (usa `LoadLibraryW`); `DllMain` fallisce; AV/EDR blocca l’injection.
- **Nessun effetto in gioco**: il problema è nella DLL (offset/codecave/patch). Aggiungi `MessageBoxA` in `DllMain` e usa `FlushInstructionCache` dopo le patch.
- **Permessi**: se il target è elevato, esegui l’iniettore come Amministratore.

## Estensioni consigliate
- Parametri CLI per processo target e percorso DLL.
- Variante con `LoadLibraryW` scrivendo una wide‑string nel target.
- Log su file oltre alla console.

## Nota legale
Codice per scopi educativi. Usalo responsabilmente e solo dove consentito.

## Approfondimenti didattici

### Blocco 8 — Attesa del processo target (spiegazione completa)

Obiettivo: trovare il PID (Process ID) del processo bersaglio, ad esempio `wesnoth.exe`. Finché non è avviato, aspettiamo e riproviamo fino a un limite di tempo.

Passi, uno per uno:
- Inizializziamo variabili di controllo:
  - `pid = 0`: finché resta 0, il processo non è stato trovato.
  - `attesaMaxMs = 30000`: aspettiamo al massimo 30 secondi.
  - `atteso = 0`: tempo già atteso.
- Ciclo di polling: finché `pid == 0` e `atteso <= attesaMaxMs`:
  - `CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0)`: crea uno "snapshot" (fotografia) dei processi correnti.
  - Prepariamo `PROCESSENTRY32W pe = {0}` e impostiamo `pe.dwSize = sizeof(pe)` (obbligatorio per le API Win32).
  - `Process32FirstW(snap, &pe)` per leggere il primo processo; poi `Process32NextW` in un ciclo per scorrere tutti.
  - Confrontiamo `_wcsicmp(pe.szExeFile, target)` per verificare se il nome dell'eseguibile coincide (case-insensitive, wide/Unicode).
  - Se coincide, salviamo `pe.th32ProcessID` in `pid`.
  - `CloseHandle(snap)` per chiudere sempre l'handle dello snapshot.
  - Se `pid` è ancora 0: `Sleep(500)` e sommiamo 500 a `atteso`.
- Se dopo il ciclo `pid` è 0, stampiamo un messaggio chiaro e usciamo: il processo non è stato trovato entro il tempo limite.

Perché polling e non "eventi"? La ToolHelp API fornisce una vista statica; non esiste una callback "avvisami quando appare X", quindi si controlla periodicamente.

Errori comuni:
- Dimenticare `pe.dwSize = sizeof(pe)`: la funzione fallisce.
- Non chiudere lo snapshot con `CloseHandle`: perdite di handle.
- Nome processo errato o diverso (versioni Steam, localizzazioni): `_wcsicmp` non troverà match.
- Timeout troppo breve: aumenta `attesaMaxMs` se serve.

### Blocco 12 — Risoluzione di LoadLibraryA (spiegazione completa)

Obiettivo: ottenere l'indirizzo della funzione `LoadLibraryA` in `kernel32.dll` per usarla come entry-point del thread remoto. Il thread nel processo target chiamerà quella funzione per caricare la DLL.

Passi, uno per uno:
- `HMODULE k32 = GetModuleHandleA("kernel32.dll");`
  - Restituisce l'handle (base address) del modulo `kernel32.dll` caricato nel nostro processo.
- `FARPROC pLoadLibA = GetProcAddress(k32, "LoadLibraryA");`
  - Cerca il simbolo esportato "LoadLibraryA" dentro `kernel32.dll` e restituisce un puntatore a funzione generico.

Perché questo indirizzo è valido anche nel target?
- Le DLL di sistema (come `kernel32.dll`) sono mappate in modo coerente tra processi. Con questa tecnica didattica è sufficiente usare l'indirizzo risolto nel nostro processo: il thread remoto nel target potrà eseguire la stessa funzione.

Come lo usiamo con `CreateRemoteThread`:
- Castiamo `pLoadLibA` a `LPTHREAD_START_ROUTINE` perché l'API richiede un puntatore a funzione con firma `DWORD WINAPI Fn(LPVOID)`.
- Passiamo come parametro l'indirizzo, NEL TARGET, della stringa col percorso assoluto della DLL (ottenuto da `VirtualAllocEx` + `WriteProcessMemory`).
- Il thread remoto esegue quindi `LoadLibraryA(parametro)`, il loader carica la DLL e l'`HMODULE` restituito diventa l'exit code del thread (che leggiamo con `GetExitCodeThread`).

Cause tipiche di fallimento:
- `GetModuleHandleA` o `GetProcAddress` falliscono (raro): abortiamo e stampiamo l'errore.
- La stringa passata a `LoadLibraryA` non è valida nel target (indirizzo errato o memoria liberata).
- Il percorso contiene caratteri non ASCII: meglio usare `LoadLibraryW` e scrivere una wide-string nel target.



