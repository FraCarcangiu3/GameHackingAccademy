## Wesnoth_CodeCaveDLL – Guida per principianti

Questa è una DLL di esempio che applica un hook (tramite codecave) al gioco The Battle for Wesnoth e, quando scatta l’evento desiderato, imposta l’oro del giocatore a 888. È pensata a scopo didattico per capire come funzionano patch in memoria, salti relativi e gestione di `DllMain`.

Attenzione: usa questa DLL solo in contesti legali e dove hai i permessi. Evita software protetto da anti‑cheat o policy restrittive.

### Cosa fa, in breve
- All’avvio (in `DllMain` con `DLL_PROCESS_ATTACH`) crea un thread di inizializzazione.
- Il thread chiama `VirtualProtect` per rendere scrivibile un punto del codice del gioco (l’offset `hook_location`).
- Scrive un’istruzione `JMP rel32` per saltare alla nostra funzione `codecave` dentro la DLL.
- La `codecave` salva i registri, calcola l’indirizzo dell’oro del giocatore e lo imposta a 888.
- Ripristina i registri, ricrea le istruzioni originali rimosse, e salta al `ret_address` per tornare al flusso del gioco.
- Facoltativo: visualizza un `MessageBoxA` quando l’hook è stato applicato (utile per debug).

### File e simboli importanti
- `Wesnoth_CodeCaveDLL/main.cpp`
  - `InitThread(...)`: fa la patch (JMP + NOP) al `hook_location` e ripristina le protezioni della pagina; esegue `FlushInstructionCache`.
  - `codecave()`: funzione naked assembly/c++ che esegue la logica (imposta 888) e poi salta a `ret_address`.
  - `ret_address`: indirizzo nel codice di Wesnoth dove riprendere l’esecuzione dopo la nostra codecave.
  - `hook_location`: indirizzo dove iniettare il salto relativo a `codecave` (sovrascrive 5 byte, + eventuale NOP).
  - `DllMain(...)`: crea un thread e fa pulizia di handle quando la DLL viene scaricata.

### Architettura e versioni del gioco
- Questa DLL è pensata per una specifica build 32‑bit del gioco. Gli indirizzi:
  - `hook_location = 0x00CCAF8A`
  - `ret_address   = 0x00CCAF90`
- Se la tua versione del gioco differisce (eseguibile diverso, patch, ASLR, ecc.), questi offset potrebbero non essere validi. In tal caso devi:
  1) Individuare le istruzioni che vuoi agganciare con un disassembler/debugger (x86).
  2) Ricalcolare `hook_location` e `ret_address`.
  3) Aggiornare il codice e ricompilare.

### Perché serve `VirtualProtect` e `FlushInstructionCache`
- Il codice del gioco è in pagine di memoria eseguibili/sola lettura. Per scriverci sopra usiamo `VirtualProtect` per impostare `PAGE_EXECUTE_READWRITE` temporaneamente.
- Dopo aver scritto nuove istruzioni (`JMP` + `NOP`), chiamiamo `FlushInstructionCache` per garantire che la CPU veda il codice aggiornato.

### Come costruire (Visual Studio)
1) Apri la soluzione e imposta la piattaforma corretta (Win32 per 32‑bit).
2) Compila la configurazione `Debug` o `Release`.
3) Il risultato atteso è `Wesnoth_CodeCaveDLL.dll` in `Debug\` (oppure `Release\`).

### Come usare con l’iniettore
1) Avvia Wesnoth.
2) Esegui l’iniettore (progetto `DLL_Injector`) passando il percorso assoluto della DLL, ad esempio:

```bat
DLL_Injector.exe "C:\\Users\\tuoUtente\\source\\repos\\Wesnoth_CodeCaveDLL\\Debug\\Wesnoth_CodeCaveDLL.dll"
```

3) Se la patch va a buon fine, vedrai (in debug) un messaggio “Hook applicato: codecave installata.”. In gioco, quando l’istruzione agganciata viene eseguita, l’oro dovrebbe diventare 888.

### Regole d’oro per DllMain e stabilità
- Non fare operazioni bloccanti in `DllMain`. Crea un thread e fai il lavoro lì (come nell’esempio).
- Controlla sempre i valori di ritorno di `VirtualProtect`, `OpenEvent`, ecc.
- Se qualcosa va storto, prova ad aggiungere `MessageBoxA` o log su file per capire dove si ferma.

### Troubleshooting
- **Nessun effetto in gioco**: gli offset non corrispondono alla tua versione. Ricalcolali con un debugger.
- **Crash all’avvio della DLL**: la `codecave` deve preservare i registri (`pushad`/`popad`) e ricreare correttamente le istruzioni originali.
- **`VirtualProtect` fallisce**: il puntatore a `hook_location` potrebbe essere errato, oppure non hai i diritti necessari.
- **La DLL si carica ma non vedi il MessageBox**: togli il MessageBox in ambienti con restrizioni, o aggiungi logging alternativo.

### Nota legale
Questa DLL è fornita per scopi educativi. Usala responsabilmente e solo dove consentito. Gli autori non sono responsabili di un uso improprio.


