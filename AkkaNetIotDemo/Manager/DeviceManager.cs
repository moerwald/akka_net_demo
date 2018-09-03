using Akka.Actor;
using Akka.Event;
using AkkaNetIotDemo.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaNetIotDemo.Manager
{
    public class DeviceManager : UntypedActor
    {
        private Dictionary<string, IActorRef> groupToActor = new Dictionary<string, IActorRef>();
        private Dictionary<IActorRef, string> actorToGroup = new Dictionary<IActorRef, string>();

        protected override void PreStart() => Log.Info("DeviceManager started");
        protected override void PostStop() => Log.Info("DeviceManager stopped");

        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case RequestTrackDevice requestTrackDevice:
                    if (groupToActor.TryGetValue(requestTrackDevice.GroupId, out var actorRef))
                    {
                        actorRef.Forward(requestTrackDevice);
                    }
                    else
                    {
                        Log.Info($"Creating device group actor for {requestTrackDevice.GroupId}");
                        var groupActor = Context.ActorOf(Group.DeviceGroup.Props(requestTrackDevice.GroupId));
                        Context.Watch(groupActor);
                        groupToActor.Add(requestTrackDevice.GroupId, groupActor);
                        actorToGroup.Add(groupActor, requestTrackDevice.GroupId);
                        groupActor.Forward(requestTrackDevice);
                    }
                    break;

                case Terminated t:
                    var groupId = actorToGroup[t.ActorRef];
                    Log.Info($"Device group actor for {groupId} has been terminated");
                    actorToGroup.Remove(t.ActorRef);
                    groupToActor.Remove(groupId);
                    break;
            }
        }

        public static Props Props(string groupId) => Akka.Actor.Props.Create<DeviceManager>();
    }
}
