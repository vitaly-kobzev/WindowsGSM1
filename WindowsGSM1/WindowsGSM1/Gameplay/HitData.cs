using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowsGSM1.Gameplay
{
	public struct HitData
	{
		public int Direction { get; set; }
		public Vector2 HitPosition { get; set; }
		public GameTime HitTime { get; set; }
	}
}
