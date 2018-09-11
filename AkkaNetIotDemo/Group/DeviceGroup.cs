using Akka.Actor;
using Akka.Event;
using AkkaNetIotDemo.Protocol;
using System;
using System.Collections.Generic;

namespace AkkaNetIotDemo.Group
{
    public class DeviceGroup : UntypedActor
    {
        private Dictionary<string, IActorRef> deviceIdToActor = new Dictionary<string, IActorRef>();
        private Dictionary<IActorRef, string> actorToDeviceId = new Dictionary<IActorRef, string>();
        private long nextCollectionId = 0L;

        public DeviceGroup(string groupId) => GroupId = groupId;

        protected override void PreStart() => Log.Info($"Device group {GroupId} started");

        protected override void PostStop() => Log.Info($"Device group {GroupId} stopped");

        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        protected string GroupId { get; }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case RequestAllTemperatures requestAllTemperatures:
                    Context.ActorOf(
                        Actors.DeviceGroupQuery.Props(
                            actorToDeviceId, 
                            requestAllTemperatures.RequestId, 
                            Sender, 
                            TimeSpan.FromSeconds(3)));
                    break;

                case RequestTrackDevice trackMsg when trackMsg.GroupId.Equals(GroupId):
                    if (deviceIdToActor.TryGetValue(trackMsg.DeviceId, out var actorRef))
                    {
                        actorRef.Forward(trackMsg);
                    }
                    else
                    {
                        Log.Info($"Creating device actor for {trackMsg.DeviceId}");
                        var deviceActor = Context.ActorOf(Device.Device.Props(trackMsg.GroupId, trackMsg.DeviceId), $"device-{trackMsg.DeviceId}");
                        deviceIdToActor.Add(trackMsg.DeviceId, deviceActor);
                        actorToDeviceId.Add(deviceActor, trackMsg.DeviceId);
                        Context.Watch(deviceActor);
                        deviceActor.Forward(trackMsg);
                    }
                    break;

                case RequestTrackDevice trackMsg:
                    Log.Warning($"Ignoring TrackDevice request for {trackMsg.GroupId}. This actor is responsible for {GroupId}");
                    break;

                case RequestDeviceList requestDeviceList:
                    Sender.Tell(new ReplyDeviceList(requestDeviceList.RequestId, new HashSet<string>(deviceIdToActor.Keys)));
                    break;

                case Terminated t:
                    var deviceId = actorToDeviceId[t.ActorRef];
                    actorToDeviceId.Remove(t.ActorRef);
                    deviceIdToActor.Remove(deviceId);
                    break;
            }
        }

        public static Props Props(string groupId) => Akka.Actor.Props.Create(() => new DeviceGroup(groupId));
    }
}