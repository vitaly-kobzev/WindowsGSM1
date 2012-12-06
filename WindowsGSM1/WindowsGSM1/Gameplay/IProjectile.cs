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

        string Source { get; }

        Action OnPlayerHit { get; set; }

        ExplosionData Explosion { get; }

        void OnHit();
    }
}
