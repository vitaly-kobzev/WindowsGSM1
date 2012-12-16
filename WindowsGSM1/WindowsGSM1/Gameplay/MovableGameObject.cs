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
    public abstract class GameObject
    {
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
        }

        public abstract void LoadContent(ContentManager contentManager);

        public abstract void Update(GameTime gameTime, KeyboardState keyboardState);

        protected abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, bool debugMode)
        {
            if (debugMode)
            {
                var rectTex = new Texture2D(_engine.GraphicsDevice, 1, 1);
                rectTex.SetData(new[] { Color.Red });

                spriteBatch.Draw(rectTex, BoundingRectangle, Color.Red);
            }

            Draw(gameTime, spriteBatch);
        }
    }
}
