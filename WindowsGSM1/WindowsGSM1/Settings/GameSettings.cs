using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGSM1.Settings
{
    [Serializable]
    public class GameSettings
    {
        private VideoSettings videoSettings = new VideoSettings();

        public VideoSettings VideoSettings
        {
            get { return this.videoSettings; }
            set { this.videoSettings = value; }
        }

        public GameSettings()
        {
            
        }
    }
}
