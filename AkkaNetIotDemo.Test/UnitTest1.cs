using AkkaNetIotDemo.Protocol;
using Akka.Actor;
using FluentAssertions;
using NUnit.Framework;
using Akka.TestKit.NUnit3;

namespace AkkaNetIotDemo.Test
{
    [TestFixture]
    public class UnitTest1 :  TestKit
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

                deviceActor.Tell(new ReadTemperature(requestId: 2, value:0.0 ), probe.Ref);
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
    }
}
