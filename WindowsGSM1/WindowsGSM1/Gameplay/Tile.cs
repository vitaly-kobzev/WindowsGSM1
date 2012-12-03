#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    public struct Tile : IHittable
    {
        public Texture2D Texture;
        public TileCollision Collision;

        private Texture2D HitAnimation;

        private int Health;

        public const int Width = 40;
        public const int Height = 32;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
        public Tile(Texture2D texture, TileCollision collision, Texture2D hitAnimation)
        {
            Texture = texture;
            Collision = collision;
            HitAnimation = hitAnimation;
            Health = 3;
        }

        public Texture2D GetHitAnimation()
        {
            return HitAnimation;
        }

        public bool Dead { get { return Health <= 0; }}
        public void ProcessHit()
        {
            Health--;
        }
    }
}
