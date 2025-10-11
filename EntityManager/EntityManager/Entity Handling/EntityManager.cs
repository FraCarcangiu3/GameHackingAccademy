using EntityManager.Entity_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EntityManager.Entity_Handling
{
    public abstract class EntityManager // this will be our base structure for managing entities
    {
        protected Entity localPlayer = new Entity(); // our player
        protected List<Entity> entities = new List<Entity>(); // all the other entities, excluding the localplayer 

        // methods we want to define in a unique way each time 
        public abstract void UpdateEntity(Entity entity); // update any entity
        public abstract void UpdateLocalPlayer(); //different method for the localplayer only 
        public abstract void UpdateEntities(); //update all of our nice ents

        // methods that we use in almost all cases, methods that are the same for every game 
        public List<Entity> GetEntitites()
        {
            return entities;
        }
        public Entity GetLocalPlayer()
        {
            return localPlayer;
        }

        // here we just build on more and more and more ...

        public void SortEntitiesByMagnitude() // sort by speed
        {
            entities = entities.OrderBy(o => o.magnitude).ToList();
        }

        public void SortEntitiesByFov() // get closest to crosshair 
        {
            entities = entities.OrderBy(o => o.viewAnglesDifference).ToList();
        }

        public float CalculateEntityDistances(Entity entity1, Entity entity2) // calculate distance from local player
        {
            return Vector3.Distance(entity1.originPosition3d, entity2.originPosition3d);
        }
    }
}