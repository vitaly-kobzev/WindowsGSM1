using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WindowsGSM1.Gameplay.Mechanics;
using WindowsGSM1.Gameplay.Mechanics.StateData;

namespace WindowsGSM1.Gameplay
{
	/// <summary>
	/// base class containing common properties
	/// all objects in the game should be descendants of this
	/// </summary>
    public abstract class GameObject
	{
		private Texture2D _debugTex;

        protected Engine _engine;

        private Texture2D _texture;

		public virtual Texture2D Texture
		{
			get { return _texture; }
			set { _texture = value; }
		}

        private Rectangle? _localBounds;

		public virtual Rectangle LocalBounds 
		{
			get { return _localBounds ?? new Rectangle(0,0,Texture.Width,Texture.Height); }
			set { _localBounds = value; }
		}

        protected virtual Vector2 Origin { get{return new Vector2(_texture.Width/2f,_texture.Height);} }

        // Physics state
        public Vector2 Position { get; set; }

        public Rectangle BoundingRectangle
        {
            get
            {
				int left = (int)Math.Round(Position.X - Origin.X) + LocalBounds.X;
				int top = (int)Math.Round(Position.Y - Origin.Y) + LocalBounds.Y;

				return new Rectangle(left, top, LocalBounds.Width, LocalBounds.Height);
            }
        }

        protected GameObject(Engine engine)
        {
            _engine = engine;
	        if (_engine.IsInDebugMode)
	        {
		        _debugTex = new Texture2D(_engine.GraphicsDevice, 1, 1);
		        _debugTex.SetData(new[] {Color.Red});
	        }
        }

        public abstract void Initialize(ContentManager contentManager);

        public abstract void Update(GameTime gameTime);

        protected abstract void DrawInternal(GameTime gameTime, SpriteBatch spriteBatch);

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
			if (_engine.IsInDebugMode)
            {
                spriteBatch.Draw(_debugTex, BoundingRectangle, Color.Red);
            }
			DrawInternal(gameTime, spriteBatch);
        }

		private bool _isDead;

		public bool IsDead
		{
			get { return _isDead; }
			protected set
			{
				if (value && !_isDead)
					OnDead();
				_isDead = value;
			}
		}

		public virtual void Kill()
		{
			IsDead = true;
		}

		protected abstract void OnDead();
		public abstract void OnGotHit(HitData hitData);
    }
}
