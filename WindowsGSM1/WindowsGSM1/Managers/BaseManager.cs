using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGSM1.Managers
{
    public abstract class BaseManager
    {
        protected abstract void Init();

        protected BaseManager()
        {
            this.Init();
        }
    }
}
