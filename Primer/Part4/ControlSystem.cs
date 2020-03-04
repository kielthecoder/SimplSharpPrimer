using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;

namespace Part4
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
                if (!CrestronConsole.AddNewConsoleCommand(ControlSystemInfo,
                    "controllerinfo", "Print information about this control system",
                    ConsoleAccessLevelEnum.AccessOperator))
                {
                    ErrorLog.Error("Unable to add 'controllerinfo' command to console");
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.StackTrace);
            }
        }

        public void ControlSystemInfo(string parms)
        {
            CrestronConsole.PrintLine("Number of serial ports:     {0}", this.NumberOfComPorts);
            CrestronConsole.PrintLine("Number of IR ports:         {0}", this.NumberOfIROutputPorts);
            CrestronConsole.PrintLine("Number of relay ports:      {0}", this.NumberOfRelayPorts);
            CrestronConsole.PrintLine("Number of versiports:       {0}", this.NumberOfVersiPorts);
            CrestronConsole.PrintLine("Number of switcher inputs:  {0}", this.NumberOfSwitcherInputs);
            CrestronConsole.PrintLine("Number of switcher outputs: {0}", this.NumberOfSwitcherOutputs);
        }
    }
}