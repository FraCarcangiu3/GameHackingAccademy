using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedEntityManager.Entity_Structures
{
    public class Entity
    {
        public IntPtr baseAddress { get; set; }
        public int health { get; set; }
        public string name { get; set; } = string.Empty;
        public int team { get; set; }
        public int lifeState { get; set; }
        public int jumpFlag { get; set; }
        public float magnitude { get; set; }
        public float distance { get; set; }
        public float viewAnglesDifference { get; set; }
        public Vector3 originPosition3d { get; set; }
        public Vector3 absPosition3d { get; set; }
        public Vector3 viewOffsetPosition3d { get; set; }
        public Vector2 originPosition2d { get; set; }
        public Vector2 abasPosition2d { get; set; }
        public Vector2 boxStartPositiion { get; set; }
        public Vector2 boxEndPosition { get; set; }
        public int shooting { get; set; }
    }
}
