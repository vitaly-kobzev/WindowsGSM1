using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGSM1.Exceptions;
using WindowsGSM1.Settings.Enums;

namespace WindowsGSM1.Settings
{
    public class VideoSettings
    {
        public ScreenResolution CurrentResolution { get; private set; }
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public bool IsFullScreenMode { get; set; }

        public event EventHandler<EventArgs> VideoSettingsChanged; 

        public void SetResolution(ScreenResolution resolution)
        {
            switch (resolution)
            {
                case ScreenResolution.w800h600:
                    this.ScreenWidth = 800;
                    this.ScreenHeight = 600;
                    break;
                case ScreenResolution.w1024h768:
                    this.ScreenWidth = 1024;
                    this.ScreenHeight = 768;
                    break;
                case ScreenResolution.w1280h1024:
                    this.ScreenWidth = 1280;
                    this.ScreenHeight = 1024;
                    break;
                case ScreenResolution.w1920h1080:
                    this.ScreenWidth = 1920;
                    this.ScreenHeight = 1080;
                    break;
                default:
                    throw new GameSettingsException("Unsupported screen resolution");
            }

            this.CurrentResolution = resolution;
            this.OnVideoSettingsChanged();
        }

        public List<ScreenResolution> AvailableResolutions()
        {
            List<ScreenResolution> res = new List<ScreenResolution>()
                                             {
                                                 ScreenResolution.w800h600,
                                                 ScreenResolution.w1024h768,
                                                 ScreenResolution.w1280h1024,
                                                 ScreenResolution.w1920h1080,
                                             };

            return res;
        }

        public void ToogleFullscreenMode()
        {
            this.IsFullScreenMode = !this.IsFullScreenMode;
            this.OnVideoSettingsChanged();
        }

        private void OnVideoSettingsChanged()
        {
            if (this.VideoSettingsChanged != null)
            {
                this.VideoSettingsChanged.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
