using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay.Mechanics.StateData
{
	public struct ParticleData
	{
		public float BirthTime;
		public float MaxAge;
		public Vector2 OrginalPosition;
		public Vector2 Accelaration;
		public Vector2 Direction;
		public Vector2 Position;
		public float Scaling;
		public Color ModColor;
		public Texture2D ExplosionTexture;
	}
}