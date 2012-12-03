using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
    public class TileBomb : IProjectile
    {
        private Texture2D _texture;

        public string Source { get; private set; }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        public bool Hit { get; private set; }

        private Level Level;

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

        private Rectangle localBounds;
        private Vector2 velocity;
        private bool reachedApex;
        private float rotation;

        /// <summary>
        /// Gets a rectangle which bounds this bullet in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - _texture.Bounds.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - _texture.Bounds.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public Action OnPlayerHit { get; set; }

        public ExplosionData Explosion
        {
            get
            {
                return new ExplosionData
                    {
                        NumberOfParticles = 80,
                        Angle = 360,
                        CustomTexture = null,
                        MaxAge = 600f,
                        Position = Position,
                        Size = 50.0f
                    };
            }
        }

        public void OnHit()
        {
            var tile = new Tile(GeneratedTile, TileCollision.Impassable, Level.Content.Load<Texture2D>("Sprites/wallhit"));

            var x = (int) position.X/Tile.Width;
            var y = (int) position.Y/Tile.Height;
            Level.tiles[x, y] = tile;
        }

        public Texture2D GeneratedTile { get; set; }

        public Texture2D HitTexture { get; set; }

        public void Detonate()
        {
            Hit = true;
        }

        public TileBomb(Level level, Vector2 startingPos, int movement)
        {
            Level = level;
            Movement = movement;
            Position = startingPos;

            Hit = false;

            LoadContent();
        }

        public void LoadContent()
        {
            _texture = Level.Content.Load<Texture2D>("Sprites/Tilebomb");
            GeneratedTile = Level.Content.Load<Texture2D>("Tiles/BlockB1");
            HitTexture = Level.Content.Load<Texture2D>("Sprites/explosion");
        }

        public void Update(GameTime gameTime)
        {
            ApplyPhysics(gameTime);
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.

            velocity.X += Movement * MoveAcceleration * elapsed;
            velocity.X *= 0.58f;
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the bullet is now colliding with the level, separate them.
            HandleCollisions(gameTime);
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

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleCollisions(GameTime gameTime)
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + (absDepthY+3)*Math.Sign(depth.Y));

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;

                                    Hit = true;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + (absDepthX + 3) * Math.Sign(depth.X), Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;

                                Hit = true;
                            }
                        }
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Calculate the source rectangle of the current frame.
            var origin = new Vector2(12,12);

            rotation += 0.02f * gameTime.ElapsedGameTime.Milliseconds;

            // Draw the current frame.
            spriteBatch.Draw(_texture, Position, null, Color.White, rotation, origin, 1.0f, SpriteEffects.None, 1);
        }
    }
}
