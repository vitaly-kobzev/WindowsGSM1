#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace WindowsGSM1.Gameplay
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    public class Player : MovableGameObject, IFocusable
    {
        // Animations
	    private const string LowerBody = "low";
		private const string UpperBody = "top";

		private Animation _lowerIdle;
		private Animation _upperIdle;

        private SpriteEffects flip = SpriteEffects.None;
        private SegmentedAnimationPlayer _animationPlayer;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        Vector2 velocity;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround { get; set; }

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

	    private Texture2D _blank;

	    private Vector2 _crosshairPos;

		//gun data
		private const int ShotDelay = 150;

		private int ShotBuffer;

		private bool isFiring;
	    private double _gunRotation;
	    private double _previousRotation;
		//if gun is facing forward on backward
	    private int direction = 1;

        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Engine engine, Vector2 position) : base(engine)
        {
            //LoadContent(engine.Content);
            Reset(position);
        }

        protected override Vector2 Origin
        {
			get { return _animationPlayer.Origin; }
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public override void Initialize(ContentManager contentManager)
        {
			_animationPlayer = new SegmentedAnimationPlayer(LowerBody,new AnimationPlayer());

            // Load animated textures.
	        _lowerIdle = new Animation(contentManager.Load<Texture2D>("Sprites/Player/Soldier_lower"),24,32,0.2f,true);
			_upperIdle = new Animation(contentManager.Load<Texture2D>("Sprites/Player/Soldier_upper"), 24, 12, 0.2f, true);

	        Vector2 origin = new Vector2(6,6);
			_animationPlayer.SetNamedPlayer(UpperBody, new AnimationData {Offset = new Vector2(-6, -20 - 6)}, new AnimationPlayer { Origin = origin });

            // Calculate bounds within texture size.            
			int width = (int)(24);
			int left = 0;
			int height = (int)(32); //TODO hardcode
			int top = 0;

            _localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = contentManager.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = contentManager.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = contentManager.Load<SoundEffect>("Sounds/PlayerFall");

			_animationPlayer.PlayAnimation(LowerBody, _lowerIdle);
			_animationPlayer.PlayAnimation(UpperBody, _upperIdle);

			_blank = new Texture2D(_engine.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			_blank.SetData(new[] { Color.White });

			_engine.AddSubscriberToCrosshairEvents(HandleCrosshairEvents);
        }

	    private void HandleCrosshairEvents(object sender, CrosshairArgs crosshairArgs)
	    {
		    _crosshairPos = crosshairArgs.Data.Position;

		    _previousRotation = _gunRotation;

			_gunRotation = Math.Atan2(_crosshairPos.Y - GunPoint.Y, _crosshairPos.X - GunPoint.X);

			_animationPlayer.UpdateRotation(UpperBody, _gunRotation);

		    float cos = (float) Math.Cos(_gunRotation);

		    float sin = (float) Math.Sin(_gunRotation);

			_engine.PushToHUD(string.Format("cos {0} sin {1}",cos,sin));

			Vector2 defaultGunPoint = new Vector2(Position.X-6, Position.Y - _animationPlayer.GetPlayer(LowerBody).Animation.FrameHeight - _animationPlayer.GetPlayer(UpperBody).Animation.FrameHeight);

		    var rotCorrected = Vector2.Transform(defaultGunPoint, Matrix.CreateRotationZ((float) ((float) _gunRotation-_previousRotation)));

			GunPoint = rotCorrected; // 
	    }

		private void DrawLine(SpriteBatch batch, Texture2D blank,
			  float width, Color color, Vector2 point1, Vector2 point2)
		{
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
			float length = Vector2.Distance(point1, point2);

			batch.Draw(blank, point1, null, color,
					   angle, Vector2.Zero, new Vector2(length, width),
					   SpriteEffects.None, 0);
		}

	    /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            velocity = Vector2.Zero;
            isAlive = true;
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        protected override void UpdateInternal(GameTime gameTime, KeyboardState keyboardState)
        {
			if(IsAlive)
				GetInput(keyboardState);

            ShotBuffer += gameTime.ElapsedGameTime.Milliseconds;

            if (isFiring && ShotBuffer > ShotDelay)
            {
                CreateBullet(GunPoint,
					_gunRotation, gameTime);
                ShotBuffer = 0;
            }
           
            ApplyPhysics(gameTime);
        }

		protected Vector2 GunPoint { get; set; }

	    protected override void HandleCollisionsInternal(GameTime gameTime, CollisionCheckResult collisions)
        {
            IsOnGround = collisions.IsOnGround;

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == collisions.PositionBeforeUpdate.X)
                velocity.X = 0;

            if (Position.Y == collisions.PositionBeforeUpdate.Y)
                velocity.Y = 0;

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(velocity.X) - 0.02f > 0)
                {
					//TODO run
					_animationPlayer.PlayAnimation(LowerBody,_lowerIdle);
					_animationPlayer.PlayAnimation(UpperBody, _upperIdle);
                }
                else
                {
					_animationPlayer.PlayAnimation(LowerBody,_lowerIdle);
					_animationPlayer.PlayAnimation(UpperBody, _upperIdle);
                }
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
            isFiring = false;
        }

        protected TileBomb TileBomb { get; set; }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState)
        {

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if (keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
	            direction = -1;
            }
            else if (keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
	            direction = 1;
            }
            isFiring = keyboardState.IsKeyDown(Keys.LeftControl);

            // Check if the player wants to jump.
            isJumping =
               keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W);
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        private void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
					//TODO jump
					_animationPlayer.PlayAnimation(LowerBody,_lowerIdle);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            //sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            //sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        protected override void  DrawInternal(GameTime gameTime, SpriteBatch spriteBatch)
        {
			DrawLine(spriteBatch,_blank,3,Color.Red,GunPoint,_crosshairPos);

            // Flip the sprite to face the way we are moving.
            if (velocity.X < 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (velocity.X > 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            _animationPlayer.Draw(gameTime, spriteBatch, Position, flip);
        }

	    public override void OnDead()
	    {
		    OnKilled(null);
	    }

	    public override void OnHit(HitData hitData)
	    {
		    Kill();
	    }

	    Vector2 IFocusable.Position
        {
            get { return Position; }
        }

		public void CreateBullet(Vector2 startPos, double rotation, GameTime gameTime)
		{
			_engine.ExplosionMaster.AddExplosion(startPos, 4, 2f, 360, 100f, gameTime);
			var bullet = new Bullet(_engine, startPos, rotation, "Player");
			_engine.AddGameObject(bullet);
		}

		public TileBomb CreateTilebomb(Vector2 vector2, int direction)
		{
			var bomb = new TileBomb(_engine, vector2, direction);
			_engine.AddGameObject(bomb);

			return bomb;
		}
    }
}
