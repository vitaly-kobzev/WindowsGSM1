﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGSM1.Gameplay.Mechanics;
using WindowsGSM1.Gameplay.Mechanics.StateData;
using WindowsGSM1.Gameplay.Objects.Abstractions;

namespace WindowsGSM1.Gameplay.Objects.Implementations
{
	public class DestructableObject : CollidableGameObject
	{
		private readonly string _textureName;

		public DestructableObject(string texture, Vector2 position, Engine engine) : this(texture, position,CollisionCheckType.Rectangle,engine)
		{
		}

		public DestructableObject(string texture, Vector2 position, CollisionCheckType collisionCheck, Engine engine) : base(collisionCheck, engine)
		{
			_textureName = texture;
			Position = position;
		}

		public override void Initialize(ContentManager contentManager)
		{
			Texture = contentManager.Load<Texture2D>(_textureName);

			LocalBounds = new Rectangle(0, 0, Texture.Width, Texture.Height);
		}

		protected override Vector2 Origin
		{
			get { return Vector2.Zero; }
		}

		protected override void DrawInternal(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, Position, null, Color.White, 0.0f, Origin, 1, SpriteEffects.None, 1);
		}

		protected override void OnDead()
		{
		}

		public override void OnGotHit(HitData hitData)
		{
			
		}

		protected override void UpdateInternal(GameTime gameTime)
		{
			
		}

		protected override void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions)
		{
			
		}
	}
}
