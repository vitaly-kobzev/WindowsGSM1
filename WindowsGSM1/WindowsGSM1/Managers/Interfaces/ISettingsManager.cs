using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGSM1.Settings;

namespace WindowsGSM1.Managers.Interfaces
{
    public interface ISettingsManager
    {
        void Load();
        void Save();
        GameSettings GetGameSettings();
    }
}
