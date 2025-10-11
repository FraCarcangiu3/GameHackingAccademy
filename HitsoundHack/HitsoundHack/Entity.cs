using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitsoundHack
{
    public class Entity
    {
        public IntPtr Address { get; set; } // memory address of the entity
        public int Health { get; set; } // server-side health
        public int Shooting { get; set; } // is the entity shooting
    }
}