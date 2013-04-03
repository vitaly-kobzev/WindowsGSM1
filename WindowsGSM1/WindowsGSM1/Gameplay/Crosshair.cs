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
	public class Crosshair
	{
		private Texture2D _texture;

		private Engine _engine;

		private Vector2 _position;
		private MouseState _initialState;

		private Vector2 Origin
		{
			get
			{
				return new Vector2(_texture.Width / 2f, _texture.Height);
			}
		}

		public Crosshair(Engine engine)
		{
			_engine = engine;

			_engine.Camera.CameraMoved += HandleCameraMoved;

			_position = _engine.Camera.Position;

			var center = _engine.Camera.ScreenCenter;

			Mouse.SetPosition((int)center.X, (int)center.Y);

			_initialState = Mouse.GetState();

		}

		void HandleCameraMoved(object sender, Camera2D.CameraEventArgs e)
		{
			_position += e.Delta;
		}

		public  void LoadContent(ContentManager contentManager)
		{
			_texture = contentManager.Load<Texture2D>("Sprites/Bullet");
		}

		public void Update(GameTime gameTime, MouseState state)
		{
			_position.X += state.X - _initialState.X;
			_position.Y += state.Y - _initialState.Y;

			_initialState = state;
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(_texture, _position, null, Color.White, 0.0f,  new Vector2(12,12), 1.0f, SpriteEffects.None, 1);
		}
	}
}
