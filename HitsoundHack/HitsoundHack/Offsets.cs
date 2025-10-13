using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitsoundHack
{
    public class Offsets
    {
        // Offset per Assault Cube (ac_client.exe)
        public static int entityList = 0x0018AC04; // Address of the entity list
        public static int localPlayer = 0x0017E0A8; // Local player address
        public static int health = 0xEC; // Address of the health value (relative to entity)
        
        // Offset aggiuntivi per HitsoundHack
        public static int forceAttack = 0xF0; // Is the entity shooting (relative to entity)
    }
}
