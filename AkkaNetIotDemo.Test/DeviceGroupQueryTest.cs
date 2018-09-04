using NUnit.Framework;
using System;
using System.Collections.Generic;
using Akka.TestKit.NUnit3;
using AkkaNetIotDemo.Actors;
using Akka.Actor;
using AkkaNetIotDemo.Protocol;
using Akka.Util.Internal;

namespace AkkaNetIotDemo.Test
{
    [TestFixture]
    public class DeviceGroupQueryTest : TestKit
    {
        [Test]
        public void DeviceGroupQuery_must_return_temperature_value_for_working_devices()
        {
            var requester = CreateTestProbe();

            var device1 = CreateTestProbe();
            var device2 = CreateTestProbe();

            var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
                
                new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
                requestId: 1,
                requester: requester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
            device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

            queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
            queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
                msg.RequestId == 1);
        }
    }
}
