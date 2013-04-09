using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
	internal class SegmentedAnimationPlayer
	{
		private Dictionary<string, AnimationPlayer> _players;
		private Dictionary<string, AnimationData> _data;

		private AnimationPlayer _originPlayer;

		public SegmentedAnimationPlayer(string originName,AnimationPlayer originPlayer)
		{
			_players = new Dictionary<string, AnimationPlayer>();
			_data = new Dictionary<string, AnimationData>();

			SetNamedPlayer(originName, new AnimationData{Offset = Vector2.Zero,Rotation = 0.0f,Depth = 1}, originPlayer);
			_originPlayer = originPlayer;
		}

		public void SetNamedPlayer(string name, AnimationData offset, AnimationPlayer player)
		{
			if (_players.ContainsKey(name))
			{
				_players[name] = player;
				_data[name] = offset;
			}
			else
			{
				_players.Add(name,player);
				_data.Add(name,offset);
			}
		}

		public void UpdateRotation(string name, double rotation)
		{
			var data = _data[name];
			_data[name] = new AnimationData {Depth = data.Depth, Offset = data.Offset, Rotation = (float) rotation};
		}

		public void PlayAnimation(string name, Animation animation)
		{
			_players[name].PlayAnimation(animation);
		}

		public AnimationPlayer GetPlayer(string name)
		{
			return _players[name];
		}

        public AnimationData GetData(string name)
        {
            return _data[name];
        }

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position,SpriteEffects spriteEffects)
		{
			foreach (var animationPlayer in _players)
			{
				var data = _data[animationPlayer.Key];
				//offset position
				Vector2 pos = data.Offset + position;
				animationPlayer.Value.Draw(gameTime,spriteBatch,pos,data.Rotation,spriteEffects,data.Depth);
			}
		}

		/// <summary>
		/// Gets a texture origin at the bottom center of each frame.
		/// </summary>
		public Vector2 Origin
		{
			get { return _originPlayer.Origin; }
		}
	}

	internal struct AnimationData
	{
		public Vector2 Offset;
		public float Rotation;
		public float Depth;
	}
}
