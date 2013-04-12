#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace WindowsGSM1.Gameplay
{
	/// <summary>
	/// Main class of a game. Every game object has a link to it
	/// </summary>
    public class Engine : IDisposable
    {
		public Level Level { get; private set; }

		public ICamera2D Camera { get; private set; }

        public GraphicsDevice GraphicsDevice { get; set; }

		public Player Player { get; private set; }

        public EventHandler<EventArgs> PlayerSpawned;

		public IExplosionMaster ExplosionMaster { get; private set; }

        private List<GameObject> _gameObjects = new List<GameObject>();
		
		//optimisation for collision checks
		private List<GameObject> _collidableObjects = new List<GameObject>(); 

		private readonly List<GameObject> _newObjects = new List<GameObject>();

		private Crosshair _crosshair;

		public int Score { get; private set; }

		public bool ReachedExit { get; private set; }

		// Level content.        
		public ContentManager Content { get; private set; }

		private bool _debugMode;

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
        public Engine(IServiceProvider serviceProvider, IExplosionMaster explosionMaster, bool debugMode)
        {
	        _debugMode = debugMode;

            // Create a new content manager to load content used just by this level.
            Content = new ContentManager(serviceProvider, "Content");

			ExplosionMaster = explosionMaster;

            HUDString = "HUD";
        }

		public void Initialize(Stream fileStream, ICamera2D camera)
		{
			Camera = camera;

			Level = new Level(this);

			InitCrosshair();

			AddGameObjects(Level.LoadLevel(fileStream));

			AddGameObject(Player = new Player(this, Level.StartLocation));
		}

		private void InitCrosshair()
		{
			_crosshair = new Crosshair(this);
			_crosshair.LoadContent(Content);
		}

		public Engine(IServiceProvider serviceProvider, IExplosionMaster explosionMaster):this(serviceProvider,explosionMaster,false)
		{}

		/// <summary>
		/// initializes objects and adds them to the game field
		/// </summary>
		/// <param name="objects"></param>
		public void AddGameObjects(GameObject[] objects)
		{
			Array.ForEach(objects, o => o.Initialize(Content));
			_newObjects.AddRange(objects);
		}

		/// <summary>
		/// initializes objects and adds it to the game field
		/// </summary>
		/// <param name="obj"></param>
		public void AddGameObject(GameObject obj)
	    {
			obj.Initialize(Content);
			_newObjects.Add(obj);
	    }

		public void AddSubscriberToCrosshairEvents(EventHandler<CrosshairArgs> handler)
		{
			_crosshair.CrosshairMoved += handler;
		}

		public void UnsubscribeToCrosshairEvents(EventHandler<CrosshairArgs> handler)
		{
			_crosshair.CrosshairMoved -= handler;
		}

        
        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(GameTime gameTime,KeyboardState keyboardState)
        {
	        UpdateCrosshair(gameTime, Mouse.GetState());

            // Pause while the player is dead or time is expired.
            if (Player.IsDead)
            {
                // Still want to perform physics on the player.
                Player.Update(gameTime,keyboardState);
            }
            else
            {
				UpdateGameObjects(gameTime, keyboardState);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Level.Height * Tile.Height)
                    OnPlayerKilled();

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile.
				if (!Player.IsDead &&
                    Player.IsOnGround &&
					Player.BoundingRectangle.Contains(Level.ExitPosition))
                {
                    OnExitReached();
                }
            }

            //if (gameTime.TotalGameTime.TotalSeconds > 3 && gameTime.TotalGameTime.TotalSeconds < 4 && !_movableObjects.Any(o=>o is Enemy))
            //   AddGameObject(new Enemy(this,new Vector2(1000,500)));
        }

		private void UpdateCrosshair(GameTime gameTime, MouseState mouseState)
		{
			_crosshair.Update(gameTime, mouseState);
		}

		/// <summary>
		/// Manages game object collection: adds new objects, removes dead
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="keyboardState"></param>
	    private void UpdateGameObjects(GameTime gameTime, KeyboardState keyboardState)
		{
			//manage new objects
			_gameObjects.AddRange(_newObjects);
			_collidableObjects.AddRange(_newObjects.Where(o=>o is CollidableGameObject).ToArray());
			_newObjects.Clear();

			var deadObjects = new List<GameObject>();

			foreach (var obj in _gameObjects)
			{
				if (!obj.IsDead)
					obj.Update(gameTime, keyboardState);
				else
				{
					deadObjects.Add(obj);
				}
			}
			_gameObjects = _gameObjects.Except(deadObjects).ToList();
			_collidableObjects = _collidableObjects.Except(deadObjects).ToList();

			KillInvisibleObjects();
	    }

		private void KillInvisibleObjects()
		{
			foreach (var gameObject in _collidableObjects)
			{
				if(gameObject is Bullet && !Camera.IsInView(gameObject))
					gameObject.Kill();
			}
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
            ReachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
			Player.Reset(Level.StartLocation);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var obj in _gameObjects)
            {
				obj.Draw(gameTime, spriteBatch);
            }
        }

        #endregion

		public IEnumerable<CollidableGameObject> GetCollidables()
		{
			return _collidableObjects.Cast<CollidableGameObject>();
		}

		public void DrawCrosshair(GameTime gameTime, SpriteBatch spriteBatch)
		{
			_crosshair.Draw(gameTime, spriteBatch);
		}

        public void PushToHUD(string str)
        {
            HUDString = str;
        }

        public string HUDString { get; private set; }

		public bool IsInDebugMode
		{
			get { return _debugMode; }
		}
    }
}
