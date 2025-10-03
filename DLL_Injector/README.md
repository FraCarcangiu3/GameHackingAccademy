## Iniettore di DLL per principianti (Windows)

Questo progetto è una piccola applicazione C++ che “inietta” una DLL dentro un altro programma in esecuzione (il gioco). L’idea pratica: diciamo al gioco di chiamare `LoadLibraryA("percorso\\alla\\tua.dll")` creando un thread dentro di lui. Così la tua DLL viene caricata e può eseguire codice all’interno del processo del gioco.

Importante: usa questo progetto solo in contesti legali e con software su cui hai i diritti. Le tecniche di injection possono violare ToS o policy anti-cheat.

### Obiettivo del documento
- Spiegarti tutto da zero, senza dare nulla per scontato.
- Guidarti nell’installazione, compilazione, esecuzione e debug.
- Darti strumenti per capire cosa succede “sotto il cofano”.

### Cos’è una DLL e cos’è l’iniezione
- **DLL**: libreria caricabile a runtime da un processo. Dentro può esserci codice (funzioni) eseguito dal processo che la carica.
- **Iniezione**: far caricare forzatamente una DLL da un processo che non la caricherebbe da solo.

### Come funziona l’iniettore, passo per passo
1) Trova il processo target (per default `wesnoth.exe`).
   - Usa le API: `CreateToolhelp32Snapshot`, `Process32FirstW/Process32NextW` e confronta il nome in modo case-insensitive.
2) Apre il processo con i diritti minimi indispensabili.
   - `PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ`.
3) Alloca memoria dentro il processo target.
   - `VirtualAllocEx(..., PAGE_READWRITE)` per riservare lo spazio dove scrivere la stringa col percorso della DLL.
4) Scrive il percorso assoluto della DLL in quella memoria.
   - `WriteProcessMemory` copia la stringa dal tuo processo a quello del gioco.
5) Risolve l’indirizzo di `LoadLibraryA`.
   - `GetModuleHandleA("kernel32.dll")` + `GetProcAddress("LoadLibraryA")` nel tuo processo (l’indirizzo è valido anche nel target).
6) Crea un thread remoto nel target che esegue `LoadLibraryA(percorsoDLL)`. 
   - `CreateRemoteThread(process, ..., LoadLibraryA, indirizzoStringa, ...)`.
7) Aspetta la fine del thread e legge il risultato.
   - `GetExitCodeThread` restituisce un valore: se diverso da `0`, è l’`HMODULE` della DLL caricata (successo).
8) Pulisce risorse.
   - Libera la memoria remota (`VirtualFreeEx`) e chiude gli handle.

Nel codice (`DLL_Injector/DLL_Injector/main.cpp`) sono inclusi:
- Messaggi chiari in console per ogni fase (trovi prefisso `[Injector]`).
- Un controllo che il file DLL esista prima di proseguire.
- Una piccola attesa (fino a 30s) per consentire al processo target di avviarsi.

### Perché usiamo le versioni “W” (wide) delle API di enumerazione processi
- `Process32FirstW/NextW` e `PROCESSENTRY32W` lavorano con stringhe wide (Unicode). 
- Questo evita problemi con nomi di processo contenenti caratteri non ASCII e rende il codice più robusto su sistemi non italiani.

### Perché non usiamo PROCESS_ALL_ACCESS
- Concedere il minimo indispensabile riduce i fallimenti dovuti a UAC/permessi e rende il codice più “corretto”.
- I diritti che usiamo bastano per: creare thread, leggere/scrivere memoria, cambiare protezioni.

### ANSI vs Unicode: `LoadLibraryA` o `LoadLibraryW`
- `LoadLibraryA` vuole una stringa ANSI (8-bit). Funziona finché il percorso contiene solo caratteri ASCII “semplici”.
- Se il percorso della DLL contiene caratteri speciali, conviene usare `LoadLibraryW` e scrivere nel processo una stringa wide (UTF-16).

### 32-bit vs 64-bit (architettura)
- Un processo 32-bit non può iniettare in un 64-bit e viceversa con questa tecnica.
- Devi sempre allineare: gioco, DLL e iniettore devono avere la stessa architettura.
- Visual Studio: “Win32” = 32-bit, “x64” = 64-bit. Verifica anche la cartella di output (`Debug` vs `x64\\Debug`).

### Requisiti e preparazione
- Windows con Windows SDK (Visual Studio consigliato).
- La tua DLL e l’iniettore DEVONO avere la stessa architettura del gioco:
  - Gioco 32-bit → DLL 32-bit + iniettore 32-bit (Win32)
  - Gioco 64-bit → DLL 64-bit + iniettore 64-bit (x64)
- Spesso serve eseguire l’iniettore come Amministratore (se il gioco è elevato).

### Compilazione con Visual Studio
1. Apri la soluzione e seleziona la piattaforma corretta (Win32 per 32-bit o x64 per 64-bit).
2. Compila `DLL_Injector` in Debug o Release.
3. Compila anche la tua DLL (es. `Wesnoth_CodeCaveDLL`) con la stessa piattaforma.

Percorsi di output tipici:
- Win32 (32-bit): `...\\ProgettoDLL\\Debug\\NomeDLL.dll`
- x64 (64-bit): `...\\ProgettoDLL\\x64\\Debug\\NomeDLL.dll`

Usa “Copia come percorso” sull’eseguibile della DLL in Esplora File per evitare errori di path.

### Uso: il modo più semplice
1) Avvia il gioco (ad es. `wesnoth.exe`).
2) Apri un Prompt dei comandi nella cartella dell’iniettore (o usa il path assoluto).
3) Esegui l’iniettore con il percorso assoluto della DLL tra virgolette:

```bat
DLL_Injector.exe "C:\\Users\\tuoUtente\\source\\repos\\Wesnoth_CodeCaveDLL\\Debug\\Wesnoth_CodeCaveDLL.dll"
```

Se non passi un argomento, l’iniettore usa il percorso hard-coded nel sorgente (`dll_path`). È consigliato passarlo da riga di comando.

### Cosa vedrai in console e come leggerlo
Esempio di output (semplificato):

```
[Injector] Target: wesnoth.exe
[Injector] DLL: C:\\...\\Wesnoth_CodeCaveDLL.dll
[Injector] PID trovato: 1234
[Injector] Memoria remota allocata a: 0x12345678 (size=...
[Injector] Percorso DLL scritto in memoria remota.
[Injector] Indirizzo LoadLibraryA: 0x...
[Injector] Thread remoto creato: 0x...
Remote thread exit code (HMODULE) = 0x5A0000
Fatto.
```

- Se l’HMODULE è diverso da `0x00000000`, la DLL è stata caricata correttamente nel gioco.
- Se è `0x00000000`, la DLL non è stata caricata (vedi Troubleshooting qui sotto).

### Come funziona davvero `CreateRemoteThread` qui
- Passiamo come funzione d’entrata l’indirizzo di `LoadLibraryA` nel modulo `kernel32.dll`.
- Come parametro, passiamo l’indirizzo nel target dove abbiamo scritto la stringa col percorso assoluto della DLL.
- Il thread remoto esegue `LoadLibraryA(parametro)`, il loader carica la DLL e ritorna l’`HMODULE`, che sarà il codice di uscita del thread.

### Cosa può fallire in `LoadLibraryA`
- Il file non esiste o il path non è accessibile dal processo target.
- Dipendenze mancanti: la tua DLL importa altre DLL non presenti nel `PATH` del processo target.
- Architettura non compatibile (DLL 64-bit in processo 32-bit o viceversa).
- Blocchi di sicurezza (AV/EDR) che impediscono il caricamento/iniezione.
- `DllMain` della tua DLL ritorna `FALSE` o fa crash (es. perché fa I/O complessi in `DLL_PROCESS_ATTACH`).

### Regole d’oro per `DllMain`
- Mantieni `DllMain` leggero: niente operazioni bloccanti, niente thread join, niente sincronizzazioni complicate.
- Se devi fare lavoro “pesante”, crea un thread separato e fallo lì.
- Per debug iniziale, un `MessageBoxA` in `DLL_PROCESS_ATTACH` è utilissimo per capire se la DLL si carica davvero.

### Troubleshooting (problemi comuni)
1) “Percorso DLL non valido o inesistente”
   - Controlla di non confondere Win32 con x64 nei percorsi (`Debug` vs `x64\\Debug`).
   - Usa “Copia come percorso” sulla DLL appena compilata.
2) “Processo not found / non trovato entro il tempo limite”
   - Il gioco non è avviato? Il nome del processo è diverso? Cambia `target` nel codice o passa un’opzione (vedi personalizzazioni).
3) `HMODULE = 0x00000000`
   - Dipendenze della DLL mancanti (la tua DLL importa altre DLL non presenti). Prova a copiare la DLL nella cartella del gioco.
   - Path non ASCII con `LoadLibraryA` (usa solo ASCII oppure implementa variante `LoadLibraryW`).
   - `DllMain` della tua DLL ritorna `FALSE` o va in crash: aggiungi log/MessageBox in `DLL_PROCESS_ATTACH` per verificare.
   - Antivirus/EDR che blocca `CreateRemoteThread` o l’injection.
4) “La DLL si carica ma in gioco non succede nulla”
   - Quasi sempre offset/codecave non corrispondono alla tua versione del gioco. Devi ri-trovarli con un debugger/cheat engine.
   - Assicurati che la tua DLL ripristini correttamente le istruzioni originali e usi `FlushInstructionCache` dopo le patch.

### Personalizzazioni utili
- Cambiare il nome del processo target (`const wchar_t* target = L"notepad.exe";`).
- Aggiungere il supporto a `LoadLibraryW` scrivendo una stringa wide nel processo e risolvendo `LoadLibraryW`.
- Aggiungere opzioni da riga di comando: `DLL_Injector.exe <percorsoDLL> [nomeProcesso]`.

### Nota legale
Questo codice è per studio e ricerca. Usalo responsabilmente e solo dove consentito. Gli autori non sono responsabili di eventuali abusi.



