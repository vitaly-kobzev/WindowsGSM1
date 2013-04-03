using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGSM1.Managers.Interfaces;

namespace WindowsGSM1.Factories.Interfaces
{
    public interface IManagerFactory
    {
        ISettingsManager SettingsManager {get;}
    }
}
