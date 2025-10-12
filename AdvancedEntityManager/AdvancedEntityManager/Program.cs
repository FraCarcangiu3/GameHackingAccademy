using Swed32;
using AdvancedEntityManager.Entity_Handling;
using AdvancedEntityManager.Entity_Structures;
using AdvancedEntityManager.Offset_Scanner;

Console.WriteLine("🚀 AdvancedEntityManager - Sistema Avanzato con Offset Dinamici");
Console.WriteLine(new string('=', 60));

// 1. Connetti al gioco
Swed swed = new Swed("ac_client");
if (!swed.IsValid())
{
    Console.WriteLine("❌ Errore: Impossibile connettersi al gioco Assault Cube!");
    Console.WriteLine("Assicurati che il gioco sia in esecuzione.");
    return;
}

Console.WriteLine("✅ Connesso al gioco Assault Cube!");

// 2. Ottieni l'indirizzo base del modulo
IntPtr moduleBase = swed.GetModuleBase(".exe");
Console.WriteLine($"📍 Indirizzo base del modulo: {moduleBase.ToString("X")}");

// 3. Avvia la scansione dinamica degli offset
Console.WriteLine("\n🔍 Avvio scansione dinamica degli offset...");
DynamicOffsetScanner scanner = new DynamicOffsetScanner(swed, moduleBase);
GameOffsets foundOffsets = scanner.FindAllOffsets();

// 4. Valida gli offset trovati
if (scanner.ValidateAllOffsets(foundOffsets))
{
    Console.WriteLine("\n📋 Offset trovati dinamicamente:");
    foundOffsets.PrintOffsets();
    
    // 5. Crea il manager avanzato con gli offset trovati
    ACAdvancedEntityManager entityManager = new ACAdvancedEntityManager(swed, moduleBase, foundOffsets);
    
    // 6. Aggiorna le entità
    Console.WriteLine("\n🔄 Aggiornamento delle entità...");
    entityManager.UpdateEntities();
    entityManager.UpdateLocalPlayer();
    
    // 7. Mostra riepilogo
    entityManager.PrintEntitySummary();
    
    // 8. Mostra entità dettagliate
    entityManager.PrintDetailedEntities();
    
    // 9. Analisi avanzata
    Console.WriteLine("\n🔍 Analisi Avanzata:");
    
    var aliveEntities = entityManager.GetAliveEntities();
    Console.WriteLine($"💚 Entità vive: {aliveEntities.Count}");
    
    var shootingEntities = entityManager.GetShootingEntities();
    Console.WriteLine($"🔫 Entità che stanno sparando: {shootingEntities.Count}");
    
    var closestEnemy = entityManager.GetClosestEnemy();
    if (closestEnemy != null)
    {
        Console.WriteLine($"🎯 Nemico più vicino: {closestEnemy.baseAddress.ToString("X")} (Distanza: {closestEnemy.distance:F2})");
    }
    
    // 10. Controllo entità sospette
    var suspiciousEntities = entityManager.FindSuspiciousEntities();
    if (suspiciousEntities.Count > 0)
    {
        Console.WriteLine($"⚠️ Entità sospette trovate: {suspiciousEntities.Count}");
        foreach (var entity in suspiciousEntities)
        {
            Console.WriteLine($"   🚨 Entità sospetta: {entity.baseAddress.ToString("X")}");
        }
    }
    else
    {
        Console.WriteLine("✅ Nessuna entità sospetta trovata");
    }
    
    Console.WriteLine("\n🎉 AdvancedEntityManager configurato con successo!");
    Console.WriteLine("💡 Questo sistema ha trovato automaticamente tutti gli offset!");
}
else
{
    Console.WriteLine("❌ Validazione degli offset fallita!");
    Console.WriteLine("💡 Prova a riavviare Assault Cube e riprova");
}

Console.WriteLine("\nPremi INVIO per uscire...");
Console.ReadLine();
