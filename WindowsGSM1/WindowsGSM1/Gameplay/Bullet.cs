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

    public class Bullet : MovableGameObject, IProjectile
    {
        public string Source { get; private set; }

        public bool Hit { get; private set; }

        private Vector2 _origin;

        private Engine Level;

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
                        Angle = 380,
                        CustomTexture = HitTexture,
                        MaxAge = 200f,
                        Position = Position,
                        Size = 8.0f
                    };
            }
        }

        public void OnHit()
        {
            //do nothing for now
        }

        protected override Vector2 Origin
        {
            get { return _origin; }
        }

        public Texture2D HitTexture { get; set; }

        public Bullet(Engine engine, Vector2 startingPos, int movement, string source) : base(engine)
        {
            Movement = movement;
            Position = startingPos;
            Source = source;

            Hit = false;

            LoadContent(_engine.Content);
        }

        public override void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Sprites/Bullet");

            _origin = new Vector2(_texture.Width / 2.0f, _texture.Height);
        }

        protected override void UpdateInternal(GameTime gameTime, KeyboardState keyboardState)
        {
            ApplyPhysics(gameTime);
        }

        protected override void HandleCollisionsInternal(CollisionCheckResult collisions)
        {
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

        private void ProcessHit(int x, int y)
        {
            Hit = true;
            var victim = Level.GetHitVictim(x, y);
            if (victim != null)
            {
                victim.ProcessHit();
                HitTexture = victim.GetHitAnimation();
            }

        }

        protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Calculate the source rectangle of the current frame.
            var origin = new Vector2(2,1);

            // Draw the current frame.
            spriteBatch.Draw(_texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 1);
        }
    }
}
