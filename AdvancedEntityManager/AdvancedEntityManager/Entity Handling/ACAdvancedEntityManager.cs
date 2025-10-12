using AdvancedEntityManager.Entity_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Swed32;
using AdvancedEntityManager.Offset_Scanner;

namespace AdvancedEntityManager.Entity_Handling
{
    /// <summary>
    /// Implementazione avanzata per Assault Cube con offset dinamici
    /// </summary>
    public class ACAdvancedEntityManager : AdvancedEntityManager
    {
        private Swed swed;
        private IntPtr mainModule;
        private GameOffsets offsets;

        public ACAdvancedEntityManager(Swed swedInstance, IntPtr mainModule, GameOffsets gameOffsets)
        {
            this.swed = swedInstance;
            this.mainModule = mainModule;
            this.offsets = gameOffsets;
        }

        public override void UpdateEntity(Entity entity)
        {
            try
            {
                entity.health = swed.ReadInt(entity.baseAddress, offsets.health);
                entity.team = swed.ReadInt(entity.baseAddress, offsets.team);
                entity.shooting = swed.ReadInt(entity.baseAddress, offsets.shooting);
                
                // Leggi la posizione
                int x = swed.ReadInt(entity.baseAddress, offsets.position);
                int y = swed.ReadInt(entity.baseAddress, offsets.position + 4);
                int z = swed.ReadInt(entity.baseAddress, offsets.position + 8);
                
                // Converti da int a float
                entity.originPosition3d = new Vector3(
                    BitConverter.ToSingle(BitConverter.GetBytes(x), 0),
                    BitConverter.ToSingle(BitConverter.GetBytes(y), 0),
                    BitConverter.ToSingle(BitConverter.GetBytes(z), 0)
                );
                
                // Calcola la distanza dal giocatore locale
                if (localPlayer.baseAddress != IntPtr.Zero)
                {
                    entity.distance = Vector3.Distance(entity.originPosition3d, localPlayer.originPosition3d);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Errore nell'aggiornamento entità: {ex.Message}");
            }
        }

        public override void UpdateLocalPlayer()
        {
            try
            {
                localPlayer.baseAddress = swed.ReadPointer(mainModule, offsets.localPlayer);
                UpdateEntity(localPlayer);
                
                Console.WriteLine($"👤 Giocatore Locale aggiornato:");
                Console.WriteLine($"   Indirizzo: {localPlayer.baseAddress.ToString("X")}");
                Console.WriteLine($"   Salute: {localPlayer.health}");
                Console.WriteLine($"   Squadra: {localPlayer.team}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Errore nell'aggiornamento giocatore locale: {ex.Message}");
            }
        }

        public override void UpdateEntities()
        {
            try
            {
                entities.Clear();
                
                for (int i = 0; i < 10; i++)
                {
                    IntPtr entityAddress = swed.ReadPointer(mainModule, offsets.entityList, i * 0x4);
                    
                    if (entityAddress == IntPtr.Zero)
                        continue;
                    
                    Entity entity = new Entity();
                    entity.baseAddress = entityAddress;
                    
                    UpdateEntity(entity);
                    
                    // Aggiungi solo entità valide
                    if (entity.health >= 0 && entity.health <= 100)
                    {
                        entities.Add(entity);
                    }
                }
                
                Console.WriteLine($"🎯 Entità aggiornate: {entities.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Errore nell'aggiornamento entità: {ex.Message}");
            }
        }

        /// <summary>
        /// Aggiorna gli offset dinamicamente
        /// </summary>
        public void UpdateOffsets(GameOffsets newOffsets)
        {
            this.offsets = newOffsets;
            Console.WriteLine("🔄 Offset aggiornati dinamicamente");
        }

        /// <summary>
        /// Mostra informazioni dettagliate su tutte le entità
        /// </summary>
        public void PrintDetailedEntities()
        {
            Console.WriteLine("\n📋 Entità Dettagliate:");
            Console.WriteLine(new string('=', 60));
            
            foreach (var entity in entities)
            {
                Console.WriteLine($"📍 Entità: {entity.baseAddress.ToString("X")}");
                Console.WriteLine($"   Salute: {entity.health}");
                Console.WriteLine($"   Squadra: {entity.team}");
                Console.WriteLine($"   Sparando: {(entity.shooting == 1 ? "Sì" : "No")}");
                Console.WriteLine($"   Posizione: ({entity.originPosition3d.X:F2}, {entity.originPosition3d.Y:F2}, {entity.originPosition3d.Z:F2})");
                Console.WriteLine($"   Distanza: {entity.distance:F2}");
                Console.WriteLine(new string('-', 40));
            }
        }

        /// <summary>
        /// Trova entità sospette (possibili cheat)
        /// </summary>
        public List<Entity> FindSuspiciousEntities()
        {
            var suspicious = new List<Entity>();
            
            foreach (var entity in entities)
            {
                // Entità con salute anomala
                if (entity.health > 100 || entity.health < 0)
                {
                    suspicious.Add(entity);
                    continue;
                }
                
                // Entità con posizione anomala
                if (Math.Abs(entity.originPosition3d.X) > 10000 || 
                    Math.Abs(entity.originPosition3d.Y) > 10000 || 
                    Math.Abs(entity.originPosition3d.Z) > 10000)
                {
                    suspicious.Add(entity);
                    continue;
                }
                
                // Entità con squadra anomala
                if (entity.team < 0 || entity.team > 2)
                {
                    suspicious.Add(entity);
                }
            }
            
            return suspicious;
        }
    }
}
