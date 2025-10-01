/*
    Un'applicazione console che imposta l'oro del giocatore in Wesnoth 1.14.9 al valore 555 quando viene eseguita.
    Utilizza ReadProcessMemory e WriteProcessMemory per ottenere questo risultato.
    L’indirizzo 0x017EED18 rappresenta il puntatore base del giocatore in Wesnoth.
    La ricerca dell’indirizzo e degli offset è spiegata in:
    https://gamehacking.academy/lesson/13

    Questo programma deve essere eseguito come amministratore.

    Il codice è trattato nel laboratorio a:
    https://gamehacking.academy/lesson/15
*/

// FindWindow, GetWindowThreadProcessId, OpenProcess, ReadProcessMemory e WriteProcessMemory 
// sono tutte contenute in windows.h
#include <Windows.h>

int main(int argc, char** argv) {
    /*
        Per usare ReadProcessMemory e WriteProcessMemory, abbiamo bisogno di un handle al processo di Wesnoth.

        Per ottenere questo handle, ci serve un process id.
        Il modo più veloce per ottenere il process id di un determinato processo
        è usare GetWindowThreadProcessId.

        GetWindowThreadProcessId richiede un handle di finestra (diverso da un handle di processo).
        Per ottenere questo handle di finestra, usiamo FindWindow.
    */

    // Trova la finestra di Wesnoth. A seconda delle impostazioni della lingua, 
    // il titolo potrebbe essere diverso.
    HWND wesnoth_window = FindWindow(NULL, L"The Battle for Wesnoth - 1.14.9");

    // Ottiene il process id per il processo di Wesnoth. 
    // GetWindowThreadProcessId non restituisce direttamente un process id, 
    // ma riempie una variabile fornita con il suo valore, da cui l’uso di &.
    DWORD process_id = 0;
    GetWindowThreadProcessId(wesnoth_window, &process_id);

    // Apre il processo Wesnoth. PROCESS_ALL_ACCESS significa che possiamo sia leggere 
    // che scrivere nel processo. Tuttavia, significa anche che questo programma 
    // deve essere eseguito come amministratore per funzionare.
    HANDLE wesnoth_process = OpenProcess(PROCESS_ALL_ACCESS, true, process_id);

    // Legge il valore a 0x017EED18 e lo inserisce nella variabile gold_value.
    DWORD gold_value = 0;
    DWORD bytes_read = 0;
    ReadProcessMemory(wesnoth_process, (void*)0x037EEC88, &gold_value, 4, &bytes_read);

    // Aggiunge 0xA90 al valore letto nello step precedente e poi legge il valore al nuovo indirizzo. 
    // Questi offset sono spiegati in https://gamehacking.academy/lesson/13
    gold_value += 0xA90;
    ReadProcessMemory(wesnoth_process, (void*)gold_value, &gold_value, 4, &bytes_read);

    // Aggiunge 4 a gold_value, che a questo punto punta all’indirizzo corrente dell’oro del giocatore.
    // Scrive il valore di new_gold_value (555) in questo indirizzo.
    gold_value += 4;
    DWORD new_gold_value = 555;
    DWORD bytes_written = 0;
    WriteProcessMemory(wesnoth_process, (void*)gold_value, &new_gold_value, 4, &bytes_written);

    return 0;
}