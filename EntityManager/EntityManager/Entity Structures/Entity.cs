using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EntityManager.Entity_Structures
{
    public class Entity
    {
        public IntPtr baseAddress { get; set; }
        public int health { get; set; }


        // add more entity specific stuff here
        public string name { get; set; }
        public int team { get; set; }  
        public int lifeState { get; set; } // alive or dead
        public int jumpFlag { get; set; } // is the player jumping
        public float magnitude { get; set; } // speed of the player
        public float distance { get; set; } // distance from local player
        public float viewAnglesDifference { get; set; } // difference between local player view angles and entity view angles   
        public Vector3 originPosition3d { get; set; } // 3d position in the world
        public Vector3 absPosition3d { get; set; } // absolute 3d position in the world
        public Vector3 viewOffsetPosition3d { get; set; } // view offset position in the world
        public Vector2 originPosition2d { get; set; } // 2d position on the screen
        public Vector2 abasPosition2d { get; set; } // absolute 2d position on the screen
        public Vector2 boxStartPositiion { get; set; } // view offset 2d position on the screen
        public Vector2 boxEndPosition { get; set; } // view offset 2d position on the screen
    }
}
