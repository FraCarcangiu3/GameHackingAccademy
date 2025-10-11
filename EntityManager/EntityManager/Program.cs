using Swed32;
using EntityManager;
using EntityManager.Entity_Handling;
using EntityManager.Entity_Structures;




Swed swed = new Swed("ac_client"); // create a new swed instance
IntPtr moduleBase = swed.GetModuleBase(".exe"); //when it is the main module (exe) we can create a new instance 

ACEntityManager aCEntityManager = new ACEntityManager(swed, moduleBase); // create a new entity manager instance

aCEntityManager.UpdateEntities();
aCEntityManager.UpdateLocalPlayer();

foreach(Entity entity in aCEntityManager.GetEntitites())
    {
    Console.WriteLine($"Base: {entity.baseAddress.ToString("X")} Health: {entity.health}");
}