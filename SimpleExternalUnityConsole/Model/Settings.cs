using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleExternalUnityConsole.Model
{
    public class Settings
    {
        private int _maxLogs = 999;
        public int MaxLogs 
        { 
            get { return _maxLogs; } 
            set 
            { 
                if (value > 0) { _maxLogs = value; }
                else { _maxLogs = 1; }
            }
        }
    }
}
