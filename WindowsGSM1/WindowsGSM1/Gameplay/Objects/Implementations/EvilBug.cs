using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGSM1.Gameplay.Mechanics;
using WindowsGSM1.Gameplay.Mechanics.StateData;
using WindowsGSM1.Gameplay.Objects.Abstractions;

namespace WindowsGSM1.Gameplay.Objects.Implementations
{
	public class EvilBug : AIControllableObject
	{
		public EvilBug(Engine engine) : base(engine)
		{
		}

		public EvilBug(CollisionCheckType collisionCheck, Engine engine) : base(collisionCheck, engine)
		{
		}

		public override void Initialize(ContentManager contentManager)
		{
			throw new NotImplementedException();
		}

		protected override void DrawInternal(GameTime gameTime, SpriteBatch spriteBatch)
		{
			throw new NotImplementedException();
		}

		protected override void OnDead()
		{
			throw new NotImplementedException();
		}

		public override void OnGotHit(HitData hitData)
		{
		}

		protected override void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions)
		{
			throw new NotImplementedException();
		}
	}
}
