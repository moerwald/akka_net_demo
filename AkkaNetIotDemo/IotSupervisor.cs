
using Akka.Actor;
using Akka.Event;

namespace AkkaNetIotDemo
{
    public class IotSupervisor : UntypedActor
    {
        public ILoggingAdapter Log { get; } = Context.GetLogger();

        protected override void PreStart() => Log.Info("Iot Application started");
        protected override void PostStop() => Log.Info("Iot Application stopped");

        protected override void OnReceive(object message)
        {
            // Nothing to do
        }

        public static Props Props() => Akka.Actor.Props.Create<IotSupervisor>();
    }
}
