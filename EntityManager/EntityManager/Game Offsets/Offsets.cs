using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityManager.Game_Offsets
{
    public static class Offsets // we don't really need an instance of the offsets class so we make it static
    {
        //relative to ac_client.exe 
        public static int localPlayer = 0x0017E0A8;
        public static int entityList = 0x0018AC04; //0x4 in hex between each entity

        //relative to entity
        public static int health = 0xEC; 


    }
}
