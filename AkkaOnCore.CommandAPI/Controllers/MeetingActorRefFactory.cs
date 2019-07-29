using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace AkkaOnCore.CommandAPI.Controllers
{
	public delegate Task<IActorRef> MeetingActorRefFactory(Guid meetingId);
}