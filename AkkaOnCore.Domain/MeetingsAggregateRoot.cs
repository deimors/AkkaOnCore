using System;
using System.Collections.Generic;
using System.Linq;
using AkkaOnCore.Messages;
using Functional;

namespace AkkaOnCore.Domain
{
	public class MeetingsAggregateRoot
	{
		private readonly IDictionary<Guid, string> _meetings = new Dictionary<Guid, string>();

		public Result<IEnumerable<MeetingsEvent>, MeetingsCommandError> HandleCommand(MeetingsCommand command)
			=> command.Match(StartMeeting);

		private Result<IEnumerable<MeetingsEvent>, MeetingsCommandError> StartMeeting(
			MeetingsCommand.StartMeeting command)
			=> Result.Unit<MeetingsCommandError>()
				.FailIf(
					_meetings.Values.Contains(command.Name),
					() => new MeetingsCommandError($"Meeting with name '{command.Name}' already exists")
				).FailIf(
					string.IsNullOrWhiteSpace(command.Name),
					() => new MeetingsCommandError("Meeting Name cannot be Empty or Whitespace")
				).Select(_ => new MeetingsEvent[]
					{
						new MeetingsEvent.MeetingStartedEvent(Guid.NewGuid(), command.Name)
					}
					.AsEnumerable()
				);
				
		public void ApplyEvent(MeetingsEvent meetingsEvent)
			=> meetingsEvent.Apply(OnMeetingStarted);

		private void OnMeetingStarted(MeetingsEvent.MeetingStartedEvent e)
			=> _meetings.Add(e.MeetingId, e.Name);
	}

	public static class ResultExtensions
	{
		public static Result<TSuccess, TFailure> FailIf<TSuccess, TFailure>(this Result<TSuccess, TFailure> source, bool condition, Func<TFailure> failureFactory)
			=> source.Bind(success => Result.Create(!condition, success, failureFactory()));
	}
}
