#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using WindowsGSM1.Screens.Options;

#endregion

namespace WindowsGSM1.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        private MenuEntry video;
        private MenuEntry sound;
        private MenuEntry gameplay;

        #endregion

        #region Initialization

        public OptionsMenuScreen()
        {
            // Create our menu entries.
            this.video = new MenuEntry("Video");
            this.video.Selected += VideoMenuEntrySelected;

            this.sound = new MenuEntry("Sound");
            this.gameplay = new MenuEntry("Gameplay");

            // Add entries to the menu.
            this.MenuEntries.Add(video);
            this.MenuEntries.Add(sound);
            this.MenuEntries.Add(gameplay);
        }

        #endregion

        protected override string Title
        {
            get { return "Options"; }
        }

        protected override bool BackButtonAvailable
        {
            get { return true; }
        }

        #region Handle Input

        void VideoMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            this.ScreenManager.AddScreen(new VideoOptionsMenuScreen(), e.PlayerIndex);
        }

        #endregion
    }
}
