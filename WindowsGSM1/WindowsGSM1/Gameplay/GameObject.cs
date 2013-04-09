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
	/// base class containing common properties
	/// all objects in the game should be descendants of this
	/// </summary>
    public abstract class GameObject
	{
		private Texture2D _debugTex;

        protected Engine _engine;

        protected Texture2D _texture;

        protected Rectangle _localBounds;

        protected virtual Vector2 Origin { get{return new Vector2(_texture.Width/2f,_texture.Height);} }

        // Physics state
        public Vector2 Position { get; set; }

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - Origin.X) + _localBounds.X;
                int top = (int)Math.Round(Position.Y - Origin.Y) + _localBounds.Y;

                return new Rectangle(left, top, _localBounds.Width, _localBounds.Height);
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

        public abstract void Update(GameTime gameTime, KeyboardState keyboardState);

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

		public Texture2D Texture
		{
			get { return _texture; }
		}

		public abstract void OnDead();
		public abstract void OnGotHit(HitData hitData);
    }
}
