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


        [Test]
        public void DeviceGroupQuery_must_return_TemperatureNotAvailable_for_devices_with_no_readings()
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

            queryActor.Tell(new RespondTemperature(requestId: 0, value: null), device1.Ref);
            queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"] is TemperatureNotAvailable &&
                msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
                msg.RequestId == 1);
        }


        [Test]
        public void DeviceGroupQuery_must_return_return_DeviceNotAvailable_if_device_stops_before_answering()
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
            device2.Tell(PoisonPill.Instance);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"] is DeviceNotAvailable &&
                msg.RequestId == 1);
        }

        [Test]
        public void DeviceGroupQuery_must_return_temperature_reading_even_if_device_stops_after_answering()
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
            device2.Tell(PoisonPill.Instance);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
                msg.RequestId == 1);
        }
    }
}
