#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
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
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    public class Enemy : CollidableGameObject
    {
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;

        public Enemy(Engine engine, Vector2 pos) : base(engine)
        {
            Position = pos;
        }

        public override void Initialize(ContentManager contentManager)
        {
            //_texture = contentManager.Load<Texture2D>("Sprites/Guy/monetm");
			LocalBounds = new Rectangle(0, 0, Texture.Width, Texture.Height);
        }

        protected override void DrawInternal(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0.0f, Origin,10, SpriteEffects.None, 1);
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        private void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            Vector2 velocity = Vector2.Zero;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
        }

        public override void OnDead()
        {
            
        }

        public override void OnGotHit(HitData hitData)
        {
            
        }

        public override void Kill()
        {
            
        }

        protected override void UpdateInternal(GameTime gameTime, KeyboardState keyboardState)
        {
            ApplyPhysics(gameTime);
        }

        protected override void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions)
        {
            
        }
    }
}
