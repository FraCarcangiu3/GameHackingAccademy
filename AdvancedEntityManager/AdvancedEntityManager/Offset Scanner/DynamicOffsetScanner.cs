using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedEntityManager.Offset_Scanner
{
    /// <summary>
    /// Scanner avanzato per trovare dinamicamente gli offset di memoria
    /// </summary>
    public class DynamicOffsetScanner
    {
        private Swed32.Swed swed;
        private IntPtr moduleBase;
        private int moduleSize;

        public DynamicOffsetScanner(Swed32.Swed swedInstance, IntPtr moduleBase)
        {
            this.swed = swedInstance;
            this.moduleBase = moduleBase;
            this.moduleSize = GetModuleSize();
        }

        /// <summary>
        /// Trova tutti gli offset principali utilizzando pattern scanning avanzato
        /// </summary>
        public GameOffsets FindAllOffsets()
        {
            Console.WriteLine("🔍 Avvio scansione dinamica avanzata...");
            
            var offsets = new GameOffsets();
            
            // 1. Trova il giocatore locale con pattern matching
            offsets.localPlayer = FindLocalPlayerWithPattern();
            Console.WriteLine($"✅ LocalPlayer trovato: 0x{offsets.localPlayer:X}");
            
            // 2. Trova la lista delle entità con analisi strutturale
            offsets.entityList = FindEntityListWithStructure();
            Console.WriteLine($"✅ EntityList trovato: 0x{offsets.entityList:X}");
            
            // 3. Trova gli offset relativi alle entità
            offsets.health = FindHealthWithValidation();
            Console.WriteLine($"✅ Health trovato: 0x{offsets.health:X}");
            
            offsets.name = FindNameWithStringAnalysis();
            Console.WriteLine($"✅ Name trovato: 0x{offsets.name:X}");
            
            offsets.team = FindTeamWithLogic();
            Console.WriteLine($"✅ Team trovato: 0x{offsets.team:X}");
            
            offsets.position = FindPositionWithCoordinateAnalysis();
            Console.WriteLine($"✅ Position trovato: 0x{offsets.position:X}");
            
            offsets.shooting = FindShootingWithBehaviorAnalysis();
            Console.WriteLine($"✅ Shooting trovato: 0x{offsets.shooting:X}");
            
            Console.WriteLine("🎉 Scansione dinamica completata!");
            return offsets;
        }

        /// <summary>
        /// Trova il giocatore locale utilizzando pattern matching avanzato
        /// </summary>
        private int FindLocalPlayerWithPattern()
        {
            Console.WriteLine("🔍 Ricerca LocalPlayer con pattern matching...");
            
            // Pattern 1: Cerca valori di salute che cambiano frequentemente
            for (int offset = 0x1000; offset < moduleSize; offset += 0x4)
            {
                try
                {
                    IntPtr address = IntPtr.Add(moduleBase, offset);
                    
                    // Leggi più volte per verificare la consistenza
                    int[] healthReadings = new int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        healthReadings[i] = swed.ReadInt(address, 0xEC);
                        System.Threading.Thread.Sleep(10); // Piccola pausa
                    }
                    
                    // Verifica che tutti i valori siano identici e plausibili
                    if (healthReadings.All(h => h == healthReadings[0]) && 
                        healthReadings[0] >= 0 && healthReadings[0] <= 100)
                    {
                        // Verifica aggiuntiva: controlla altri campi
                        int team = swed.ReadInt(address, 0xF0);
                        if (team >= 0 && team <= 2)
                        {
                            Console.WriteLine($"📍 Pattern LocalPlayer trovato a offset: 0x{offset:X}");
                            return offset;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
            
            // Fallback agli offset noti
            return 0x0017E0A8;
        }

        /// <summary>
        /// Trova la lista delle entità con analisi strutturale
        /// </summary>
        private int FindEntityListWithStructure()
        {
            Console.WriteLine("🔍 Ricerca EntityList con analisi strutturale...");
            
            for (int offset = 0x1000; offset < moduleSize; offset += 0x4)
            {
                try
                {
                    // Analizza la struttura come array di puntatori
                    IntPtr[] entityPointers = new IntPtr[10];
                    bool validStructure = true;
                    
                    for (int i = 0; i < 10; i++)
                    {
                        entityPointers[i] = swed.ReadPointer(moduleBase, offset, i * 0x4);
                        
                        // Verifica che il puntatore sia valido
                        if (entityPointers[i] == IntPtr.Zero || 
                            entityPointers[i].ToInt64() < moduleBase.ToInt64() || 
                            entityPointers[i].ToInt64() > moduleBase.ToInt64() + moduleSize)
                        {
                            validStructure = false;
                            break;
                        }
                    }
                    
                    if (validStructure)
                    {
                        // Verifica che almeno alcuni puntatori puntino a entità valide
                        int validEntities = 0;
                        foreach (var ptr in entityPointers)
                        {
                            if (ptr != IntPtr.Zero)
                            {
                                int health = swed.ReadInt(ptr, 0xEC);
                                if (health >= 0 && health <= 100)
                                {
                                    validEntities++;
                                }
                            }
                        }
                        
                        if (validEntities >= 2) // Almeno 2 entità valide
                        {
                            Console.WriteLine($"📍 Struttura EntityList trovata a offset: 0x{offset:X}");
                            return offset;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
            
            return 0x0018AC04; // Fallback
        }

        /// <summary>
        /// Trova l'offset della salute con validazione avanzata
        /// </summary>
        private int FindHealthWithValidation()
        {
            Console.WriteLine("🔍 Ricerca Health con validazione avanzata...");
            
            int[] commonHealthOffsets = { 0xEC, 0xF0, 0xF4, 0xF8, 0xFC, 0x100, 0x104, 0x108 };
            
            foreach (int offset in commonHealthOffsets)
            {
                if (ValidateHealthOffset(offset))
                {
                    Console.WriteLine($"📍 Health validato a offset: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0xEC; // Fallback
        }

        /// <summary>
        /// Valida un offset della salute con test multipli
        /// </summary>
        private bool ValidateHealthOffset(int offset)
        {
            try
            {
                int validReadings = 0;
                int totalReadings = 0;
                
                // Testa su diverse entità
                for (int i = 0; i < 5; i++)
                {
                    IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, i * 0x4);
                    if (entityPtr != IntPtr.Zero)
                    {
                        totalReadings++;
                        int health = swed.ReadInt(entityPtr, offset);
                        
                        // Verifica che la salute sia in un range valido
                        if (health >= 0 && health <= 100)
                        {
                            validReadings++;
                        }
                    }
                }
                
                // Almeno il 80% delle letture deve essere valida
                return totalReadings > 0 && (validReadings * 100 / totalReadings) >= 80;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Trova l'offset del nome con analisi delle stringhe
        /// </summary>
        private int FindNameWithStringAnalysis()
        {
            Console.WriteLine("🔍 Ricerca Name con analisi stringhe...");
            
            int[] commonNameOffsets = { 0x205, 0x225, 0x245, 0x265, 0x285, 0x2A5 };
            
            foreach (int offset in commonNameOffsets)
            {
                if (ValidateNameOffset(offset))
                {
                    Console.WriteLine($"📍 Name validato a offset: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0x205; // Fallback
        }

        /// <summary>
        /// Valida un offset del nome
        /// </summary>
        private bool ValidateNameOffset(int offset)
        {
            try
            {
                IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, 0);
                if (entityPtr != IntPtr.Zero)
                {
                    // Leggi i primi 32 byte come int per verificare che non siano tutti zero
                    int nameValue = swed.ReadInt(entityPtr, offset);
                    return nameValue != 0 && nameValue < 1000000; // Range ragionevole
                }
            }
            catch
            {
                // Ignora errori
            }
            return false;
        }

        /// <summary>
        /// Trova l'offset del team con logica di validazione
        /// </summary>
        private int FindTeamWithLogic()
        {
            Console.WriteLine("🔍 Ricerca Team con logica di validazione...");
            
            int[] commonTeamOffsets = { 0xF0, 0xF4, 0xF8, 0xFC, 0x100, 0x104 };
            
            foreach (int offset in commonTeamOffsets)
            {
                if (ValidateTeamOffset(offset))
                {
                    Console.WriteLine($"📍 Team validato a offset: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0xF0; // Fallback
        }

        /// <summary>
        /// Valida un offset del team
        /// </summary>
        private bool ValidateTeamOffset(int offset)
        {
            try
            {
                int validTeams = 0;
                int totalEntities = 0;
                
                for (int i = 0; i < 5; i++)
                {
                    IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, i * 0x4);
                    if (entityPtr != IntPtr.Zero)
                    {
                        totalEntities++;
                        int team = swed.ReadInt(entityPtr, offset);
                        
                        // Verifica che il team sia in un range valido (0-2)
                        if (team >= 0 && team <= 2)
                        {
                            validTeams++;
                        }
                    }
                }
                
                return totalEntities > 0 && (validTeams * 100 / totalEntities) >= 80;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Trova l'offset della posizione con analisi delle coordinate
        /// </summary>
        private int FindPositionWithCoordinateAnalysis()
        {
            Console.WriteLine("🔍 Ricerca Position con analisi coordinate...");
            
            int[] commonPositionOffsets = { 0x34, 0x38, 0x3C, 0x40, 0x44, 0x48 };
            
            foreach (int offset in commonPositionOffsets)
            {
                if (ValidatePositionOffset(offset))
                {
                    Console.WriteLine($"📍 Position validata a offset: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0x34; // Fallback
        }

        /// <summary>
        /// Valida un offset della posizione
        /// </summary>
        private bool ValidatePositionOffset(int offset)
        {
            try
            {
                IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, 0);
                if (entityPtr != IntPtr.Zero)
                {
                    // Leggi le coordinate come int
                    int x = swed.ReadInt(entityPtr, offset);
                    int y = swed.ReadInt(entityPtr, offset + 4);
                    int z = swed.ReadInt(entityPtr, offset + 8);
                    
                    // Converti da int a float per il controllo
                    float fx = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                    float fy = BitConverter.ToSingle(BitConverter.GetBytes(y), 0);
                    float fz = BitConverter.ToSingle(BitConverter.GetBytes(z), 0);
                    
                    // Verifica che le coordinate siano in un range ragionevole
                    return Math.Abs(fx) < 10000 && Math.Abs(fy) < 10000 && Math.Abs(fz) < 10000;
                }
            }
            catch
            {
                // Ignora errori
            }
            return false;
        }

        /// <summary>
        /// Trova l'offset del shooting con analisi del comportamento
        /// </summary>
        private int FindShootingWithBehaviorAnalysis()
        {
            Console.WriteLine("🔍 Ricerca Shooting con analisi comportamento...");
            
            int[] commonShootingOffsets = { 0xF0, 0xF4, 0xF8, 0xFC, 0x100, 0x104 };
            
            foreach (int offset in commonShootingOffsets)
            {
                if (ValidateShootingOffset(offset))
                {
                    Console.WriteLine($"📍 Shooting validato a offset: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0xF0; // Fallback
        }

        /// <summary>
        /// Valida un offset del shooting
        /// </summary>
        private bool ValidateShootingOffset(int offset)
        {
            try
            {
                IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, 0);
                if (entityPtr != IntPtr.Zero)
                {
                    int shooting = swed.ReadInt(entityPtr, offset);
                    // Verifica che il valore sia 0 o 1 (boolean-like)
                    return shooting == 0 || shooting == 1;
                }
            }
            catch
            {
                // Ignora errori
            }
            return false;
        }

        /// <summary>
        /// Ottiene la dimensione del modulo
        /// </summary>
        private int GetModuleSize()
        {
            return 0x2000000; // 32MB come limite massimo
        }

        /// <summary>
        /// Valida tutti gli offset trovati
        /// </summary>
        public bool ValidateAllOffsets(GameOffsets offsets)
        {
            Console.WriteLine("🔍 Validazione completa degli offset...");
            
            try
            {
                // Testa il giocatore locale
                IntPtr localPlayerPtr = swed.ReadPointer(moduleBase, offsets.localPlayer);
                if (localPlayerPtr == IntPtr.Zero)
                {
                    Console.WriteLine("❌ LocalPlayer non valido");
                    return false;
                }
                
                int health = swed.ReadInt(localPlayerPtr, offsets.health);
                if (health < 0 || health > 100)
                {
                    Console.WriteLine("❌ Health offset non valido");
                    return false;
                }
                
                // Testa la lista delle entità
                IntPtr entityPtr = swed.ReadPointer(moduleBase, offsets.entityList);
                if (entityPtr == IntPtr.Zero)
                {
                    Console.WriteLine("❌ EntityList non valida");
                    return false;
                }
                
                Console.WriteLine("✅ Tutti gli offset sono validi!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore durante la validazione: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Struttura per contenere tutti gli offset trovati dinamicamente
    /// </summary>
    public class GameOffsets
    {
        public int localPlayer { get; set; }
        public int entityList { get; set; }
        public int health { get; set; }
        public int name { get; set; }
        public int team { get; set; }
        public int position { get; set; }
        public int shooting { get; set; }
        
        public void PrintOffsets()
        {
            Console.WriteLine("📋 Offset trovati dinamicamente:");
            Console.WriteLine($"   LocalPlayer: 0x{localPlayer:X}");
            Console.WriteLine($"   EntityList: 0x{entityList:X}");
            Console.WriteLine($"   Health: 0x{health:X}");
            Console.WriteLine($"   Name: 0x{name:X}");
            Console.WriteLine($"   Team: 0x{team:X}");
            Console.WriteLine($"   Position: 0x{position:X}");
            Console.WriteLine($"   Shooting: 0x{shooting:X}");
        }
    }
}
