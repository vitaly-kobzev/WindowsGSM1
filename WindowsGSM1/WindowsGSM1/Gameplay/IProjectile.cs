using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
    public interface IProjectile
    {
        bool Hit { get; }
        Texture2D HitTexture { get; }

        void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        void Update(GameTime gameTime);

        string Source { get; }

        Rectangle BoundingRectangle { get; }

        Vector2 Position { get; }

        Action OnPlayerHit { get; set; }
        ExplosionData Explosion { get; }

        void OnHit();
    }
}
