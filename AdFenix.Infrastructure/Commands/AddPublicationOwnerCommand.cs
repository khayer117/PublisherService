namespace AdFenix.Infrastructure.Commands
{
    public class AddPublicationOwnerCommand:IQueueCommand
    {
        public string Name { get; set; }
        public string HostUrl { get; set; }
    }
}
