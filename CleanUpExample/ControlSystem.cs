using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support

namespace CleanUpExample
{
    public class ControlSystem : CrestronControlSystem
    {
        private Thread _unstoppable;
        private bool _running;

        public ControlSystem() : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                CrestronEnvironment.ProgramStatusEventHandler += ProgramEventHandler;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                _unstoppable = new Thread(InfiniteLoop, null);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        void ProgramEventHandler(eProgramStatusEventType type)
        {
            if (type == eProgramStatusEventType.Stopping)
                _running = false;
        }

        object InfiniteLoop(object userObj)
        {
            var toggle = false;

            Thread.Sleep(1000);
            CrestronConsole.PrintLine("Starting our loop: ");

            _running = true;
            while (_running)
            {
                toggle = !toggle;
                Thread.Sleep(1500);

                if (toggle)
                    CrestronConsole.Print("/");
                else
                    CrestronConsole.Print("\\");
            }

            CrestronConsole.PrintLine("Out of the loop!");

            return null;
        }
    }
}