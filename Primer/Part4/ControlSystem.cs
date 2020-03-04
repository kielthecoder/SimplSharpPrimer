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
            if (parms == "?")
            {
                CrestronConsole.ConsoleCommandResponse("CONTROLLERINFO\n\r\tNo parameters needed.\n\r");
            }
            else
            {
                CrestronConsole.ConsoleCommandResponse("Controller prompt:        {0}\n\r", this.ControllerPrompt);
                CrestronConsole.ConsoleCommandResponse("Number of serial ports:   {0}\n\r", this.NumberOfComPorts);
                CrestronConsole.ConsoleCommandResponse("Number of IR ports:       {0}\n\r", this.NumberOfIROutputPorts);

                if (this.SupportsRelay)
                    CrestronConsole.ConsoleCommandResponse("Number of relay ports:    {0}\n\r", this.NumberOfRelayPorts);
                if (this.SupportsDigitalInput)
                    CrestronConsole.ConsoleCommandResponse("Number of digital inputs: {0}\n\r", this.NumberOfDigitalInputPorts);
                if (this.SupportsVersiport)
                    CrestronConsole.ConsoleCommandResponse("Number of versiports:     {0}\n\r", this.NumberOfVersiPorts);

                CrestronConsole.ConsoleCommandResponse("Internal RF Gateway:      {0}\n\r", this.SupportsInternalRFGateway ? "YES" : "NO");

                // Check if built-in DM switcher
                if (this.SystemControl != null)
                {
                    CrestronConsole.ConsoleCommandResponse("System ID: {0}\n\r", this.SystemControl.SystemId);

                    if (this.SupportsSwitcherInputs)
                        CrestronConsole.ConsoleCommandResponse("Number of switcher inputs:  {0}\n\r", this.NumberOfSwitcherInputs);
                    if (this.SupportsSwitcherOutputs)
                        CrestronConsole.ConsoleCommandResponse("Number of switcher outputs: {0}\n\r", this.NumberOfSwitcherOutputs);
                }
            }
        }
    }
}