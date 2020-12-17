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
        private CTimer _delay;
        private ushort _menu;
        private string _dialString;
        private short _pan;
        private short _tilt;
        private short _zoom;
        private bool _privacy;
        private bool _mute;
        private ushort _volume;
        private ushort _presentationSource;
        private ushort[] _lights;
        private ushort[][] _preset;

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
                _volume = 32767;
                _presentationSource = 0;

                _lights = new ushort[3];
                
                _preset = new ushort[3][];
                
                _preset[0] = new ushort[3];
                _preset[0][0] = 65535;
                _preset[0][1] = 65535;
                _preset[0][2] = 65535;
                
                _preset[1] = new ushort[3];
                _preset[1][0] = 32767;
                _preset[1][1] = 32767;
                _preset[1][2] = 32767;
                
                _preset[2] = new ushort[3];
                _preset[2][0] = 0;
                _preset[2][1] = 0;
                _preset[2][2] = 0;

                _tp = new XpanelForSmartGraphics(0x03, this);
                _tp.LoadSmartObjects(Directory.GetApplicationDirectory() +
                    Path.DirectorySeparatorChar + "SG Primer XPANEL.sgd");

                _tp.OnlineStatusChange += _tp_Online;
                _tp.SigChange += _tp_SigChange;

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
            while (true)
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
            while (true)
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

        private object _tp_Zoom(short dir)
        {
            while (true)
            {
                if (dir < 0) // Out
                {
                    if (_zoom > -100)
                        _zoom--;
                    else
                        break;
                }
                else // In
                {
                    if (_zoom < 100)
                        _zoom++;
                    else
                        break;
                }

                CrestronConsole.PrintLine("Zoom: {0}", _zoom);
                Thread.Sleep(50);
            }

            return null;
        }

        private void _tp_Online(GenericBase dev, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                _tp.UShortInput[11].UShortValue = _volume;
            }
        }

        private void _tp_SigChange(BasicTriList dev, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    if (args.Sig.BoolValue)
                        _tp_Press(args.Sig.Number);
                    else
                        _tp_Release(args.Sig.Number);
                    break;
                case eSigType.UShort:
                    switch (args.Sig.Number)
                    {
                        case 11:    // Volume
                            _volume = args.Sig.UShortValue;
                            CrestronConsole.PrintLine("Volume = {0}", _volume);
                            _tp.UShortInput[11].UShortValue = _volume;
                            break;
                        case 51:    // Lights Zone 1
                        case 52:    // Lights Zone 2
                        case 53:    // Lights Zone 3
                            if (!args.Sig.IsRamping)
                            {
                                var zn = args.Sig.Number - 51;
                                _lights[zn] = args.Sig.UShortValue;

                                CrestronConsole.PrintLine("Zone {0} level = {1}", zn + 1, args.Sig.UShortValue);
                                
                                _tp.UShortInput[51].UShortValue = _lights[0];
                                _tp.UShortInput[52].UShortValue = _lights[1];
                                _tp.UShortInput[53].UShortValue = _lights[2];
                            }

                            break;
                    }
                    break;
            }
        }

        private void _tp_Press(uint sig)
        {
            switch (sig)
            {
                case 11:    // Privacy
                    _privacy = !_privacy;
                    _tp.BooleanInput[11].BoolValue = _privacy;
                    break;
                case 12:    // Volume Mute
                    _mute = !_mute;
                    _tp.BooleanInput[12].BoolValue = _mute;
                    break;
                case 52:    // Dial
                    if (_dialString.Length > 0)
                        CrestronConsole.PrintLine("Dialing {0}...", _dialString);
                    break;
                case 53:    // Hang Up
                    _delay = new CTimer(o => { _dialString = ""; _tp.StringInput[11].StringValue = _dialString; }, 500);
                    break;
                case 54:    // Zoom In
                    _ramp = new Thread(o => { return _tp_Zoom(1); }, null);
                    break;
                case 55:    // Zoom Out
                    _ramp = new Thread(o => { return _tp_Zoom(-1); }, null);
                    break;
                case 71:    // Laptop
                case 72:    // AirMedia
                case 73:    // Apple TV
                case 74:    // Chromecast
                    var src = (ushort)(sig - 70);

                    if (_presentationSource == src)
                        _presentationSource = 0;
                    else
                        _presentationSource = src;

                    _tp.BooleanInput[71].BoolValue = (_presentationSource == 1);
                    _tp.BooleanInput[72].BoolValue = (_presentationSource == 2);
                    _tp.BooleanInput[73].BoolValue = (_presentationSource == 3);
                    _tp.BooleanInput[74].BoolValue = (_presentationSource == 4);

                    break;
                case 111:
                case 112:
                case 113:
                    var pre = sig - 111;
                    CrestronConsole.PrintLine("Recalling preset {0}...", pre);

                    _lights[0] = _preset[pre][0];
                    _lights[1] = _preset[pre][1];
                    _lights[2] = _preset[pre][2];

                    _tp.UShortInput[51].CreateRamp(_lights[0], 100);
                    _tp.UShortInput[52].CreateRamp(_lights[1], 100);
                    _tp.UShortInput[53].CreateRamp(_lights[2], 100);

                    break;
            }
        }

        private void _tp_Release(uint sig)
        {
            switch (sig)
            {
                case 54:    // Zoom In
                case 55:    // Zoom Out
                    if (_ramp != null)
                        _ramp.Abort();
                    break;
            }
        }
    }
}