using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
    public interface IFocusable
    {
        Vector2 Position { get; }
    }

    public interface ICamera2D
    {
        /// <summary>
        /// Gets or sets the position of the camera
        /// </summary>
        /// <value>The position.</value>
        Vector2 Position { get; set; }

        Vector2 TitleSafeArea { get; }

        /// <summary>
        /// Gets or sets the move speed of the camera.
        /// The camera will tween to its destination.
        /// </summary>
        /// <value>The move speed.</value>
        float MoveSpeed { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the camera.
        /// </summary>
        /// <value>The rotation.</value>
        float Rotation { get; set; }

        /// <summary>
        /// Gets the origin of the viewport (accounts for Scale)
        /// </summary>        
        /// <value>The origin.</value>
        Vector2 Origin { get; }

        /// <summary>
        /// Gets or sets the scale of the Camera
        /// </summary>
        /// <value>The scale.</value>
        float Scale { get; set; }

        /// <summary>
        /// Gets the screen center (does not account for Scale)
        /// </summary>
        /// <value>The screen center.</value>
        Vector2 ScreenCenter { get; }

        /// <summary>
        /// Gets the transform that can be applied to 
        /// the SpriteBatch Class.
        /// </summary>
        /// <see cref="SpriteBatch"/>
        /// <value>The transform.</value>
        Matrix Transform { get; }

        /// <summary>
        /// Gets or sets the focus of the Camera.
        /// </summary>
        /// <seealso cref="IFocusable"/>
        /// <value>The focus.</value>
        IFocusable Focus { get; set; }

		/// <summary>
		/// Determines whether the target is in view given the specified position.
		/// This can be used to increase performance by not drawing objects
		/// directly in the viewport
		/// </summary>
		/// <returns>
		///     <c>true</c> if the target is in view at the specified position; otherwise, <c>false</c>.
		/// </returns>
		bool IsInView(GameObject gameObject);

	    Matrix GetViewMatrix(Vector2 parallax);

		event EventHandler<Camera2D.CameraEventArgs> CameraMoved;

    }

    public class Camera2D : GameComponent, ICamera2D
    {
        private Vector2 _position;
		private Vector2 _titleSafeArea;

        private Engine _engine;

        protected float _viewportHeight;
        protected float _viewportWidth;

        public Camera2D(Game game, Engine engine)
            : base(game)
        {
            game.Components.Add(this);

            _engine = engine;

            _titleSafeArea = new Vector2();
        }

        #region Properties

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public float Rotation { get; set; }
        public Vector2 Origin { get; set; }
        public float Scale { get; set; }
        public Vector2 ScreenCenter { get; protected set; }
        public Matrix Transform { get; set; }
        public IFocusable Focus { get; set; }
		public Vector2 TitleSafeArea { get { return _titleSafeArea; } }
        public float MoveSpeed { get; set; }

        #endregion

        /// <summary>
        /// Called when the GameComponent needs to be initialized. 
        /// </summary>
        public override void Initialize()
        {
            _viewportWidth = Game.GraphicsDevice.Viewport.Width;
            _viewportHeight = Game.GraphicsDevice.Viewport.Height;

            ScreenCenter = new Vector2(_viewportWidth / 2, _viewportHeight / 2);
            Scale = 1;
            MoveSpeed = 1.25f;

            base.Initialize();
        }

		public Matrix GetViewMatrix(Vector2 parallax)
		{
			// To add parallax, simply multiply it by the position
			return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
				// The next line has a catch. See note below.
				   Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
				   Matrix.CreateRotationZ(Rotation) *
				   Matrix.CreateScale(Scale, Scale, 1) *
				   Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
		}

        public override void Update(GameTime gameTime)
        {
            // Create the Transform used by any
            // spritebatch process

            Transform = Matrix.Identity *
                        Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                        Matrix.CreateRotationZ(Rotation) *
                        Matrix.CreateTranslation(Origin.X, Origin.Y, 0) *
                        Matrix.CreateScale(new Vector3(Scale, Scale, Scale));

            Origin = ScreenCenter / Scale;

            // Move the Camera to the position that it needs to go
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

	        Vector2 beforeUpdate = _position;

			var dx = (Focus.Position.X - Position.X) * MoveSpeed * delta;
			var dy = (Focus.Position.Y - Position.Y) * MoveSpeed * delta;

	        _position.X += dx;
	        _position.Y += dy;

	        float belowGround = _position.Y + _viewportHeight/2f - _engine.Level.GroundLevel;

			if(belowGround > 0)
				_position.Y -= belowGround; 

            _titleSafeArea.X = _position.X - ScreenCenter.X+10;
            _titleSafeArea.Y = _position.Y - ScreenCenter.Y+10;

			if(_position-beforeUpdate!=Vector2.Zero)
				OnCameraMoved(new CameraEventArgs { Delta = _position - beforeUpdate });

            base.Update(gameTime);
        }

	    public event EventHandler<CameraEventArgs> CameraMoved;

	    private void OnCameraMoved(CameraEventArgs e)
	    {
		    EventHandler<CameraEventArgs> handler = CameraMoved;
		    if (handler != null) handler(this, e);
	    }

	    /// <summary>
        /// Determines whether the target is in view given the specified position.
        /// This can be used to increase performance by not drawing objects
        /// directly in the viewport
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="texture">The texture.</param>
        /// <returns>
        ///     <c>true</c> if [is in view] [the specified position]; otherwise, <c>false</c>.
        /// </returns>
		public bool IsInView(GameObject gameObject)
        {
            // If the object is not within the horizontal bounds of the screen
		    var position = gameObject.Position;

		    var texture = gameObject.Texture;

		    if (texture != null) //dont kill the tiles
		    {

			    if ((position.X + texture.Width) < (Position.X - Origin.X) || (position.X) > (Position.X + Origin.X))
				    return false;

			    // If the object is not within the vertical bounds of the screen
			    if ((position.Y + texture.Height) < (Position.Y - Origin.Y) || (position.Y) > (Position.Y + Origin.Y))
				    return false;
		    }
		    // In View
            return true;
        }

		public class CameraEventArgs :EventArgs
		{
			public Vector2 Delta { get; set; }
		}
    }
}