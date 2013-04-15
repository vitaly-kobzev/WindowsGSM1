using Microsoft.Xna.Framework;

namespace WindowsGSM1.Gameplay.Mechanics.StateData
{
	public struct HitData
	{
		public double Rotation { get; set; }
		public Vector2 HitPosition { get; set; }
		public GameTime HitTime { get; set; }
        public int Damage { get; set; }
	}
}
