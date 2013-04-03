using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGSM1.Exceptions;
using WindowsGSM1.Managers.Interfaces;
using WindowsGSM1.Settings;
using WindowsGSM1.Settings.Enums;

namespace WindowsGSM1.Managers.Implementation
{
    public class SettingsManager : BaseManager, ISettingsManager
    {
        protected GameSettings Settings { get; private set; }

        protected override void Init()
        {
            this.Load();
        }

        public void Load()
        {
            // temporary hardcoded load. TODO: add load from xml

            this.Settings = new GameSettings
                                {
                                    VideoSettings = new VideoSettings()
                                                        {
                                                            IsFullScreenMode = false,
                                                        }
                                };

            this.Settings.VideoSettings.SetResolution(ScreenResolution.w800h600);
        }

        public void Save()
        {
            // TODO: add save to xml
        }

        public GameSettings GetGameSettings()
        {
            if (this.Settings == null)
            {
                throw new GameSettingsException("Trying to retrieve game settings before they were initialized");
            }

            return this.Settings;
        }
    }
}
