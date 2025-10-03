/*
	Una DLL che reindirizza la funzione "Descrizione Terreno" di Wesnoth 1.14.9 a una funzione personalizzata che imposta l'oro del giocatore a 888.
	Questa funzione personalizzata poi ricrea la funzione "Descrizione Terreno" e restituisce l'esecuzione al programma.

	Questo viene fatto tramite l'uso di una codecave. Quando iniettata, la DLL modifica la funzione che mostra la
	descrizione del terreno e cambia il codice per saltare alla funzione codecave definita nella DLL. La funzione codecave
	salva poi i registri, imposta l'oro a 888, e poi ripristina le istruzioni originali modificate
	prima di ritornare al codice chiamante originale.

	Questo deve essere iniettato nel processo di Wesnoth per funzionare. Un modo per farlo è usare un injector di DLL.
	Un altro modo è abilitare AppInit_DLLs nel registro.

	Gli offset e la spiegazione della codecave sono trattati in https://gamehacking.academy/lesson/11 e https://gamehacking.academy/lesson/17
*/

#include <Windows.h>


DWORD* player_base;
DWORD* game_base;
DWORD* gold;
DWORD ret_address = 0xCCAF90;

// Handle dell'evento creato dall'inniettore (aperto con OpenEvent)
static HANDLE hInjectorEvent = NULL;
static HMODULE g_hModule = NULL;

// Dichiarazione forward
void SignalInjector();

// Codecave: esegue patch lato gioco e poi richiama SignalInjector
__declspec(naked) void codecave() {
    __asm { pushad }    // salva registri

    // codice C++ normale (puoi spostarlo in una funzione se preferisci)
    // Nota: non usare molte API complesse direttamente qui; qui chiami una funzione semplice.
    // Impostare l'oro
    //player_base = (DWORD*)0x037EEC88;
    player_base = (DWORD*)0x017EED18;
    game_base = (DWORD*)(*player_base + 0xA90);
    gold = (DWORD*)(*game_base + 4);
    *gold = 888;

    __asm { popad }     // ripristina registri

    // Chiamiamo una piccola funzione C che segnala l'event all'inniettore (se presente)
    SignalInjector();

    // Ripristiniamo le istruzioni originali (esempio) e saltiamo all'indirizzo di ritorno:
    __asm {
        mov eax, dword ptr ds : [ecx]
        lea esi, dword ptr ds : [esi]
        jmp dword ptr[ret_address]
    }
}

// Funzione che fa SetEvent sull'event aperto dall'InitThread
void SignalInjector() {
    if (hInjectorEvent) {
        SetEvent(hInjectorEvent);
    }
}

// Thread di inizializzazione: esegue VirtualProtect + patch e apre l'evento
DWORD WINAPI InitThread(LPVOID lpv) {
    // Apri l'event endpoint creato dall'inniettore (se esiste)
    hInjectorEvent = OpenEventA(EVENT_MODIFY_STATE, FALSE, "WesnothHookEvent");
    // NON è un errore se OpenEvent fallisce: l'inniettore potrebbe non averlo creato.

    // esegui la patch come prima
    DWORD old_protect;
    unsigned char* hook_location = (unsigned char*)0x00CCAF8A;

    if (VirtualProtect((void*)hook_location, 6, PAGE_EXECUTE_READWRITE, &old_protect)) {
        *hook_location = 0xE9;
        *(DWORD*)(hook_location + 1) = (DWORD)&codecave - ((DWORD)hook_location + 5);
        *(hook_location + 5) = 0x90;

        // (facoltativo) ripristina la protezione originale
        VirtualProtect((void*)hook_location, 6, old_protect, &old_protect);
    }

    // Thread può terminare: non deve rimanere bloccato in DllMain.
    return 0;
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved) {
    g_hModule = (HMODULE)hinstDLL;

    if (fdwReason == DLL_PROCESS_ATTACH) {
        // Evitiamo chiamate potenzialmente bloccanti in DllMain:
        // creiamo subito un thread che farà il resto.
        HANDLE h = CreateThread(NULL, 0, InitThread, NULL, 0, NULL);
        if (h) CloseHandle(h);
    }
    else if (fdwReason == DLL_PROCESS_DETACH) {
        // pulizia
        if (hInjectorEvent) { CloseHandle(hInjectorEvent); hInjectorEvent = NULL; }
    }
    return TRUE;
}