using System.ComponentModel.DataAnnotations;

namespace LineNotifier.Models.Db
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string TargetType { get; set; }
        public string Target { get; set; }
        public DateTime CreatedDate { get; set; }

        public User User { get; set; }
    }
}
