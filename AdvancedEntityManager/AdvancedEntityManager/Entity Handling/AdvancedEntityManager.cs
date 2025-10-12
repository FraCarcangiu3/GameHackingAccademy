using AdvancedEntityManager.Entity_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedEntityManager.Entity_Handling
{
    /// <summary>
    /// Manager avanzato per la gestione delle entità con offset dinamici
    /// </summary>
    public abstract class AdvancedEntityManager
    {
        protected Entity localPlayer = new Entity();
        protected List<Entity> entities = new List<Entity>();

        // Metodi astratti da implementare per ogni gioco
        public abstract void UpdateEntity(Entity entity);
        public abstract void UpdateLocalPlayer();
        public abstract void UpdateEntities();

        // Metodi comuni avanzati
        public List<Entity> GetEntitites()
        {
            return entities;
        }
        
        public Entity GetLocalPlayer()
        {
            return localPlayer;
        }

        // Funzionalità avanzate
        public void SortEntitiesByMagnitude()
        {
            entities = entities.OrderBy(o => o.magnitude).ToList();
        }

        public void SortEntitiesByFov()
        {
            entities = entities.OrderBy(o => o.viewAnglesDifference).ToList();
        }

        public void SortEntitiesByDistance()
        {
            entities = entities.OrderBy(o => o.distance).ToList();
        }

        public float CalculateEntityDistances(Entity entity1, Entity entity2)
        {
            return Vector3.Distance(entity1.originPosition3d, entity2.originPosition3d);
        }

        public List<Entity> GetEntitiesByTeam(int team)
        {
            return entities.Where(e => e.team == team).ToList();
        }

        public List<Entity> GetAliveEntities()
        {
            return entities.Where(e => e.health > 0).ToList();
        }

        public List<Entity> GetShootingEntities()
        {
            return entities.Where(e => e.shooting == 1).ToList();
        }

        public Entity GetClosestEntity()
        {
            return entities.OrderBy(e => e.distance).FirstOrDefault();
        }

        public Entity GetClosestEnemy()
        {
            return entities.Where(e => e.team != localPlayer.team && e.health > 0)
                          .OrderBy(e => e.distance)
                          .FirstOrDefault();
        }

        public int GetEntityCount()
        {
            return entities.Count;
        }

        public int GetAliveEntityCount()
        {
            return entities.Count(e => e.health > 0);
        }

        public void PrintEntitySummary()
        {
            Console.WriteLine($"📊 Riepilogo Entità:");
            Console.WriteLine($"   Totale: {GetEntityCount()}");
            Console.WriteLine($"   Vive: {GetAliveEntityCount()}");
            Console.WriteLine($"   Che stanno sparando: {GetShootingEntities().Count}");
            Console.WriteLine($"   Squadra 0: {GetEntitiesByTeam(0).Count}");
            Console.WriteLine($"   Squadra 1: {GetEntitiesByTeam(1).Count}");
        }
    }
}
