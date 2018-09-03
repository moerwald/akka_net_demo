using Akka.TestKit.NUnit3;
using AkkaNetIotDemo.Group;
using AkkaNetIotDemo.Protocol;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace AkkaNetIotDemo.Test
{
    [TestFixture]
    public class GroupsTest : TestKit
    {
        [Test]
        public void DeviceGroup_actor_must_be_able_to_register_a_device_actor()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

            groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();

            var deviceActor1 = probe.LastSender;

            groupActor.Tell(new RequestTrackDevice("group", "device2"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor2 = probe.LastSender;

            deviceActor1.Should().NotBe(deviceActor2);

            // Check that device actors are working
            deviceActor1.Tell(new RecordTemperature(requestId: 0, value: 1.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 0);
            deviceActor2.Tell(new RecordTemperature(requestId: 1, value: 2.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);
        }

        [Test]
        public void DeviceGroup_actor_must_ignore_requests_for_wrong_groupId()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

            groupActor.Tell(new RequestTrackDevice("wrongGroup", "device1"), probe.Ref);
            probe.ExpectNoMsg(TimeSpan.FromMilliseconds(500));
        }

        public void DeviceGroup_actor_must_return_same_actor_for_same_deviceId()
        {
            var probe = CreateTestProbe();
            var groupActor = Sys.ActorOf(DeviceGroup.Props("group"));

            groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor1 = probe.LastSender;

            groupActor.Tell(new RequestTrackDevice("group", "device1"), probe.Ref);
            probe.ExpectMsg<DeviceRegistered>();
            var deviceActor2 = probe.LastSender;

            deviceActor1.Should().Be(deviceActor2);
        }
    }
}