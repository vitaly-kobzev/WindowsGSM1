using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
    public struct ExplosionData 
    {
        public Vector2 Position;
        public int NumberOfParticles;
        public float Size;
	    public int MinAngle;
        public int MaxAngle;
        public float MaxAge;
        public Texture2D CustomTexture;
    }
}
