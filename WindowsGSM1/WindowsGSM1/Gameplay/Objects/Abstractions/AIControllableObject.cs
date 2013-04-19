using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGSM1.Gameplay.AI;
using WindowsGSM1.Gameplay.Mechanics;

namespace WindowsGSM1.Gameplay.Objects.Abstractions
{
	public abstract class AIControllableObject : CollidableGameObject
	{
		protected AIRule Behavior;
		protected AIControllableObject(Engine engine) : this(CollisionCheckType.Rectangle, engine)
		{
		}

		protected AIControllableObject(CollisionCheckType collisionCheck, Engine engine) : base(collisionCheck, engine)
		{
		}

		public virtual void ApplyRule(AIRule rule)
		{
			Behavior = rule;
		}
	}
}
