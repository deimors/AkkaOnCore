namespace AkkaOnCore.Messages
{
	public abstract class MeetingEvent
	{
		public abstract TResult Match<TResult>();

		public abstract void Apply();
	}
}