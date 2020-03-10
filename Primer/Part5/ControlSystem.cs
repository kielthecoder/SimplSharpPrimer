using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

namespace Part5
{
    public class ControlSystem : CrestronControlSystem
    {
        public const uint NumberOfSources = 2;

        // Samsung MDC protocol
        public const string PowerOn = "\xAA\x11\x01\x01\x01\x14";
        public const string PowerOff = "\xAA\x11\x01\x01\x00\x13";

        private DMInput[] _source;
        private bool[] _sourceAvailable;

        private DmTx200C2G _tx1;
        private DmTx200C2G _tx2;
        private DmRmcScalerC _rmc1;

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                _sourceAvailable = new bool[NumberOfSources];
                _source = new DMInput[NumberOfSources];
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
                    var control = this.SystemControl as Dmps3SystemControl;
                    control.SystemPowerOn();

                    // Samsung MDC = 9600 baud, 8 data bits, no parity, 1 stop bit
                    var displayComSettings = new ComPort.ComPortSpec();
                    displayComSettings.Protocol = ComPort.eComProtocolType.ComspecProtocolRS232;
                    displayComSettings.BaudRate = ComPort.eComBaudRates.ComspecBaudRate9600;
                    displayComSettings.DataBits = ComPort.eComDataBits.ComspecDataBits8;
                    displayComSettings.Parity = ComPort.eComParityType.ComspecParityNone;
                    displayComSettings.StopBits = ComPort.eComStopBits.ComspecStopBits1;
                    displayComSettings.HardwareHandShake = ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone;
                    displayComSettings.SoftwareHandshake = ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone;

                    // Assume DM transmitter is connected to DM input 6
                    _source[0] = this.SwitcherInputs[6] as DMInput;
                    _tx1 = new DmTx200C2G(0x14, _source[0]);
                    _tx1.HdmiInput.InputStreamChange += new EndpointInputStreamChangeEventHandler((input, args) => tx_InputStreamChange(0, input, args));
                    _tx1.VgaInput.InputStreamChange += new EndpointInputStreamChangeEventHandler((input, args) => tx_InputStreamChange(0, input, args));

                    // Assume DM transmitter is connected to DM input 7
                    _source[1] = this.SwitcherInputs[7] as DMInput;
                    _tx2 = new DmTx200C2G(0x15, _source[1]);
                    _tx2.HdmiInput.InputStreamChange += new EndpointInputStreamChangeEventHandler((input, args) => tx_InputStreamChange(1, input, args));
                    _tx2.VgaInput.InputStreamChange += new EndpointInputStreamChangeEventHandler((input, args) => tx_InputStreamChange(1, input, args));

                    // Assume DM roombox is connected to DM output 3
                    _rmc1 = new DmRmcScalerC(0x16, this.SwitcherOutputs[3] as DMOutput);
                    _rmc1.ComPorts[1].SetComPortSpec(displayComSettings);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.StackTrace);
            }
        }

        void tx_InputStreamChange(uint src, EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            if (args.EventId == EndpointInputStreamEventIds.SyncDetectedFeedbackEventId)
            {
                switch (inputStream.StreamType)
                {
                    case eDmStreamType.Hdmi:
                        var hdmiStream = inputStream as EndpointHdmiInput;
                        _sourceAvailable[src] = hdmiStream.SyncDetectedFeedback.BoolValue;
                        if (_sourceAvailable[src])
                            CrestronConsole.PrintLine("HDMI detected on source {0}", src);
                        else
                            CrestronConsole.PrintLine("HDMI not detected on source {0}", src);
                        break;
                    case eDmStreamType.Vga:
                        var vgaStream = inputStream as EndpointVgaInput;
                        _sourceAvailable[src] = vgaStream.SyncDetectedFeedback.BoolValue;
                        if (_sourceAvailable[src])
                            CrestronConsole.PrintLine("VGA detected on source {0}", src);
                        else
                            CrestronConsole.PrintLine("VGA not detected on source {0}", src);
                        break;
                }
                AutoRouteVideo();
            }
        }

        void AutoRouteVideo()
        {
            for (uint i = 0; i < NumberOfSources; i++)
            {
                if (_sourceAvailable[i])
                {
                    // Make route and power on display
                    (this.SwitcherOutputs[3] as DMOutput).VideoOut = _source[i];
                    _rmc1.ComPorts[1].Send(PowerOn);
                    return;
                }
            }

            // Clear route and power off display
            (this.SwitcherOutputs[3] as DMOutput).VideoOut = null;
            _rmc1.ComPorts[1].Send(PowerOff);
        }
    }
}