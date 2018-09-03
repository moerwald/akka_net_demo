using Akka.Actor;
using Akka.TestKit.NUnit3;
using AkkaNetIotDemo.Protocol;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace AkkaNetIotDemo.Test
{
    [TestFixture]
    public class UnitTest1 : TestKit
    {
        [Test]
        public void Device_actor_must_reply_with_latest_temperature_reading()
        {
            using (var system = ActorSystem.Create("iot-system"))
            {
                var probe = CreateTestProbe();
                var deviceActor = system.ActorOf(Device.Device.Props("group", "device"));

                deviceActor.Tell(new RecordTemperature(requestId: 1, value: 24.0), probe.Ref);
                probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);

                deviceActor.Tell(new ReadTemperature(requestId: 2, value: 0.0), probe.Ref);
                var response1 = probe.ExpectMsg<RespondTemperature>();
                response1.RequestId.Should().Be(2);
                response1.Value.Should().Be(24.0);

                deviceActor.Tell(new RecordTemperature(requestId: 3, value: 55.0), probe.Ref);
                probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 3);

                deviceActor.Tell(new ReadTemperature(requestId: 4, value: 0.0), probe.Ref);
                var response2 = probe.ExpectMsg<RespondTemperature>();
                response2.RequestId.Should().Be(4);
                response2.Value.Should().Be(55.0);
            }
        }

        [Test]
        public void Device_actor_must_reply_to_registration_requensts()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Device.Props("group", "device"));

            deviceActor.Tell(new RequestTrackDevice("group", "device"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            probe.LastSender.Should().Be(deviceActor);
        }

        [Test]
        public void Device_actor_must_ignore_wrong_registration_requests()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Device.Props("group", "device"));

            deviceActor.Tell(new RequestTrackDevice("wrongGroup", "device"), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));

            deviceActor.Tell(new RequestTrackDevice("group", "Wrongdevice"), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
        }
    }
}