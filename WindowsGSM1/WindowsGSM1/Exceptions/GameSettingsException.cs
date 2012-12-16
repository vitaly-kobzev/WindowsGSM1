using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGSM1.Exceptions
{
    public class GameSettingsException : Exception
    {
        public GameSettingsException(string message) : base(message)
        {
        }
    }
}
