using System.ComponentModel.DataAnnotations;

namespace LineNotifier.Models.Db
{
    public class User
    {
        public User()
        {
            Subscriptions = new HashSet<Subscription>();
        }

        [Key]
        public string LineUserId { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
