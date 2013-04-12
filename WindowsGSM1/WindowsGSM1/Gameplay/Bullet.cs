using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowsGSM1.Gameplay
{

    public class Bullet : CollidableGameObject
    {
        public string Source { get; private set; }

		public double Rotation;

        private const float MoveSpeed = 350.0f;

        public Action OnPlayerHit { get; set; }

        public ExplosionData Explosion
        {
            get
            {
                return new ExplosionData
                    {
                        NumberOfParticles = 40,
                        MaxAngle = 380,
                        CustomTexture = HitTexture,
                        MaxAge = 200f,
                        Position = Position,
                        Size = 8.0f
                    };
            }
        }

        public override void OnGotHit(HitData hitData)
        {
	        Kill();
        }

        public Texture2D HitTexture { get; set; }

        public Bullet(Engine engine, Vector2 startingPos, double rotation, string source) : base(CollisionCheckType.Rectangle,engine)
        {
			Rotation = rotation;
            Position = startingPos;
            Source = source;

            //LoadContent(_engine.Content);
        }

        public override void Initialize(ContentManager content)
        {
            Texture = content.Load<Texture2D>("Sprites/Bullet");
        }

        protected override void UpdateInternal(GameTime gameTime, KeyboardState keyboardState)
        {
            ApplyPhysics(gameTime);
        }

        protected override void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions)
        {
			if (collisions.HitImpassable)
			{
				//_engine.ExplosionMaster.AddExplosion(Explosion,gameTime);
				Kill();

				if (collisions.CollidedObject != null) //if what we hit isn't just a tile or screen border
				{
					//TODO USE ROTATION
					collisions.CollidedObject.OnGotHit(new HitData{Rotation = Rotation,HitPosition = Position,HitTime = gameTime, Damage = 1});
				}
			}
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;

			double hipothenus = MoveSpeed*elapsed;
			var x = hipothenus * Math.Cos(Rotation);
			var y = hipothenus * Math.Sin(Rotation);

            var velocity = new Vector2 {X = (float) x, Y = (float) y};

            Position += velocity;
            Position = new Vector2(Position.X, Position.Y);
        }

        protected override void DrawInternal(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the current frame.
            spriteBatch.Draw(Texture, Position, null, Color.White, 0.0f, Origin, 1.0f, SpriteEffects.None, 1);
        }

	    public override void OnDead()
	    {}
    }
}
