using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
	public enum HUDStatus
	{
		PlayerWon,
		PlayerLost,
		PlayerDied
	}

	public interface IHUD
	{
		void Push(string data);
		void SetStatus(HUDStatus status);
		void ResetStatus();
	}

	public class HUD : IHUD
	{
		private readonly Queue<string> _data;
		private readonly int _size;

		private readonly ICamera2D _camera;

		private SpriteFont _hudFont;
		private Dictionary<HUDStatus, Texture2D> _overlayResources; 
		private HUDStatus? _status;

		public HUD(ICamera2D camera) : this(camera,5) { }

		public HUD(ICamera2D camera,int size)
		{
			_camera = camera;
			_size = size;
			_data = new Queue<string>(_size);
		}

		public void Initialize(ContentManager content)
		{
			_hudFont = content.Load<SpriteFont>("menufont");

			_overlayResources = new Dictionary<HUDStatus, Texture2D>();

			// Load overlay textures
			_overlayResources.Add(HUDStatus.PlayerWon, content.Load<Texture2D>("Overlays/you_win"));
			_overlayResources.Add(HUDStatus.PlayerLost,content.Load<Texture2D>("Overlays/you_lose"));
			_overlayResources.Add(HUDStatus.PlayerDied,content.Load<Texture2D>("Overlays/you_died"));
		}

		public void Push(string data)
		{
			if (_data.Count == _size)
				_data.Dequeue();
			_data.Enqueue(data);
		}

		public void SetStatus(HUDStatus status)
		{
			_status = status;
		}

		public void ResetStatus()
		{
			_status = null;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			Vector2 hudLocation = _camera.TitleSafeArea;
			Vector2 center = _camera.Position;

			//string hudString = _gameEngine.HUDString;

			spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, _camera.Transform);

			Vector2 offset = Vector2.Zero;

			foreach (var s in _data)
			{
				DrawShadowedString(spriteBatch, s, hudLocation+offset, Color.Yellow);
				offset = new Vector2(offset.X,offset.Y+32);
			}
			
			// Draw score
			//float timeHeight = _hudFont.MeasureString(hudString).Y;
			//DrawShadowedString(_hudFont, "SCORE: " + _gameEngine.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

			if (_status.HasValue)
			{
				var overlayResource = _overlayResources[_status.Value];
				// Draw status message.
				Vector2 statusSize = new Vector2(overlayResource.Width, overlayResource.Height);
				spriteBatch.Draw(overlayResource, center - statusSize / 2, Color.White);
			}

			spriteBatch.End();
		}

		private void DrawShadowedString(SpriteBatch spriteBatch, string value, Vector2 position, Color color)
		{
			spriteBatch.DrawString(_hudFont, value, position + new Vector2(1.0f, 1.0f), Color.Black);
			spriteBatch.DrawString(_hudFont, value, position, color);
		}
	}
}
