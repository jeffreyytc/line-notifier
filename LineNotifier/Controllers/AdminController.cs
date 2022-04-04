using LineNotifier.Models.Db;
using LineNotifier.Services;
using Microsoft.AspNetCore.Mvc;

namespace LineNotifier.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly LineNotifierDbContext _dbContext;
        private readonly NotifyApiHelper _notifyApiHelper;

        public AdminController(IHttpClientFactory clientFactory, LineNotifierDbContext dbContext, NotifyApiHelper notifyApiHelper)
        {
            _clientFactory = clientFactory;
            _dbContext = dbContext;
            _notifyApiHelper = notifyApiHelper;
        }

        public IActionResult Index()
        {
            var count = _dbContext.Subscriptions.Count();

            ViewBag.SubscriptionCount = count;

            return View();
        }

        [HttpPost]
        public IActionResult SendMessage(string message)
        {
            var subscriptions = _dbContext.Subscriptions.ToList();

            foreach (var subscription in subscriptions)
            {
                _notifyApiHelper.Notify(subscription.AccessToken, message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
