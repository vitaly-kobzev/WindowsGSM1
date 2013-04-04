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

    public class Bullet : MovableGameObject
    {
        public string Source { get; private set; }

        private Vector2 _origin;

        public int Movement;

        private const float MoveSpeed = 175.0f;

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

        public override void OnHit(HitData hitData)
        {
	        IsDead = true;
        }

        public Texture2D HitTexture { get; set; }

        public Bullet(Engine engine, Vector2 startingPos, int movement, string source) : base(engine)
        {
            Movement = movement;
            Position = startingPos;
            Source = source;

            //LoadContent(_engine.Content);
        }

        public override void Initialize(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Sprites/Bullet");
        }

        protected override void UpdateInternal(GameTime gameTime, KeyboardState keyboardState)
        {
            ApplyPhysics(gameTime);
        }

        protected override void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions)
        {
			if (collisions.HitImpassable)
			{
				_engine.ExplosionMaster.AddExplosion(Explosion,gameTime);
				IsDead = true;

				if (collisions.CollidedObject != null) //if what we hit isn't just a tile or screen border
				{
					collisions.CollidedObject.OnHit(new HitData{Direction = Movement,HitPosition = Position,HitTime = gameTime});
				}
			}
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            var velocity = new Vector2 {X = Movement*MoveSpeed*elapsed, Y = 0};

            Position += velocity;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
        }

        protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the current frame.
            spriteBatch.Draw(_texture, Position, null, Color.White, 0.0f, Origin, 1.0f, SpriteEffects.None, 1);
        }

	    public override void OnDead()
	    {}
    }
}
