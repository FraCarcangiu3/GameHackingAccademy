using Swed32;
using NAudio.Wave;
using HitsoundHack;
using System.Diagnostics;

try
{
    // Verifica se il processo Assault Cube esiste
    Process[] processes = Process.GetProcessesByName("ac_client");
    if (processes.Length == 0)
    {
        Console.WriteLine("Errore: Il processo 'ac_client' (Assault Cube) non è in esecuzione!");
        Console.WriteLine("Assicurati che Assault Cube sia avviato prima di eseguire questo hack.");
        Console.WriteLine("Premi un tasto per uscire...");
        Console.ReadKey();
        return;
    }

    Console.WriteLine($"Processo Assault Cube trovato! PID: {processes[0].Id}");
    Swed swed = new Swed("ac_client"); // Assault Cube process

    IntPtr moduleBase = swed.GetModuleBase(".exe"); // Get the base address of ac_client.exe
    
    if (moduleBase == IntPtr.Zero)
    {
        Console.WriteLine("Errore: Modulo principale di Assault Cube non trovato!");
        Console.WriteLine("Assicurati che Assault Cube sia completamente caricato.");
        Console.WriteLine("Premi un tasto per uscire...");
        Console.ReadKey();
        return;
    }
    
    Console.WriteLine($"Modulo principale caricato: 0x{moduleBase.ToInt64():X}");

List<Entity> entities = new List<Entity>(); // List to hold entities
Entity localPlayer = new Entity(); // Local player entity

PlaySound();

//main loop
while (true)
{
    // Per Assault Cube, leggiamo il local player direttamente dal modulo base
    localPlayer.Address = swed.ReadPointer(moduleBase, Offsets.localPlayer); // Read the local player address
    localPlayer.Shooting = swed.ReadInt(localPlayer.Address, Offsets.forceAttack); // Read the local player's shooting status

    for (int i = 0; i < 10; i++) // Assault Cube ha max 10 entità
    {
        // Per Assault Cube, leggiamo le entità direttamente dal modulo base
        IntPtr currentEnt = swed.ReadPointer(moduleBase, Offsets.entityList, i * 0x4); // Read the current entity address
        if (currentEnt == IntPtr.Zero) continue; // Skip if the entity address is null

        int health = swed.ReadInt(currentEnt, Offsets.health); // Read the entity's health
        if (health < 101)
        {
            Entity entity = new Entity();
            entity.Address = currentEnt;
            entity.Health = health;

            // check if there is an existing entity
            var existingEntity = entities.FirstOrDefault(e => e.Address == entity.Address);
            if (existingEntity != null)
            { //if there is an entity already

                //check if the health is lower, and that we are shooting 
                if (health < existingEntity.Health && localPlayer.Shooting == 5)
                { // if the health is lower than the previous one
                    //create a new thread to play the sound
                    Thread audioThread = new Thread(PlaySound) { IsBackground = true };
                    audioThread.Start(); // start the thread

                }
                existingEntity.Health = health; // update existing health
            }
            else entities.Add(entity); // add new entity to the list if not existing
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Errore durante l'esecuzione: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Console.WriteLine("Premi un tasto per uscire...");
    Console.ReadKey();
}

void PlaySound()
{
    string directory = AppDomain.CurrentDomain.BaseDirectory; // Get the base directory of the application

    using (var audioFile = new AudioFileReader(@"../../../Hitsounds/classic_hurt.mp3")) // Load the hitsound.wav file
        using (var outputDevice = new WaveOutEvent()) // Create a new output device
    {
        outputDevice.Init(audioFile); // Initialize the output device with the audio file
        outputDevice.Volume = 0.5f; // Set volume to 50%
        outputDevice.Play(); // Play the sound
        while (outputDevice.PlaybackState == PlaybackState.Playing) // Wait for the sound to finish playing
        {
            Thread.Sleep(1); // Sleep for a short duration to avoid busy-waiting
        }
    }
}

