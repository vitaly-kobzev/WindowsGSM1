using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGSM1.Exceptions;
using WindowsGSM1.Managers.Interfaces;
using WindowsGSM1.Settings;
using WindowsGSM1.Settings.Enums;

namespace WindowsGSM1.Screens.Options
{
    class VideoOptionsMenuScreen : MenuScreen
    {
        private MenuEntry resolutionMenuEntry;
        private MenuEntry fullScreenMenuEntry;

        private VideoSettings Settings { get; set; }

        protected override string Title
        {
            get { return "Video"; }
        }

        protected override bool BackButtonAvailable
        {
            get { return true; }
        }

        public override void Initialize()
        {
            base.Initialize();

            GameStateManagementGame game = this.ScreenManager.Game as GameStateManagementGame;

            if (game == null)
            {
                throw new GameSettingsException("Incorrect game type");
            }

            ISettingsManager settingsManager = game.ManagerFactory.SettingsManager;
            GameSettings gameSettings = settingsManager.GetGameSettings();
            this.Settings = gameSettings.VideoSettings;

            InitResolutionMenuEntry();
            InitFullScreenMenuEntry();

            this.MenuEntries.Add(this.resolutionMenuEntry);
            this.MenuEntries.Add(this.fullScreenMenuEntry);

            UpdateScreen();
        }

        private void InitResolutionMenuEntry()
        {
            this.resolutionMenuEntry = new MenuEntry();
            this.resolutionMenuEntry.Selected += OnResolutionMenuEntryClick;
        }

        private void InitFullScreenMenuEntry()
        {
            this.fullScreenMenuEntry = new MenuEntry();
            this.fullScreenMenuEntry.Selected += OnFullScreenMenuEntryClick;
        }

        private void OnFullScreenMenuEntryClick(object sender, PlayerIndexEventArgs e)
        {
            this.Settings.ToogleFullscreenMode();
            this.UpdateScreen();
        }

        private void OnResolutionMenuEntryClick(object sender, PlayerIndexEventArgs e)
        {
            List<ScreenResolution> res = this.Settings.AvailableResolutions();

            int index = res.FindIndex(x => x == this.Settings.CurrentResolution);
            int totalCount = res.Count;

            index = (index >= totalCount - 1) ? 0 : ++index;

            this.Settings.SetResolution(res[index]);
            this.UpdateScreen();
        }

        private void UpdateScreen()
        {
            this.resolutionMenuEntry.Text = string.Format("Resolution: {0}", this.Settings.CurrentResolution);
            this.fullScreenMenuEntry.Text = string.Format("Fullscreen: {0}", this.Settings.IsFullScreenMode ? "Yes" : "No");
        }
    }
}
