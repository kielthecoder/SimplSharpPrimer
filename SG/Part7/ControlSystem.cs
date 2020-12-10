using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

namespace Part7
{
    public class ControlSystem : CrestronControlSystem
    {
        private XpanelForSmartGraphics _tp;
        private ushort _menu;
        private string _dialString;

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 100;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in Constructor: {0}", e.Message);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                _dialString = "";

                _tp = new XpanelForSmartGraphics(0x03, this);
                _tp.LoadSmartObjects(Directory.GetApplicationDirectory() +
                    Path.DirectorySeparatorChar + "SG Primer XPANEL.sgd");

                _tp.SmartObjects[1].SigChange += _tp_MenuSigChange;
                _tp.SmartObjects[2].SigChange += _tp_KeypadSigChange;

                var result = _tp.Register();

                if (result != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Warn("Problem registering XPanel: {0}", result);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void _tp_MenuSigChange(GenericBase dev, SmartObjectEventArgs args)
        {
            if (args.Sig.Name == "Item Clicked")
            {
                _menu = args.Sig.UShortValue;
                _tp_UpdateMenu();
            }
        }

        private void _tp_UpdateMenu()
        {
            _tp.BooleanInput[21].BoolValue = (_menu == 1);  // Video Call
            _tp.BooleanInput[22].BoolValue = (_menu == 2);  // Presentation
            _tp.BooleanInput[23].BoolValue = (_menu == 3);  // Lights
        }

        private void _tp_KeypadSigChange(GenericBase dev, SmartObjectEventArgs args)
        {
            if (args.Sig.BoolValue) // Button press
            {
                if (_dialString.Length < 50) 
                {
                    if (args.Sig.Name == "Misc_1")
                        _dialString += "*";
                    else if (args.Sig.Name == "Misc_2")
                        _dialString += "#";
                    else
                        _dialString += args.Sig.Name;

                    _tp_UpdateDialString();
                }
            }
        }

        private void _tp_UpdateDialString()
        {
            _tp.StringInput[11].StringValue = _dialString;
        }
    }
}