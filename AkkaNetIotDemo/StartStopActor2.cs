
using Akka.Actor;
using System;

namespace AkkaNetIotDemo
{
    public class StartStopActor2 : UntypedActor
    {
        protected override void PreStart()
        {
            Console.WriteLine("second start");
        }

        protected override void PostStop()
        {
            Console.WriteLine("second stop");
        }
        protected override void OnReceive(object message)
        {
        }
    }
}
