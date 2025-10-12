using Swed32;
using NAudio.Wave;
using HitsoundHack;


//Swed  swed = new Swed("hl");

//IntPtr hwwModule = swed.GetModuleBase("hw.dll"); // Get the base address of hw.dll
//IntPtr client = swed.GetModuleBase("client.dll"); // Get the base address of client.dll

List<Entity> entities = new List<Entity>(); // List to hold entities
Entity localPlayer = new Entity(); // Local player entity

PlaySound();

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

