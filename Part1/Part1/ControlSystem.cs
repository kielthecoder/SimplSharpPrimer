using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;

namespace Part1
{
    public class ControlSystem : CrestronControlSystem
    {
        public ControlSystem()
            : base()
        {
            Thread.MaxNumberOfUserThreads = 10;
        }

        public override void InitializeSystem()
        {
            CrestronConsole.PrintLine("\n Hello world! \n");
        }
    }
}