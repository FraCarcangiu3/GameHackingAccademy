using Swed32;
using NAudio.Wave;
using HitsoundHack;
using System.Diagnostics;

try
{
    // Verifica se il processo esiste prima di creare Swed
    Process[] processes = Process.GetProcessesByName("hl");
    if (processes.Length == 0)
    {
        Console.WriteLine("Errore: Il processo 'hl' (Half-Life) non è in esecuzione!");
        Console.WriteLine("Assicurati che Half-Life sia avviato prima di eseguire questo hack.");
        Console.WriteLine("Premi un tasto per uscire...");
        Console.ReadKey();
        return;
    }

    Console.WriteLine($"Processo Half-Life trovato! PID: {processes[0].Id}");
    Swed swed = new Swed("hl");

    // Verifica che i moduli necessari siano caricati
    IntPtr hwwModule = swed.GetModuleBase("hw.dll"); // Get the base address of hw.dll
    IntPtr client = swed.GetModuleBase("client.dll"); // Get the base address of client.dll
    
    if (hwwModule == IntPtr.Zero)
    {
        Console.WriteLine("Errore: Modulo hw.dll non trovato!");
        Console.WriteLine("Assicurati che Half-Life sia completamente caricato.");
        Console.WriteLine("Premi un tasto per uscire...");
        Console.ReadKey();
        return;
    }
    
    if (client == IntPtr.Zero)
    {
        Console.WriteLine("Errore: Modulo client.dll non trovato!");
        Console.WriteLine("Assicurati che Half-Life sia completamente caricato.");
        Console.WriteLine("Premi un tasto per uscire...");
        Console.ReadKey();
        return;
    }
    
    Console.WriteLine("Moduli caricati con successo!");
    Console.WriteLine($"hw.dll base: 0x{hwwModule.ToInt64():X}");
    Console.WriteLine($"client.dll base: 0x{client.ToInt64():X}");

List<Entity> entities = new List<Entity>(); // List to hold entities
Entity localPlayer = new Entity(); // Local player entity

PlaySound();

//main loop
while (true)
{
    IntPtr entityList = swed.ReadPointer(hwwModule, Offsets.entitylistAddress); // Read the entity list pointer
    localPlayer.Address = swed.ReadPointer(entityList, Offsets.localPlayer); // Read the local player address
    localPlayer.Shooting = swed.ReadInt(localPlayer.Address, Offsets.forceAttack); // Read the local player's shooting status

    for (int i = 1; i < 32; i++)
    {
        IntPtr currentEnt = swed.ReadPointer(entityList, Offsets.localPlayer + i * 0x4); // Read the current entity address
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
    try
    {
        string audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hitsounds", "classic_hurt.mp3");
        
        if (!File.Exists(audioPath))
        {
            Console.WriteLine($"File audio non trovato: {audioPath}");
            return;
        }

        using (var audioFile = new AudioFileReader(audioPath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(audioFile);
            outputDevice.Volume = 0.5f;
            outputDevice.Play();
            
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(1);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore durante la riproduzione del suono: {ex.Message}");
    }
}

