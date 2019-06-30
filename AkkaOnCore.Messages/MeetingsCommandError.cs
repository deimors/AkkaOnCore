namespace AkkaOnCore.Messages
{
	public class MeetingsCommandError
	{
		public string Message { get; }

		public MeetingsCommandError(string message)
		{
			Message = message;
		}
	}
}