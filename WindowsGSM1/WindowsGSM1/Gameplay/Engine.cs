#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace WindowsGSM1.Gameplay
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    public class Engine : IDisposable
    {
        private Texture2D[] layers;

        public GraphicsDevice GraphicsDevice { get; set; }

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        public EventHandler<EventArgs> PlayerSpawned;

        private IExplosionMaster _explosionMaster;

        private List<Gem> gems = new List<Gem>();
        private List<GameObject> gameObjects = new List<GameObject>();
        private List<IProjectile> projectiles = new List<IProjectile>();


        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Engine(IServiceProvider serviceProvider, Stream fileStream, IExplosionMaster explosionMaster)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            

            _explosionMaster = explosionMaster;

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Texture2D[2];
            for (int i = 0; i < layers.Length; ++i)
            {
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_0");
            }

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        private void SpawnPlayer()
        {
            player = new Player(this, start);
            FirePlayerSpawned();
        }

        private void FirePlayerSpawned()
        {
            if (PlayerSpawned != null)
                PlayerSpawned(this,new EventArgs());
        }
        
        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision


        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(GameTime gameTime,KeyboardState keyboardState)
        {
            UpdateProjectiles(gameTime);

            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState);
                UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled();

                foreach (var obj in gameObjects)
                {
                    obj.Update(gameTime,keyboardState);
                }

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        private void UpdateProjectiles(GameTime gameTime)
        {
            var hitProjectiles = projectiles.Where(b => b.Hit).ToArray();
            //apply hit actions
            Array.ForEach(hitProjectiles, h => h.OnHit());
            //create explosions
            Array.ForEach(hitProjectiles, h => _explosionMaster.AddExplosion(h.Explosion, gameTime));

            //remove hit projectiles
            projectiles = projectiles.Except(hitProjectiles).ToList();
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    OnGemCollected(gem, Player);
                }
            }
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;

            gem.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled()
        {
            Player.OnKilled(null);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawTiles(spriteBatch);
            foreach (Gem gem in gems)
                gem.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch, false);

            foreach (var obj in gameObjects)
            {
                obj.Draw(gameTime, spriteBatch, false);
            }

        }

        #endregion

        public void CreateBullet(Vector2 startPos, float movement, GameTime gameTime)
        {
            _explosionMaster.AddExplosion(startPos, 4, 2f, 360, 100f, gameTime);
            var bullet = new Bullet(this, startPos, (int) movement, "Player");
            bullet.OnPlayerHit = OnPlayerKilled;
            gameObjects.Add(bullet);
            projectiles.Add(bullet);
        }

        public TileBomb CreateTilebomb(Vector2 vector2, int direction)
        {
            var bomb = new TileBomb(this, vector2, direction);
            gameObjects.Add(bomb);
            projectiles.Add(bomb);

            return bomb;
        }
    }
}
