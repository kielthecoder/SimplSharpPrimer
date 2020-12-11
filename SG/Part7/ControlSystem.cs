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
        private Thread _ramp;
        private ushort _menu;
        private string _dialString;
        private short _pan;
        private short _tilt;

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
                _tp.SmartObjects[3].SigChange += _tp_DPadSigChange;

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
            if (args.Sig.BoolValue) // Press
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

        private void _tp_DPadSigChange(GenericBase dev, SmartObjectEventArgs args)
        {
            if (args.Sig.BoolValue) // Press
            {
                if (args.Sig.Name == "Center")
                {
                    _pan = 0;
                    _tilt = 0;
                }
                else if (args.Sig.Name == "Up")
                {
                    _ramp = new Thread(o => { return _tp_Tilt(1); }, null);
                }
                else if (args.Sig.Name == "Down")
                {
                    _ramp = new Thread(o => { return _tp_Tilt(-1); }, null);
                }
                else if (args.Sig.Name == "Left")
                {
                    _ramp = new Thread(o => { return _tp_Pan(-1); }, null);
                }
                else if (args.Sig.Name == "Right")
                {
                    _ramp = new Thread(o => { return _tp_Pan(1); }, null);
                }
            }
            else // Release
            {
                if (args.Sig.Name == "Center")
                {
                    // nothing
                }
                else
                {
                    if (_ramp != null)
                        _ramp.Abort();
                }
            }
        }

        private object _tp_Tilt(short dir)
        {
            while (_tilt >= -100 && _tilt <= 100)
            {
                if (dir < 0) // Down
                {
                    if (_tilt > -100)
                        _tilt--;
                    else
                        break;
                }
                else // Up
                {
                    if (_tilt < 100)
                        _tilt++;
                    else
                        break;
                }

                CrestronConsole.PrintLine("Pan: {0}\tTilt: {1}", _pan, _tilt);
                Thread.Sleep(50);
            }

            return null;
        }

        private object _tp_Pan(short dir)
        {
            while (_pan >= -180 && _pan <= 180)
            {
                if (dir < 0) // Left
                {
                    if (_pan > -180)
                        _pan--;
                    else
                        break;
                }
                else // Right
                {
                    if (_pan < 180)
                        _pan++;
                    else
                        break;
                }

                CrestronConsole.PrintLine("Pan: {0}\tTilt: {1}", _pan, _tilt);
                Thread.Sleep(50);
            }

            return null;
        }
    }
}