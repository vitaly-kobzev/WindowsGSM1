using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowsGSM1.Gameplay.AI
{
	public abstract class AIRule
	{
		protected AIManager Manager;

		public AIRule(AIManager aiManager)
		{
			Manager = aiManager;
		}

		public abstract Vector2 GetVelocity();

		public abstract int GetDirection();
	}
}
