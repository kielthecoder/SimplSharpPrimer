using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.UI;

namespace Part3
{
    public class ControlSystem : CrestronControlSystem
    {
        private Tsw1050 _tp;
        private Switch _sw;
        private List<CardDevice> _inputs;
        private List<DmcOutputSingle> _outputs;

        private DmTx4K100C1G _tx;
        private DmRmc4k100C _rx;

        public ControlSystem()
            : base()
        {
            Thread.MaxNumberOfUserThreads = 20;
        }

        public override void InitializeSystem()
        {
            _tp = new Tsw1050(0x03, this);
            _tp.SigChange += new SigEventHandler(_tp_SigChange);
            _tp.Register();

            _inputs = new List<CardDevice>();
            _outputs = new List<DmcOutputSingle>();

            _sw = new DmMd8x8(0x10, this);
            _inputs.Add(new Dmc4kC(1, _sw));
            _outputs.Add(new DmcCoHdSingle(1, _sw));
            
            _tx = new DmTx4K100C1G(0x14, _sw.Inputs[1]);
            _tx.OnlineStatusChange +=new OnlineStatusChangeEventHandler(_tx_OnlineStatusChange);
            
            _rx = new DmRmc4k100C(0x15, _sw.Outputs[1]);
            _rx.OnlineStatusChange += new OnlineStatusChangeEventHandler(_rx_OnlineStatusChange);
            
            _sw.OnlineStatusChange += new OnlineStatusChangeEventHandler(_sw_OnlineStatusChange);
            _sw.Register();
        }

        void _tp_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                // Display controls
                if (args.Sig.Number > 20 && args.Sig.Number < 30)
                {
                    _tp_DisplayControl(currentDevice, args.Sig.Number - 20, args.Sig.BoolValue);
                }

                // Blu-ray controls
                if (args.Sig.Number > 30 && args.Sig.Number < 50)
                {
                    _tp_BluRayControl(currentDevice, args.Sig.Number - 30, args.Sig.BoolValue);
                }
            }
        }

        void _tp_DisplayControl(BasicTriList device, uint number, bool value)
        {
            if (value)
            {
                switch (number)
                {
                    case 1: // Power On
                        _rx.ComPorts[1].Send("\xAA\x11\x01\x01\x01\x14");
                        break;
                    case 2: // Power Off
                        _rx.ComPorts[1].Send("\xAA\x11\x01\x01\x00\x13");
                        break;
                    case 3: // HDMI
                        _rx.ComPorts[1].Send("\xAA\x14\x01\x01\x21\x37");
                        break;
                }
            }
        }

        void _tp_BluRayControl(BasicTriList device, uint number, bool value)
        {
            string[] commands = {
                "",
                "PLAY", "STOP", "PAUSE",
                "RSCAN", "FSCAN", "TRACK-", "TRACK+",
                "UP_ARROW", "DN_ARROW", "LEFT_ARROW", "RIGHT_ARROW", "ENTER/SELECT"
            };

            if (value)
                _tx.IROutputPorts[1].Press(commands[number]);
            else
                _tx.IROutputPorts[1].Release();
        }

        void _sw_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
        }

        void _tx_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                (currentDevice as DmTx4K100C1G).IROutputPorts[1].LoadIRDriver(Directory.GetApplicationDirectory() + Path.PathSeparator + "Samsung BD Series.ir");
            }
        }

        void _rx_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                (currentDevice as DmRmc4k100C).ComPorts[1].SetComPortSpec(ComPort.eComBaudRates.ComspecBaudRate9600,
                    ComPort.eComDataBits.ComspecDataBits8, ComPort.eComParityType.ComspecParityNone, ComPort.eComStopBits.ComspecStopBits1,
                    ComPort.eComProtocolType.ComspecProtocolRS232, ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                    ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone, false);
            }
        }
    }
}