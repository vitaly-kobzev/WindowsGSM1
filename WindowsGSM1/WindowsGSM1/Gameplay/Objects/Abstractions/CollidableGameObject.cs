using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGSM1.Extensions;
using WindowsGSM1.Gameplay.Mechanics;

namespace WindowsGSM1.Gameplay.Objects.Abstractions
{
	public enum CollisionCheckType
	{
		Rectangle,
		PerPixel
	}


    /// <summary>
    /// descendant of GameObject with physics support
    /// </summary>
    public abstract class CollidableGameObject : GameObject
    {
        private Vector2 _positionBeforeUpdate;

        private Rectangle _previousBounds;

	    private Color[] _textureData;

	    protected Vector2 Velocity;

		public override Texture2D Texture
		{
			get
			{
				return base.Texture;
			}
			set
			{
				_textureData = new Color[value.Height*value.Width];
				value.GetData(_textureData);

				base.Texture = value;
			}
		}

		public readonly CollisionCheckType CollCheckLevel;

		public Color[] GetTextureData()
		{
			return _textureData;
		}

        protected CollidableGameObject(Engine engine) : this(CollisionCheckType.Rectangle,engine)
        {}

		protected CollidableGameObject(CollisionCheckType collisionCheck,Engine engine)
			: base(engine)
		{
			CollCheckLevel = collisionCheck;
		}

	    protected abstract void UpdateInternal(GameTime gameTime);
	    protected virtual void ApplyPhysicsInternal(GameTime gameTime, float elapsed)
	    {
		    //do nothing is base class
	    }

		/// <summary>
		/// Applies gravity forces to the object (modifies only 'Y' part of the vector)
		/// Override if you dont want your object to be affected by gravity
		/// </summary>
		/// <param name="gameTime"></param>
		protected virtual void ApplyPhysics(GameTime gameTime)
	    {
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			Velocity.Y = Velocity.Y + Gravity.GravityAcceleration*elapsed;

			ApplyPhysicsInternal(gameTime, elapsed);

			Velocity.Y = MathHelper.Clamp(Velocity.Y, -Gravity.MaxFallSpeed, Gravity.MaxFallSpeed);

			// Apply velocity.
			Position += Velocity * elapsed;
			Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
	    }

	    protected abstract void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions);

        public override void Update(GameTime gameTime)
        {
            //flow of update is:
            //save initial position
            _positionBeforeUpdate = Position;
			//apply physics
			ApplyPhysics(gameTime);
            //do any kind of adjustments
            UpdateInternal(gameTime);
            //do collision check
            var collisions = DoCollisionCheck();
            //let collisions to be handled in a custom way
            HandleCollisionsInternal(gameTime, collisions);
        }

        protected virtual CollisionCheckResult DoCollisionCheck()
        {
            var result = new CollisionCheckResult {PositionBeforeUpdate = _positionBeforeUpdate, PositionOfCollision = Position};

	        CheckTileCollisions(ref result);

			//if we are clear of tiles, check game objects - performance consuming
			if(!result.HitImpassable)
				CheckObjectCollisions(ref result);

            return result;
        }

	    private void CheckObjectCollisions(ref CollisionCheckResult result)
	    {
			Rectangle bounds = BoundingRectangle;

		    foreach (var obj in _engine.GetCollidables())
		    {
				//dont collide with yourself,stupid
				if(obj==this)
					continue;

			    Vector2 depth = bounds.GetIntersectionDepth(obj.BoundingRectangle);

				if (depth != Vector2.Zero)
				{
					//if both objects are using the fastest collision check type - this is enough
					if (this.CollCheckLevel == CollisionCheckType.Rectangle && obj.CollCheckLevel == CollisionCheckType.Rectangle)
					{
						result.HitImpassable = true;
						result.CollidedObject = obj;
					}
					else
					{
						if (Collider.DoPerPixelCollision(bounds, _textureData, obj.BoundingRectangle, obj.GetTextureData()))
						{
							result.HitImpassable = true;
							result.CollidedObject = obj;
						}
					}
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
