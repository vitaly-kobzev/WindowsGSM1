using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
    public enum BulletSource
    {
        Player,
        Enemy
    }

    public class Bullet : IProjectile
    {
        private Texture2D _texture;

        public string Source { get; private set; }

        public bool Hit { get; private set; }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private Level Level;

        public int Movement;

        private const float MoveSpeed = 1750.0f;

        private Rectangle localBounds;
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


        public Texture2D HitTexture { get; set; }

        public Bullet(Level level, Vector2 startingPos, int movement, string source)
        {
            Level = level;
            Movement = movement;
            Position = startingPos;
            Source = source;

            Hit = false;

            LoadContent();
        }

        public void LoadContent()
        {
            _texture = Level.Content.Load<Texture2D>("Sprites/Bullet");
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
            var velocity = new Vector2 {X = Movement*MoveSpeed*elapsed, Y = 0};

            Position += velocity;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the bullet is now colliding with the level, separate them.
            HandleCollisions(gameTime);
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
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;

                                    ProcessHit(x, y);
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;

                                ProcessHit(x, y);
                            }
                        }
                    }
                }
            }
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

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Calculate the source rectangle of the current frame.
            var origin = new Vector2(2,1);

            // Draw the current frame.
            spriteBatch.Draw(_texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 1);
        }
    }
}
