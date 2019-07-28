namespace AkkaOnCore.Messages
{
	public abstract class MeetingCommand
	{
		public abstract TResult Match<TResult>();
		public abstract void Apply();
	}
}