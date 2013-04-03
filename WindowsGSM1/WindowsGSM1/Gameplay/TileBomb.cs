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
    public class TileBomb : MovableGameObject
    {

        public string Source { get; private set; }

        public bool Hit { get; private set; }

        public int Movement;

        private const float MoveAcceleration = 20000f;
        private const float MaxMoveSpeed = 1750.0f;
        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.15f;
        private const float JumpLaunchVelocity = -2000.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        private float jumpTime;

        private Vector2 velocity;
        private bool reachedApex;
        private float rotation;
        private Vector2 _origin;

        /// <summary>
        /// Gets a rectangle which bounds this bullet in world space.
        /// </summary>


        public Action OnPlayerHit { get; set; }

        public ExplosionData Explosion
        {
            get
            {
                return new ExplosionData
                    {
                        NumberOfParticles = 80,
                        MaxAngle = 360,
                        CustomTexture = null,
                        MaxAge = 600f,
                        Position = Position,
                        Size = 50.0f
                    };
            }
        }

        public override void OnHit(HitData hitData)
        {
            //var tile = new Tile(GeneratedTile, TileCollision.Impassable, _engine.Content.Load<Texture2D>("Sprites/wallhit"));

            var x = (int) Position.X/Tile.Width;
            var y = (int)Position.Y / Tile.Height;
            //_engine.tiles[x, y] = tile;
        }

        public Texture2D GeneratedTile { get; set; }

        public Texture2D HitTexture { get; set; }

        public void Detonate()
        {
            Hit = true;
        }

        public TileBomb(Engine engine, Vector2 startingPos, int movement) : base(engine)
        {
            Movement = movement;
            Position = startingPos;

            Hit = false;
        }

        protected override Vector2 Origin
        {
            get { return _origin; }
        }

        public override void LoadContent(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Sprites/Tilebomb");
            GeneratedTile = content.Load<Texture2D>("Tiles/BlockB1");
            HitTexture = content.Load<Texture2D>("Sprites/explosion");

			int width = (int)(_texture.Width * 0.4);
			int left = (_texture.Width - width) / 2;
			int height = (int)(_texture.Height * 0.8);
			int top = _texture.Height - height;

			_localBounds = new Rectangle(left, top, width, height);
        }

        protected override void UpdateInternal(GameTime gameTime, KeyboardState keyboardState)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.

            velocity.X += Movement * MoveAcceleration * elapsed;
            velocity.X *= 0.58f;
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
        }

        protected override void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions)
        {
            Hit = collisions.HitImpassable;
        }

        private float DoJump(float velocityY, GameTime gameTime)
        {
            if (!reachedApex)
            {
                // Begin or continue a jump
                //if (jumpTime == 0.0f)
                //        jumpSound.Play();

                jumpTime += (float) gameTime.ElapsedGameTime.TotalSeconds;

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity*(1.0f - (float) Math.Pow(jumpTime/MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                    reachedApex = true;
                }
            }

            return velocityY;
        }

        protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Calculate the source rectangle of the current frame.
            var origin = new Vector2(12,12);

            rotation += 0.02f * gameTime.ElapsedGameTime.Milliseconds;

            // Draw the current frame.
            spriteBatch.Draw(_texture, Position, null, Color.White, rotation, origin, 1.0f, SpriteEffects.None, 1);
        }

	    public override void OnDead()
	    {
		    //same
	    }
    }
}
