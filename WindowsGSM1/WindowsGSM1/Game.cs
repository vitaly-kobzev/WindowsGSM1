#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGSM1.Factories.Implementation;
using WindowsGSM1.Factories.Interfaces;
using WindowsGSM1.Managers;
using WindowsGSM1.Managers.Implementation;
using WindowsGSM1.Managers.Interfaces;
using WindowsGSM1.Settings;

#endregion

namespace WindowsGSM1
{
    /// <summary>
    /// Sample showing how to manage different game states, with transitions
    /// between menu screens, a loading screen, the game itself, and a pause
    /// menu. This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class GameStateManagementGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        private readonly IManagerFactory managerFactory;

        private ScreenManager screenManager;

        // By preloading any assets used by UI rendering, we avoid framerate glitches
        // when they suddenly need to be loaded in the middle of a menu transition.
        private static readonly string[] preloadAssets = {"gradient"};

        #endregion

        #region Constructors

        public GameStateManagementGame()
        {
            Content.RootDirectory = "Content";

            managerFactory = new ManagerFactory();

            GameSettings.VideoSettings.VideoSettingsChanged += VideoSettings_VideoSettingsChanged;

            InitGraphicsDevice();

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);
        }

        private void VideoSettings_VideoSettingsChanged(object sender, System.EventArgs e)
        {
            this.InitGraphicsDevice();
        }

        #endregion

        #region Properties

        public IManagerFactory ManagerFactory
        {
            get { return this.managerFactory; }
        }

        private GameSettings GameSettings
        {
            get { return this.ManagerFactory.SettingsManager.GetGameSettings(); }
        }

        private GraphicsDeviceManager Graphics { get; set; }

        #endregion

        #region Overrides

        protected override void LoadContent()
        {
            foreach (string asset in preloadAssets)
            {
                this.Content.Load<object>(asset);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            this.Graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }

        #endregion

        private void InitGraphicsDevice()
        {
            VideoSettings videoSettings = this.GameSettings.VideoSettings;

            if (this.Graphics == null)
            {
                this.Graphics = new GraphicsDeviceManager(this);
            }

            this.Graphics.PreferredBackBufferWidth = videoSettings.ScreenWidth;
            this.Graphics.PreferredBackBufferHeight = videoSettings.ScreenHeight;
            this.Graphics.IsFullScreen = videoSettings.IsFullScreenMode;

            this.Graphics.ApplyChanges();
        }
    }

    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (GameStateManagementGame game = new GameStateManagementGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
