namespace AkkaOnCore.Messages
{
	public class StartMeetingCommand
	{
		public string Name { get; private set; }

		public StartMeetingCommand(string name)
		{
			Name = name;
		}
	}
}