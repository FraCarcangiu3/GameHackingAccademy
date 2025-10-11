using EntityManager.Entity_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swed32;
using EntityManager.Game_Offsets;

namespace EntityManager.Entity_Handling
{
    public class ACEntityManager : EntityManager
    {
        // unique variables for assault cube 
        private Swed swed; // swed instance
        private IntPtr mainModule; // base address of the main module

        // constructor because we want fresh data from certain variables
        public ACEntityManager(Swed swedInstance, IntPtr mainModule)
        {
            this.swed = swedInstance;
            this.mainModule = mainModule;
        }   


        public override void UpdateEntity(Entity entity)
        {
            entity.health = swed.ReadInt(entity.baseAddress,Offsets.health); // now we need some offsets
        }

        public override void UpdateLocalPlayer()
        {
            localPlayer.baseAddress = swed.ReadPointer(mainModule, Offsets.localPlayer);
            UpdateEntity(localPlayer);
            //then specific localplayer stuff after i suppose
        }

        public override void UpdateEntities()
        {
            entities.Clear(); // clear the list so we don't have duplicates
            for (int i = 0; i < 10; i++) // loop through 10 entities, max 32 players in assault cube 
            {
                //get current entity
                IntPtr entityAddress = swed.ReadPointer(mainModule, Offsets.entityList, i * 0x4); // 0x4 between each entity

                //do example check to see that enity is valid
                if (entityAddress == IntPtr.Zero)
                    continue; // skip to next entity if this one is invalid


                //create new entity and get its information 
                Entity entity = new Entity();
                entity.baseAddress = entityAddress;

                UpdateEntity(entity); // update the entity with our method
                entities.Add(entity); // add the entity to our list



            }





        }
    }
}
