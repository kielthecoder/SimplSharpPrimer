using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;

namespace Part3
{
    public class ControlSystem : CrestronControlSystem
    {
        public ControlSystem()
            : base()
        {
            Thread.MaxNumberOfUserThreads = 20;
        }

        public override void InitializeSystem()
        {

        }
   }
}