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
                _tp = new XpanelForSmartGraphics(0x03, this);
                /*_tp.LoadSmartObjects(Directory.GetApplicationDirectory() +
                    Path.DirectorySeparatorChar + "SG Primer XPANEL.sgd");*/

                _tp.SmartObjects[1].SigChange += _tp_MenuSigChange;

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
            CrestronConsole.PrintLine("{0}: {1}", args.Sig.Type, args.Sig.Name);
        }
    }
}