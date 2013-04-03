using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsGSM1.Factories.Interfaces;
using WindowsGSM1.Managers.Implementation;
using WindowsGSM1.Managers.Interfaces;

namespace WindowsGSM1.Factories.Implementation
{
    public class ManagerFactory : IManagerFactory
    {
        private readonly ISettingsManager settingsManager;

        public ISettingsManager SettingsManager
        {
            get { return this.settingsManager; }
        }

        public ManagerFactory()
        {
            settingsManager = new SettingsManager();
        }
    }
}
