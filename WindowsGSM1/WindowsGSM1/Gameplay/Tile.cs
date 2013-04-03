#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowsGSM1.Gameplay
{
    /// <summary>
    /// Controls the collision detection and response behavior of a tile.
    /// </summary>
    public enum TileCollision
    {
        /// <summary>
        /// A passable tile is one which does not hinder player motion at all.
        /// </summary>
        Passable = 0,

        /// <summary>
        /// An impassable tile is one which does not allow the player to move through
        /// it at all. It is completely solid.
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// A platform tile is one which behaves like a passable tile except when the
        /// player is above it. A player can jump up through a platform as well as move
        /// past it to the left and right, but can not fall down through the top of it.
        /// </summary>
        Platform = 2,
    }

    public interface IHittable
    {
        Texture2D GetHitAnimation();
        bool Dead { get; }
        void ProcessHit();
    }

    /// <summary>
    /// Stores the appearance and collision behavior of a tile.
    /// </summary>
    public class Tile : GameObject
    {
        public TileCollision Collision;

        public const int Width = 40;
        public const int Height = 32;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
		public Tile(Texture2D texture, Vector2 position, TileCollision collision, Engine engine):base(engine)
        {
            _texture = texture;
	        Position = position;
            Collision = collision;
        }

	    public override void LoadContent(ContentManager contentManager)
	    {
	    }

	    public override void Update(GameTime gameTime, KeyboardState keyboardState)
	    {
	    }

	    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	    {
			if(_texture!=null)
				spriteBatch.Draw(_texture, Position, Color.White);
	    }

	    public override void OnDead()
	    {
		    var v = Position/Size;
			_engine.Level.RemoveTileAt((int)v.X,(int)v.Y);
	    }

	    public override void OnHit(HitData hitData)
	    {
			_engine.ExplosionMaster.AddExplosion(new ExplosionData
                    {
                        NumberOfParticles = 10,
						MinAngle = 135,
                        MaxAngle = 225,
                        CustomTexture = null,
                        MaxAge = 600f,
                        Position = hitData.HitPosition,
                        Size = 25.0f
                    }, hitData.HitTime);
		    IsDead = true;
	    }
    }
}
