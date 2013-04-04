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

        Rectangle TitleSafeArea { get; }

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

		event EventHandler<Camera2D.CameraEventArgs> CameraMoved;
    }

    public class Camera2D : GameComponent, ICamera2D
    {
        private Vector2 _position;

        private int _boundaryBottom;
        private int _boundaryRight;
        private int _boundaryLeft;
        private Rectangle _titleSafeArea;

        private Engine _engine;

        protected float _viewportHeight;
        protected float _viewportWidth;

        public Camera2D(Game game, Engine engine)
            : base(game)
        {
            game.Components.Add(this);

            _engine = engine;

            _titleSafeArea = new Rectangle();
            _titleSafeArea.Width = 300;
            _titleSafeArea.Height = 200;
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
        public Rectangle TitleSafeArea { get { return _titleSafeArea; } }
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

            _boundaryBottom = (int)_viewportHeight - 400;
            _boundaryLeft = 0;
            base.Initialize();
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

			var dx = (Focus.Position.X - Position.X) * MoveSpeed * delta;
			var dy = (Focus.Position.Y - Position.Y) * MoveSpeed * delta;

	        _position.X += dx;
	        _position.Y += dy;

            _titleSafeArea.X = (int)(_position.X - ScreenCenter.X+10);
            _titleSafeArea.Y = (int)(_position.Y - ScreenCenter.Y+10);

			if(dx!=0 || dy!=0)
				OnCameraMoved(new CameraEventArgs{Delta = new Vector2(dx,dy)});

            base.Update(gameTime);
        }

        private void ApplyBoundaries()
        {
            _boundaryRight = _engine.Level.Width * 32;

            if (_position.Y > _boundaryBottom)
                _position.Y = _boundaryBottom;
            if (_position.X > _boundaryRight)
                _position.X = _boundaryRight;
            else if (_position.X < _boundaryLeft)
                _position.X = _boundaryLeft;
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