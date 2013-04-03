using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGSM1.Gameplay
{
    public interface IExplosionMaster
    {
        void AddExplosion(Vector2 explosionPos, int numberOfParticles, float size, int angle, float maxAge, GameTime gameTime);
        void AddExplosion(Vector2 explosionPos, int numberOfParticles, float size, int angle, float maxAge, GameTime gameTime, Texture2D customTexture);
        void AddExplosion(ExplosionData explosionPos, GameTime gameTime);
    }

    public struct ParticleData
    {
        public float BirthTime;
        public float MaxAge;
        public Vector2 OrginalPosition;
        public Vector2 Accelaration;
        public Vector2 Direction;
        public Vector2 Position;
        public float Scaling;
        public Color ModColor;
        public Texture2D ExplosionTexture;
    }

    public class ParticleEngine : IExplosionMaster
    {
        private List<ParticleData> _particleList = new List<ParticleData>();
        private Random _randomizer = new Random();
        private Texture2D _explosionTexture;

        public ParticleEngine(ContentManager manager)
        {
            _explosionTexture = manager.Load<Texture2D>("Sprites/explosion");
        }

        public int ParticleCount { get { return _particleList.Count; } }

        public void UpdateParticles(GameTime gameTime)
        {
            float now = (float)gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = _particleList.Count - 1; i >= 0; i--)
            {
                ParticleData particle = _particleList[i];
                float timeAlive = now - particle.BirthTime;

                if (timeAlive > particle.MaxAge)
                {
                    _particleList.RemoveAt(i);
                }
                else
                {
                    float relAge = timeAlive / particle.MaxAge;
                    particle.Position = 0.5f * particle.Accelaration * relAge * relAge + particle.Direction * relAge + particle.OrginalPosition;
                    float invAge = 1.0f - relAge;
                    particle.ModColor = new Color(new Vector4(invAge, invAge, invAge, invAge));
                    Vector2 positionFromCenter = particle.Position - particle.OrginalPosition;
                    float distance = positionFromCenter.Length();
                    particle.Scaling = (50.0f + distance) / 200.0f;
                    _particleList[i] = particle;
                }
            }
        }

        public void AddExplosion(Vector2 explosionPos, int numberOfParticles, float size, int angle, float maxAge, GameTime gameTime)
        {
            AddExplosion(explosionPos, numberOfParticles, size,angle, maxAge, gameTime, _explosionTexture);
        }

        public void AddExplosion(Vector2 explosionPos, int numberOfParticles, float size, int angle, float maxAge, GameTime gameTime,
                                 Texture2D customTexture)
        {
            for (int i = 0; i < numberOfParticles; i++)
                AddExplosionParticle(explosionPos, size, 0, angle, maxAge, gameTime,customTexture);
        }

        public void AddExplosion(ExplosionData edata, GameTime gameTime)
        {
            for (int i = 0; i < edata.NumberOfParticles; i++)
                AddExplosionParticle(edata.Position, edata.Size, edata.MinAngle, edata.MaxAngle, edata.MaxAge, gameTime, edata.CustomTexture);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _particleList.Count; i++)
            {
                ParticleData particle = _particleList[i];
                spriteBatch.Draw(particle.ExplosionTexture??_explosionTexture, particle.Position, null, particle.ModColor, i, new Vector2(32, 32), particle.Scaling, SpriteEffects.None, 1);
            }
        }

        private void AddExplosionParticle(Vector2 explosionPos, float explosionSize,int minAngle, int maxAngle, float maxAge, GameTime gameTime,Texture2D explosionTexture)
        {
            var particle = new ParticleData();

            particle.ExplosionTexture = explosionTexture;
            particle.OrginalPosition = explosionPos;
            particle.Position = particle.OrginalPosition;

            particle.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            particle.MaxAge = maxAge;
            particle.Scaling = 0.25f;
            particle.ModColor = Color.White;  

            float particleDistance = (float)_randomizer.NextDouble() * explosionSize;
            Vector2 displacement = new Vector2(particleDistance, 0);
            var angle = MathHelper.ToRadians(_randomizer.Next(minAngle,maxAngle));
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

            particle.Direction = displacement * 2.0f;
            particle.Accelaration = -particle.Direction;

            _particleList.Add(particle);

        }

    }
}
