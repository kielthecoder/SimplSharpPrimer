using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

namespace Part3
{
    public class ControlSystem : CrestronControlSystem
    {
        private Tsw1050 _tp;

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
        }

        void _tp_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            throw new NotImplementedException();
        }
   }
}