namespace LineNotifier.Models
{
    public class NotifyStatusResult
    {
        public int status { get; set; }
        public string message { get; set; }
        public string targetType { get; set; }
        public string target { get; set; }
    }
}
