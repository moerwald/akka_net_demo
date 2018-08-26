
using Akka.Actor;
using System;

namespace AkkaNetIotDemo
{
    public class IotApp
    {
        public static void Init()
        {
            using(var system = ActorSystem.Create("iot-system"))
            {
                var supervisor = system.ActorOf(Props.Create<IotSupervisor>(), "iot-supervisor");
                Console.ReadKey();
            }
        }
    }
}
