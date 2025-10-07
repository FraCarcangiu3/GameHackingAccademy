/*
Un wallhack per Urban Terror 4.3.4 che rivela le entità attraverso i muri disabilitando il depth-testing.

Questo viene fatto modificando il flag di rendering di ogni entità, che è responsabile di determinare come l'entità deve essere renderizzata.
Impostando questo valore sul valore di gioco corrispondente al depth testing disabilitato (0xD), le entità verranno disegnate indipendentemente dal fatto che dovrebbero essere visibili o meno.
Il codice agganciato è un'istruzione mov che avviene dopo che ebx è stato caricato con una struttura entità valida.

Questo deve essere iniettato nel processo di Urban Terror per funzionare. Un modo per farlo è usare un injector di DLL.
Un altro modo è abilitare AppInit_DLLs nel registro.

Gli offset e il metodo per scoprirli sono discussi nell'articolo su: https://gamehacking.academy/lesson/22
*/
#include <Windows.h>

DWORD ret_address = 0x0052D303;

// La nostra codecave a cui l'esecuzione del programma salterà. L'attributo declspec naked dice al compilatore di non aggiungere
// header di funzione attorno al codice assemblato
__declspec(naked) void codecave() {
	// I blocchi Asm permettono di scrivere puro assembly
	// In questo caso li usiamo per salvare tutti i registri
	// Poi impostiamo il valore di render dell'entità a [ebx+4] su depth testing disabilitato (0xD)
	// Poi ripristiniamo i registri, ricreiamo l'istruzione originale, e saltiamo indietro al codice del programma
	__asm {
		pushad
		mov dword ptr ds : [ebx + 4] , 0xD
		popad
		mov dword ptr ds : [0x102AE98] , ebx

		jmp ret_address
	}
}

// Quando la nostra DLL viene allegata, rimuovi la protezione dalla memoria nel punto in cui vogliamo scrivere
// Poi imposta il primo opcode a E9, ossia un jump
// Calcola la posizione usando la formula: new_location - original_location + 5
// Infine, poiché le istruzioni originali erano in totale 6 byte, riempi con NOP l'ultimo byte rimanente
BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved) {
	DWORD old_protect;
	unsigned char* hook_location = (unsigned char*)0x0052D2FD;

	if (fdwReason == DLL_PROCESS_ATTACH) {
		VirtualProtect((void*)hook_location, 5, PAGE_EXECUTE_READWRITE, &old_protect);
		*hook_location = 0xE9;
		*(DWORD*)(hook_location + 1) = (DWORD)&codecave - ((DWORD)hook_location + 5);
		*(hook_location + 5) = 0x90;
	}

	return true;
}