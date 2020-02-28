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

namespace Part4
{
    public class ControlSystem : CrestronControlSystem
    {
        private DmRmcScalerC _rmc;
        private DmTx200C2G _tx;

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
                _rmc = new DmRmcScalerC(0x14, SwitcherOutputs[3] as DMOutput);
                _rmc.OnlineStatusChange += new OnlineStatusChangeEventHandler(_debug_OnlineStatusChange);
                _rmc.Register();

                _tx = new DmTx200C2G(0x15, SwitcherInputs[6] as DMInput);
                _tx.OnlineStatusChange += new OnlineStatusChangeEventHandler(_debug_OnlineStatusChange);
                _tx.Register();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.StackTrace);
            }
        }

        void _debug_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            CrestronConsole.PrintLine("{0} is {1}", currentDevice, args);
        }
    }
}