
using Akka.Actor;
using System;

namespace AkkaNetIotDemo
{
    public class StartStopActor1 : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case "stop":
                    Context.Stop(Self);
                    break;
            }
        }

        protected override void PreStart()
        {
            Console.WriteLine("first started");
            Context.ActorOf<StartStopActor2>("second");
        }

        protected override void PostStop()
        {
            Console.WriteLine("first stopped");
        }
    }
}
