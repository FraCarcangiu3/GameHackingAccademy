## DLL Injector (Windows)

Questo progetto è un semplice iniettore di DLL per Windows. L’iniettore cerca un processo target (per default `wesnoth.exe`), alloca memoria nel processo, scrive il percorso di una DLL e crea un thread remoto che chiama `LoadLibraryA` per caricare la DLL nel processo target.

Attenzione: l’uso di tecniche di injection può violare i Termini di Servizio di giochi/software o leggi/local policy. Utilizza il codice solo in contesti legali e su software di cui hai il permesso di effettuare reverse/injection.

### Come funziona (in breve)
- **Enumerazione processi (Unicode)**: usa `CreateToolhelp32Snapshot`, `Process32FirstW/Process32NextW` con `PROCESSENTRY32W` e confronto `_wcsicmp` per trovare il PID del processo target.
- **Apertura con privilegi minimi**: apre il processo con i soli diritti necessari (`PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ`).
- **Allocazione e scrittura**: alloca memoria nel processo target con `VirtualAllocEx` e scrive il percorso della DLL con `WriteProcessMemory`.
- **Risoluzione di LoadLibraryA**: ottiene l’indirizzo di `LoadLibraryA` da `kernel32.dll` con `GetModuleHandleA` e `GetProcAddress`.
- **Thread remoto**: crea un thread remoto con `CreateRemoteThread` che esegue `LoadLibraryA(percorsoDll)` nel processo target.
- **Sincronizzazione e risultato**: attende la terminazione del thread (`WaitForSingleObject`) e legge il codice di uscita (`GetExitCodeThread`), che corrisponde all’`HMODULE` della DLL caricata in caso di successo.
- **Pulizia**: libera la memoria remota con `VirtualFreeEx` e chiude gli handle.

Nel file `DLL_Injector/DLL_Injector/main.cpp` sono presenti anche:
- **Helper errori** (`stampaErrore`) che mostra `GetLastError` con messaggio di testo.
- **Override da riga di comando** del percorso DLL: se avvii l’eseguibile con un argomento, verrà usato come percorso della DLL al posto del default nel sorgente.

### Requisiti
- Windows (librerie Win32 disponibili: `windows.h`, `tlhelp32.h`).
- Compilatore/IDE con Windows SDK (es. Visual Studio). 
- La DLL e l’iniettore devono avere la **stessa architettura** del processo target:
  - Gioco 64-bit → DLL 64-bit + iniettore 64-bit
  - Gioco 32-bit → DLL 32-bit + iniettore 32-bit
- Permessi adeguati (spesso è necessario eseguire l’iniettore come **Amministratore**).

### Compilazione (Visual Studio)
1. Apri la soluzione o crea un progetto Console C++ e aggiungi `DLL_Injector/DLL_Injector/main.cpp`.
2. Seleziona la piattaforma corretta (x64 o Win32) in base al target.
3. Compila in modalità Debug o Release.

### Utilizzo
1. Assicurati che il processo target sia in esecuzione (default: `wesnoth.exe`).
2. Prepara il percorso assoluto alla tua DLL.
3. Avvia l’iniettore:

```bash
DLL_Injector.exe "C:\\percorso\\assoluto\\alla\\tua.dll"
```

Se non passi argomenti, l’iniettore usa il percorso di default definito nel sorgente (`dll_path`).

Output atteso in caso di successo:

```text
Remote thread exit code (HMODULE) = 0xXXXXXXXX
Fatto.
```

### Personalizzazioni comuni
- **Cambiare processo target**: modifica la variabile `target` in `main.cpp` (es. `const wchar_t* target = L"notepad.exe";`).
- **Usare `LoadLibraryW`**: se vuoi supportare percorsi DLL con caratteri non ASCII, scrivi il percorso come wide string nel processo remoto e risolvi `LoadLibraryW`.
- **Percorso da CLI e nome processo**: estendi il parsing degli argomenti per accettare anche il nome del processo dalla riga di comando.

### Troubleshooting
- Verifica la **corrispondenza 32/64-bit** tra gioco, DLL e iniettore.
- Assicurati che il percorso DLL sia **assoluto** ed esista.
- Esegui come **Amministratore** se il target è elevato o se ricevi errori di accesso negato.
- Disabilita/whitelista temporaneamente AV/EDR che possono bloccare injection o `CreateRemoteThread`.
- Verifica che la tua DLL abbia un `DllMain` valido e che non fallisca alla `DLL_PROCESS_ATTACH`.
- Alcuni processi protetti (PPL) o app UWP/servizi possono impedire l’injection.

### Nota legale
Questo codice è fornito a scopo didattico. L’autore e i contributori non sono responsabili per un uso improprio. Assicurati di avere i diritti e le autorizzazioni per sperimentare su un dato software.


