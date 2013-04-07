#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
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

    /// <summary>
    /// Stores the appearance and collision behavior of a tile.
    /// </summary>
    public abstract class Tile : GameObject
    {
        public TileCollision Collision;

        public const int Width = 40;
        public const int Height = 32;

        private Texture2D _hitTexture;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
		protected Tile(Texture2D texture, Vector2 position, TileCollision collision, Engine engine):base(engine)
        {
            _texture = texture;
	        Position = position;
            Collision = collision;
        }

	    public override void Initialize(ContentManager contentManager)
	    {
	        _hitTexture = contentManager.Load<Texture2D>("Sprites/tile_debris");
	    }

	    public override void Update(GameTime gameTime, KeyboardState keyboardState)
	    {
	    }

	    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
	    {
			if(_texture!=null)
				spriteBatch.Draw(_texture, Position, Color.White);
	    }

	    public override void OnHit(HitData hitData)
	    {
	        const int distributionAngle = 45;
	        float hitAngle = MathHelper.ToDegrees((float) hitData.Rotation) + 180;

			_engine.ExplosionMaster.AddExplosion(new ExplosionData
                    {
                        NumberOfParticles = 30,
                        MinAngle = (int) hitAngle - distributionAngle,
                        MaxAngle = (int) (hitAngle + distributionAngle),
                        CustomTexture = _hitTexture,
                        MaxAge = 600f,
                        Position = hitData.HitPosition,
                        Size = 40.0f
                    }, hitData.HitTime);
	    }
    }

    public class GroundTile : Tile
    {
        public GroundTile(Texture2D texture, Vector2 position, TileCollision collision, Engine engine) : base(texture, position, collision, engine)
        {
        }
        public override void OnDead()
        {
            //We are undestructable, weee!
        }
    }

    public class DestructableTile : Tile
    {
        private int _life;

        public DestructableTile(int life,Texture2D texture, Vector2 position, TileCollision collision, Engine engine) : base(texture, position, collision, engine)
        {
            _life = life;
        }

        public override void OnHit(HitData hitData)
        {
            //animate hit
            base.OnHit(hitData);

            ReceiveDamage(hitData);
        }

        private void ReceiveDamage(HitData hitData)
        {
            _life -= hitData.Damage;

            if(_life<=0)
                Kill();
        }

        public override void OnDead()
        {
            var v = Position / Size;
            _engine.Level.RemoveTileAt((int)v.X, (int)v.Y);
        }
    }
}
