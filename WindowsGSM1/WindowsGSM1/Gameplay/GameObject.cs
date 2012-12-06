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
    /// <summary>
    /// base class containing common properties
    /// all objects in the game should be descendants of this
    /// </summary>
    public abstract class GameObject
    {
        protected Texture2D _texture;

        protected Rectangle _localBounds;

        protected Rectangle _previousBounds;

        protected Level _level;

        public Vector2 Position { get; set; }

        protected GameObject(Level level)
        {
            _level = level;
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - Origin.X) + _localBounds.X;
                int top = (int)Math.Round(Position.Y - Origin.Y) + _localBounds.Y;

                return new Rectangle(left, top, _localBounds.Width, _localBounds.Height);
            }
        }

        protected abstract Vector2 Origin { get; }

        public abstract void LoadContent(ContentManager contentManager);

        protected abstract void UpdateInternal(GameTime gameTime, KeyboardState keyboardState);

        protected abstract void HandleCollisionsInternal(CollisionCheckResult collisions);

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            //flow of update is:
            //do any kind of adjustments
            UpdateInternal(gameTime, keyboardState);
            //do collision check
            var collisions = DoCollisionCheck();
            //handle collisions in a custom way
            HandleCollisionsInternal(collisions);
        }

        protected virtual CollisionCheckResult DoCollisionCheck()
        {
            var result = new CollisionCheckResult {PositionBeforeCheck = Position};
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            bool isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = _level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = _level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (_previousBounds.Bottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || isOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;

                                    result.HitImpassable = true;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;

                                result.HitImpassable = true;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            _previousBounds = bounds;
            result.IsOnGround = isOnGround;

            return result;
        }

    }

    public class CollisionCheckResult
    {
        public bool IsOnGround { get; set; }
        public Vector2 PositionBeforeCheck { get; set; }
        public bool HitImpassable { get; set; }
    }
}
