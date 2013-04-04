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

		public event EventHandler<CrosshairArgs> CrosshairMoved;

		private void OnCrosshairMoved(CrosshairArgs e)
		{
			EventHandler<CrosshairArgs> handler = CrosshairMoved;
			if (handler != null) handler(this, e);
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
			int dx = state.X - _initialState.X;
			int dy = state.Y - _initialState.Y;

			_position.X += dx;
			_position.Y += dy;

			_initialState = state;

			OnCrosshairMoved(new CrosshairArgs(new CrosshairData{Position = _position}));
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(_texture, _position, null, Color.White, 0.0f, Origin, 1.0f, SpriteEffects.None, 1);
		}
	}

	public class CrosshairArgs : EventArgs
	{
		public CrosshairData Data { get; private set; }

		public CrosshairArgs(CrosshairData data)
		{
			Data = data;
		}
	}

	public struct CrosshairData
	{
		public Vector2 Position { get; set; }
	}
}
