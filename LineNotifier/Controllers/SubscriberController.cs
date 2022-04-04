using LineNotifier.Models;
using LineNotifier.Models.Db;
using LineNotifier.Services;
using Microsoft.AspNetCore.Mvc;

namespace LineNotifier.Controllers
{
    public class SubscriberController : Controller
    {
        private readonly string _loggedInUserIdSessionKey = "LoggedInUserId";

        private readonly IHttpClientFactory _clientFactory;
        private readonly LineNotifierDbContext _dbContext;
        private readonly LoginApiHelper _loginApiHelper;
        private readonly NotifyApiHelper _notifyApiHelper;

        public SubscriberController(IHttpClientFactory clientFactory, LineNotifierDbContext dbContext, LoginApiHelper loginApiHelper, NotifyApiHelper notifyApiHelper)
        {
            _clientFactory = clientFactory;
            _dbContext = dbContext;
            _loginApiHelper = loginApiHelper;
            _notifyApiHelper = notifyApiHelper;
        }

        public IActionResult Login()
        {
            return Redirect(_loginApiHelper.GetAuthUrl());
        }

        public IActionResult LoginCallback(string code)
        {
            var tokenResult = _loginApiHelper.GetTokenResult(code);
            var verifyResult = _loginApiHelper.GetVerifyResult(tokenResult.id_token);

            var user = _dbContext.Users.Find(verifyResult.sub);

            if (user == null)
            {
                user = new User()
                {
                    LineUserId = verifyResult.sub,
                    Name = verifyResult.name,
                    Picture = verifyResult.picture
                };
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
            }

            HttpContext.Session.SetString(_loggedInUserIdSessionKey, user.LineUserId);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString(_loggedInUserIdSessionKey);
            if (string.IsNullOrEmpty(userId)) { return RedirectToAction(nameof(Login)); }

            var user = _dbContext.Users.Find(userId);
            if (user == null) { return RedirectToAction(nameof(Login)); }

            var subscriptions = _dbContext.Entry(user).Collection(u => u.Subscriptions).Query().ToList();

            ViewBag.User = user;
            ViewBag.Subscriptions = subscriptions;

            return View();
        }

        public IActionResult Add()
        {
            var userId = HttpContext.Session.GetString(_loggedInUserIdSessionKey);
            if (string.IsNullOrEmpty(userId)) { return RedirectToAction(nameof(Login)); }

            return base.Redirect(_notifyApiHelper.GetAuthUrl());
        }

        public IActionResult NotifyCallback(string code)
        {
            var userId = HttpContext.Session.GetString(_loggedInUserIdSessionKey);
            if (string.IsNullOrEmpty(userId)) { return RedirectToAction(nameof(Login)); }

            var user = _dbContext.Users.Find(userId);
            if (user == null) { return RedirectToAction(nameof(Login)); }

            var tokenResult = _notifyApiHelper.GetTokenResult(code);
            var statusResult = _notifyApiHelper.GetStatusResult(tokenResult.access_token);

            var subscription = new Subscription()
            {
                CreatedDate = DateTime.Now,
                AccessToken = tokenResult.access_token,
                TargetType = statusResult.targetType,
                Target = statusResult.target
            };
            user.Subscriptions.Add(subscription);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Unsubscribe(int id)
        {
            var subscription = _dbContext.Subscriptions.Find(id);

            if (subscription == null) { return NotFound(); }

            _notifyApiHelper.Revoke(subscription.AccessToken);

            _dbContext.Subscriptions.Remove(subscription);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove(_loggedInUserIdSessionKey);

            return RedirectToAction("Index", "Home");
        }
    }
}
