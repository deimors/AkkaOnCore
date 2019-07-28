namespace AkkaOnCore.Messages
{
	public class MeetingCommandError
	{
		public string Message { get; }

		public MeetingCommandError(string message)
		{
			Message = message;
		}
	}
}