using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.UI;

namespace Part3
{
    public class ControlSystem : CrestronControlSystem
    {
        private Tsw1050 _tp;
        private Switch _sw;

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

            _sw = new DmMd8x8(0x10, this);
            _sw.OnlineStatusChange += new OnlineStatusChangeEventHandler(_sw_OnlineStatusChange);
            _sw.Register();

            _tx = new DmTx4K100C1G(_sw.Inputs[1]);
            _tx.OnlineStatusChange += new OnlineStatusChangeEventHandler(_tx_OnlineStatusChange);
            _tx.Register();

            _rx = new DmRmc4k100C(_sw.Outputs[1]);
            _rx.OnlineStatusChange += new OnlineStatusChangeEventHandler(_rx_OnlineStatusChange);
            _rx.Register();
        }

        void _tp_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            throw new NotImplementedException();
        }

        void _sw_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            throw new NotImplementedException();
        }

        void _tx_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            throw new NotImplementedException();
        }

        void _rx_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}