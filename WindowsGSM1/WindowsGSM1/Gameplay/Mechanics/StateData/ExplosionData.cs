using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay.Mechanics.StateData
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
