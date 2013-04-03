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
    /// descendant of GameObject with physics support
    /// </summary>
    public abstract class MovableGameObject : GameObject
    {
        private Vector2 _positionBeforeUpdate;

        private Rectangle _previousBounds;

        protected MovableGameObject(Engine engine) : base(engine)
        {}

        protected abstract void UpdateInternal(GameTime gameTime, KeyboardState keyboardState);

		protected abstract void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions);

        public override void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            //flow of update is:
            //save initial position
            _positionBeforeUpdate = Position;
            //do any kind of adjustments
            UpdateInternal(gameTime, keyboardState);
            //do collision check
            var collisions = DoCollisionCheck();
            //let collisions to be handled in a custom way
            HandleCollisionsInternal(gameTime, collisions);
        }

        protected virtual CollisionCheckResult DoCollisionCheck()
        {
            var result = new CollisionCheckResult {PositionBeforeUpdate = _positionBeforeUpdate, PositionOfCollision = Position};

	        CheckTileCollisions(ref result);

			if(!result.HitImpassable)
				CheckMovableCollisions(ref result);

            return result;
        }

	    private void CheckMovableCollisions(ref CollisionCheckResult result)
	    {
			Rectangle bounds = BoundingRectangle;

		    foreach (var obj in _engine.GetMovables())
		    {
			    Vector2 depth = bounds.GetIntersectionDepth(obj.BoundingRectangle);

				if (depth != Vector2.Zero)
				{
					result.HitImpassable = true;
					result.CollidedObject = obj;
				}
		    }
	    }

	    private void CheckTileCollisions(ref CollisionCheckResult result)
		{
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
					GameObject collidedTile = null;
					// If this tile is collidable,
					TileCollision collision = _engine.Level.GetCollision(x, y, out collidedTile);
					if (collision != TileCollision.Passable)
					{
						// Determine collision depth (with direction) and magnitude.
						Rectangle tileBounds = _engine.Level.GetBounds(x, y);
						Vector2 depth = bounds.GetIntersectionDepth(tileBounds);
						if (depth != Vector2.Zero)
						{
							float absDepthX = Math.Abs(depth.X);
							float absDepthY = Math.Abs(depth.Y);

							result.HitImpassable = true;
							result.CollidedObject = collidedTile;

							// Resolve the collision along the shallow axis.
							if (absDepthY < absDepthX)
							{
								// If we crossed the top of a tile, we are on the ground.
								if (_previousBounds.Bottom <= tileBounds.Top)
									isOnGround = true;

								// Resolve the collision along the Y axis.
								Position = new Vector2(Position.X, Position.Y + depth.Y);

								// Perform further collisions with the new bounds.
								bounds = BoundingRectangle;
							}
							else
							{
								// Resolve the collision along the X axis.
								Position = new Vector2(Position.X + depth.X, Position.Y);

								// Perform further collisions with the new bounds.
								bounds = BoundingRectangle;
							}
						}
					}
				}
			}
			// Save the new bounds bottom.
			_previousBounds = bounds;
			result.IsOnGround = isOnGround;
		}

    }

    public struct CollisionCheckResult
    {
        public bool IsOnGround { get; set; }
        public Vector2 PositionBeforeUpdate { get; set; }
		public Vector2 PositionOfCollision { get; set; }
        public bool HitImpassable { get; set; }

		public GameObject CollidedObject { get; set; }
    }
}
