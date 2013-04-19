using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowsGSM1.Gameplay.Objects.Abstractions;

namespace WindowsGSM1.Gameplay.AI
{
	public class MeleeOffensiveRule : AIRule
	{
		private AIControllableObject _subject;

		public MeleeOffensiveRule(AIControllableObject subject, AIManager aiManager) : base(aiManager)
		{
			_subject = subject;
		}

		public override Vector2 GetVelocity()
		{
			throw new NotImplementedException();
		}

		public override int GetDirection()
		{
			int result = 0;
			if (Manager.PlayerPosition.X > _subject.Position.X)
				result = 1;
			else
			{
				result = -1;
			}
			return result;
		}
	}
}
