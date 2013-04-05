#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using WindowsGSM1.Gameplay;
using WindowsGSM1.Postprocessing;

#endregion

namespace WindowsGSM1
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        // Global content.
        private SpriteFont hudFont;

        private ICamera2D _camera;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;
	    private Texture2D _background;

        private Engine _gameEngine;

	    private GraphicalPostprocessor _postprocessor;

        private ParticleEngine _particleEngine;

        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;

        ContentManager content;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random();

        float pauseAlpha;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Load fonts
            hudFont = content.Load<SpriteFont>("menufont");

            // Load overlay textures
            winOverlay = content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = content.Load<Texture2D>("Overlays/you_died");
	        _background = content.Load<Texture2D>("Backgrounds/sunset");

            _particleEngine = new ParticleEngine(content);


            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", 0);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
            {
                _gameEngine = new Engine(ScreenManager.Game.Services,_particleEngine);
                _gameEngine.GraphicsDevice = ScreenManager.GraphicsDevice;

				_camera = new Camera2D(ScreenManager.Game, _gameEngine);

				_gameEngine.Initialize(fileStream, _camera);

				_camera.Focus = _gameEngine.Player;
				//todo: implement video setting for postprocessing
				_postprocessor = new GraphicalPostprocessor(true);
				_postprocessor.Initialize(ScreenManager.Game);
            }

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                _gameEngine.Update(gameTime,keyboardState);
                _particleEngine.UpdateParticles(gameTime);
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
			keyboardState = Keyboard.GetState();
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.Black);

	        _postprocessor.BeginDraw();

            var spriteBatch = ScreenManager.SpriteBatch;

			DrawBackground(spriteBatch);

	        DrawObjects(gameTime, spriteBatch);

	        DrawParticles(spriteBatch);

	        DrawCrosshair(gameTime, spriteBatch);

	        //execute postprocessing
            base.Draw(gameTime);

			//Hud should be drawn in the last place - it shouldn't be affected by postprocessing
			DrawHud(spriteBatch);
        }

	    private void DrawCrosshair(GameTime gameTime, SpriteBatch spriteBatch)
	    {
		    spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, _camera.Transform);

		    _gameEngine.DrawCrosshair(gameTime, spriteBatch);

		    spriteBatch.End();
	    }

	    private void DrawParticles(SpriteBatch spriteBatch)
	    {
		    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, _camera.Transform);

		    _particleEngine.Draw(spriteBatch);

		    spriteBatch.End();
	    }

	    private void DrawObjects(GameTime gameTime, SpriteBatch spriteBatch)
	    {
		    spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, _camera.Transform);

		    _gameEngine.Draw(gameTime, spriteBatch);

		    spriteBatch.End();
	    }

	    private void DrawBackground(SpriteBatch spriteBatch)
	    {
		    spriteBatch.Begin(0, BlendState.Opaque, SamplerState.LinearWrap, null, null);

		    var rectangle = new Rectangle(0, 0, (ScreenManager.GraphicsDevice.Viewport.Width),(ScreenManager.GraphicsDevice.Viewport.Height));

			var source = new Rectangle((int) (_camera.Position.X*0.1f), 0, _background.Width, _background.Height);

		    spriteBatch.Draw(_background, rectangle,source, Color.White);

			spriteBatch.End();
	    }

	    private void DrawHud(SpriteBatch spriteBatch)
        {
            Vector2 hudLocation = _camera.TitleSafeArea;
	        Vector2 center = _camera.Position;

			string hudString = string.Format("HUD");

			spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, _camera.Transform);

			DrawShadowedString(hudFont, hudString, hudLocation, Color.Yellow);

            // Draw score
			float timeHeight = hudFont.MeasureString(hudString).Y;
            DrawShadowedString(hudFont, "SCORE: " + _gameEngine.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (!_gameEngine.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                ScreenManager.SpriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

			spriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            ScreenManager.SpriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            ScreenManager.SpriteBatch.DrawString(font, value, position, color);
        }


        #endregion
    }
}
