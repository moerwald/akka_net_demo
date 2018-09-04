using Akka.Actor;
using Akka.Event;
using AkkaNetIotDemo.Protocol;
using System;
using System.Collections.Generic;

namespace AkkaNetIotDemo.Actors
{
    public class DeviceGroupQuery : UntypedActor
    {
        private ICancelable queryTimeoutTimer;

        public DeviceGroupQuery(
            Dictionary<IActorRef, string> actorToDeviceId,
            long requestId,
            IActorRef requester,
            TimeSpan timeout)
        {
            ActorToDeviceId = actorToDeviceId ?? throw new ArgumentNullException(nameof(actorToDeviceId));
            RequestId = requestId;
            Requester = requester ?? throw new ArgumentNullException(nameof(requester));
            Timeout = timeout;

            queryTimeoutTimer = Context.System.Scheduler.ScheduleTellOnceCancelable(timeout, Self, CollectionTimeout.Instance, Self);

            Become(WaitingForReplies(new Dictionary<string, ITemperatureReading>(), new HashSet<IActorRef>(actorToDeviceId.Keys)));
        }

        public Dictionary<IActorRef, string> ActorToDeviceId { get; }
        public IActorRef Requester { get; }
        public long RequestId { get; }
        public TimeSpan Timeout { get; }

        protected ILoggingAdapter Log { get; } = Context.GetLogger();

        public static Props Props(
            Dictionary<IActorRef, string> actorToDevice,
            long requestId,
            IActorRef requester,
            TimeSpan timeout)
            => Akka.Actor.Props.Create(() => new DeviceGroupQuery(actorToDevice, requestId, requester, timeout));

        public void ReceivedResponse(
            IActorRef deviceActor,
            ITemperatureReading reading,
            HashSet<IActorRef> stillWaiting,
            Dictionary<string, ITemperatureReading> repliesSoFar)
        {
            Context.Unwatch(deviceActor);
            var deviceId = ActorToDeviceId[deviceActor];
            stillWaiting.Remove(deviceActor);

            repliesSoFar.Add(deviceId, reading);
            if (stillWaiting.Count == 0)
            {
                Requester.Tell(new RespondAllTemperatures(RequestId, repliesSoFar));
                Context.Stop(Self);
            }
            else
            {
                Context.Become(WaitingForReplies(repliesSoFar, stillWaiting));
            }
        }

        public UntypedReceive WaitingForReplies(
            Dictionary<string, ITemperatureReading> repliesSoFar, HashSet<IActorRef> stillWaiting)
        {
            return message =>
            {
               switch (message)
               {
                   case RespondTemperature response when response.RequestId == 0:
                       var deviceActor = Sender;
                       ITemperatureReading temperatureReading = null;
                       if (response.Value.HasValue)
                       {
                           temperatureReading = new Temperature(response.Value.Value);
                       }
                       else
                       {
                           temperatureReading = TemperatureNotAvailable.Instance;
                       }

                       ReceivedResponse(deviceActor, temperatureReading, stillWaiting, repliesSoFar);
                       break;

                   case Terminated terminated:
                       ReceivedResponse(terminated.ActorRef, DeviceNotAvailable.Instance, stillWaiting, repliesSoFar);
                       break;

                   case CollectionTimeout collectionTimeout:
                       var replies = new Dictionary<string, ITemperatureReading>(repliesSoFar);
                       foreach (var actor in stillWaiting)
                       {
                           var deviceId = ActorToDeviceId[actor];
                           replies.Add(deviceId, DeviceTimeOut.Instance);
                       }
                       Requester.Tell(new RespondAllTemperatures(RequestId, replies));
                       Context.Stop(Self);


                       break;
               }
           };
        }

        protected override void OnReceive(object message)
        {
        }

        protected override void PostStop() => queryTimeoutTimer.Cancel();

        protected override void PreStart()
        {
            foreach (var deviceActor in ActorToDeviceId.Keys)
            {
                Context.Watch(deviceActor);
                deviceActor.Tell(new ReadTemperature(0, null));
            }
        }
    }
}