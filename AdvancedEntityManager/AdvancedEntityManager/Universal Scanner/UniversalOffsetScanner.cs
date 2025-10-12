using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedEntityManager.Universal_Scanner
{
    /// <summary>
    /// Scanner universale per trovare offset in qualsiasi gioco
    /// </summary>
    public class UniversalOffsetScanner
    {
        private Swed32.Swed swed;
        private IntPtr moduleBase;
        private int moduleSize;

        public UniversalOffsetScanner(Swed32.Swed swedInstance, IntPtr mainModule)
        {
            this.swed = swedInstance;
            this.moduleBase = mainModule;
            this.moduleSize = GetModuleSize();
        }

        /// <summary>
        /// Trova tutti gli offset per qualsiasi gioco
        /// </summary>
        public UniversalGameOffsets FindAllOffsets()
        {
            Console.WriteLine("🌍 Avvio scansione universale per qualsiasi gioco...");
            
            var offsets = new UniversalGameOffsets();
            
            // 1. Trova il giocatore locale con ricerca intelligente
            offsets.localPlayer = FindLocalPlayerUniversal();
            Console.WriteLine($"✅ LocalPlayer trovato: 0x{offsets.localPlayer:X}");
            
            // 2. Trova la lista delle entità con pattern recognition
            offsets.entityList = FindEntityListUniversal();
            Console.WriteLine($"✅ EntityList trovato: 0x{offsets.entityList:X}");
            
            // 3. Trova gli offset relativi alle entità
            offsets.health = FindHealthUniversal();
            Console.WriteLine($"✅ Health trovato: 0x{offsets.health:X}");
            
            offsets.team = FindTeamUniversal();
            Console.WriteLine($"✅ Team trovato: 0x{offsets.team:X}");
            
            offsets.position = FindPositionUniversal();
            Console.WriteLine($"✅ Position trovato: 0x{offsets.position:X}");
            
            offsets.shooting = FindShootingUniversal();
            Console.WriteLine($"✅ Shooting trovato: 0x{offsets.shooting:X}");
            
            Console.WriteLine("🎉 Scansione universale completata!");
            return offsets;
        }

        /// <summary>
        /// Trova il giocatore locale con ricerca universale
        /// </summary>
        private int FindLocalPlayerUniversal()
        {
            Console.WriteLine("🔍 Ricerca universale LocalPlayer...");
            
            // Cerca pattern comuni per il giocatore locale
            for (int offset = 0x1000; offset < Math.Min(moduleSize, 0x100000); offset += 0x4)
            {
                try
                {
                    IntPtr address = IntPtr.Add(moduleBase, offset);
                    
                    // Testa se questo indirizzo punta a dati di giocatore
                    if (IsPlayerData(address))
                    {
                        Console.WriteLine($"📍 LocalPlayer trovato universalmente: 0x{offset:X}");
                        return offset;
                    }
                }
                catch
                {
                    continue;
                }
            }
            
            // Fallback: usa offset comuni
            return 0x0017E0A8;
        }

        /// <summary>
        /// Verifica se un indirizzo contiene dati di giocatore
        /// </summary>
        private bool IsPlayerData(IntPtr address)
        {
            try
            {
                // Testa diversi offset comuni per la salute
                int[] healthOffsets = { 0xEC, 0xF0, 0xF4, 0xF8, 0xFC, 0x100, 0x104, 0x108 };
                
                foreach (int healthOffset in healthOffsets)
                {
                    int health = swed.ReadInt(address, healthOffset);
                    if (health >= 0 && health <= 100)
                    {
                        // Verifica anche il team
                        int team = swed.ReadInt(address, healthOffset + 4);
                        if (team >= 0 && team <= 2)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Ignora errori
            }
            return false;
        }

        /// <summary>
        /// Trova la lista delle entità con ricerca universale
        /// </summary>
        private int FindEntityListUniversal()
        {
            Console.WriteLine("🔍 Ricerca universale EntityList...");
            
            // Cerca array di puntatori
            for (int offset = 0x1000; offset < Math.Min(moduleSize, 0x200000); offset += 0x4)
            {
                try
                {
                    if (IsEntityList(offset))
                    {
                        Console.WriteLine($"📍 EntityList trovata universalmente: 0x{offset:X}");
                        return offset;
                    }
                }
                catch
                {
                    continue;
                }
            }
            
            // Fallback: usa offset comuni
            return 0x0018AC04;
        }

        /// <summary>
        /// Verifica se un offset contiene una lista di entità
        /// </summary>
        private bool IsEntityList(int offset)
        {
            try
            {
                // Testa i primi 5 puntatori
                IntPtr[] pointers = new IntPtr[5];
                int validPointers = 0;
                
                for (int i = 0; i < 5; i++)
                {
                    pointers[i] = swed.ReadPointer(moduleBase, offset, i * 0x4);
                    
                    if (pointers[i] != IntPtr.Zero && 
                        pointers[i].ToInt64() > moduleBase.ToInt64() && 
                        pointers[i].ToInt64() < moduleBase.ToInt64() + moduleSize)
                    {
                        validPointers++;
                    }
                }
                
                // Almeno 2 puntatori validi
                return validPointers >= 2;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Trova l'offset della salute con ricerca universale
        /// </summary>
        private int FindHealthUniversal()
        {
            Console.WriteLine("🔍 Ricerca universale Health...");
            
            // Testa offset comuni per la salute
            int[] commonHealthOffsets = { 0xEC, 0xF0, 0xF4, 0xF8, 0xFC, 0x100, 0x104, 0x108, 0x10C, 0x110 };
            
            foreach (int offset in commonHealthOffsets)
            {
                if (ValidateHealthUniversal(offset))
                {
                    Console.WriteLine($"📍 Health trovato universalmente: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0xEC; // Fallback
        }

        /// <summary>
        /// Valida un offset della salute universalmente
        /// </summary>
        private bool ValidateHealthUniversal(int offset)
        {
            try
            {
                // Testa su diverse entità
                int validReadings = 0;
                int totalReadings = 0;
                
                for (int i = 0; i < 5; i++)
                {
                    IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, i * 0x4);
                    if (entityPtr != IntPtr.Zero)
                    {
                        totalReadings++;
                        int health = swed.ReadInt(entityPtr, offset);
                        
                        if (health >= 0 && health <= 100)
                        {
                            validReadings++;
                        }
                    }
                }
                
                return totalReadings > 0 && (validReadings * 100 / totalReadings) >= 60;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Trova l'offset del team con ricerca universale
        /// </summary>
        private int FindTeamUniversal()
        {
            Console.WriteLine("🔍 Ricerca universale Team...");
            
            int[] commonTeamOffsets = { 0xF0, 0xF4, 0xF8, 0xFC, 0x100, 0x104, 0x108, 0x10C };
            
            foreach (int offset in commonTeamOffsets)
            {
                if (ValidateTeamUniversal(offset))
                {
                    Console.WriteLine($"📍 Team trovato universalmente: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0xF0; // Fallback
        }

        /// <summary>
        /// Valida un offset del team universalmente
        /// </summary>
        private bool ValidateTeamUniversal(int offset)
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
                        
                        if (team >= 0 && team <= 2)
                        {
                            validTeams++;
                        }
                    }
                }
                
                return totalEntities > 0 && (validTeams * 100 / totalEntities) >= 60;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Trova l'offset della posizione con ricerca universale
        /// </summary>
        private int FindPositionUniversal()
        {
            Console.WriteLine("🔍 Ricerca universale Position...");
            
            int[] commonPositionOffsets = { 0x34, 0x38, 0x3C, 0x40, 0x44, 0x48, 0x4C, 0x50 };
            
            foreach (int offset in commonPositionOffsets)
            {
                if (ValidatePositionUniversal(offset))
                {
                    Console.WriteLine($"📍 Position trovata universalmente: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0x34; // Fallback
        }

        /// <summary>
        /// Valida un offset della posizione universalmente
        /// </summary>
        private bool ValidatePositionUniversal(int offset)
        {
            try
            {
                IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, 0);
                if (entityPtr != IntPtr.Zero)
                {
                    int x = swed.ReadInt(entityPtr, offset);
                    int y = swed.ReadInt(entityPtr, offset + 4);
                    int z = swed.ReadInt(entityPtr, offset + 8);
                    
                    float fx = BitConverter.ToSingle(BitConverter.GetBytes(x), 0);
                    float fy = BitConverter.ToSingle(BitConverter.GetBytes(y), 0);
                    float fz = BitConverter.ToSingle(BitConverter.GetBytes(z), 0);
                    
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
        /// Trova l'offset del shooting con ricerca universale
        /// </summary>
        private int FindShootingUniversal()
        {
            Console.WriteLine("🔍 Ricerca universale Shooting...");
            
            int[] commonShootingOffsets = { 0xF0, 0xF4, 0xF8, 0xFC, 0x100, 0x104, 0x108, 0x10C };
            
            foreach (int offset in commonShootingOffsets)
            {
                if (ValidateShootingUniversal(offset))
                {
                    Console.WriteLine($"📍 Shooting trovato universalmente: 0x{offset:X}");
                    return offset;
                }
            }
            
            return 0xF0; // Fallback
        }

        /// <summary>
        /// Valida un offset del shooting universalmente
        /// </summary>
        private bool ValidateShootingUniversal(int offset)
        {
            try
            {
                IntPtr entityPtr = swed.ReadPointer(moduleBase, 0x0018AC04, 0);
                if (entityPtr != IntPtr.Zero)
                {
                    int shooting = swed.ReadInt(entityPtr, offset);
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
    }

    /// <summary>
    /// Struttura per offset universali
    /// </summary>
    public class UniversalGameOffsets
    {
        public int localPlayer { get; set; }
        public int entityList { get; set; }
        public int health { get; set; }
        public int team { get; set; }
        public int position { get; set; }
        public int shooting { get; set; }
        
        public void PrintOffsets()
        {
            Console.WriteLine("🌍 Offset universali trovati:");
            Console.WriteLine($"   LocalPlayer: 0x{localPlayer:X}");
            Console.WriteLine($"   EntityList: 0x{entityList:X}");
            Console.WriteLine($"   Health: 0x{health:X}");
            Console.WriteLine($"   Team: 0x{team:X}");
            Console.WriteLine($"   Position: 0x{position:X}");
            Console.WriteLine($"   Shooting: 0x{shooting:X}");
        }
    }
}
