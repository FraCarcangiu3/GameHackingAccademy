using Swed32;
using NAudio.Wave;
using HitsoundHack;


Swed  swed = new Swed("hl");

IntPtr hwwModule = swed.GetModuleBase("hw.dll"); // Get the base address of hw.dll
IntPtr client = swed.GetModuleBase("client.dll"); // Get the base address of client.dll

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

