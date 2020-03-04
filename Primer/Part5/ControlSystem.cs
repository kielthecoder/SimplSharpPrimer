using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;

namespace Part5
{
    public class ControlSystem : CrestronControlSystem
    {
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in ControlSystem constructor: {0}", e.StackTrace);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                // Currently only runs on DMPS architecture
                if (this.SystemControl == null)
                {
                    // Eventually we'll handle external switchers, too
                    ErrorLog.Error("Sorry, this program only runs on DMPS3 processors!");
                }
                else
                {

                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.StackTrace);
            }
        }
    }
}