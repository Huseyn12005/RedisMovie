using Microsoft.AspNetCore.Mvc;
using OMDbApiNet;
using StackExchange.Redis;

namespace RedisMovie.Controllers
{
    public class RedisController : Controller
    {
        private readonly IDatabase _db;
        private readonly OmdbClient _omdbClient;

        public RedisController()
        {
            var muxer = ConnectionMultiplexer.Connect(
              new ConfigurationOptions
              {
                  EndPoints = { { "redis-10768.c323.us-east-1-2.ec2.redns.redis-cloud.com", 10768 } },
                  User = "default",
                  Password = "qvLRaiX3VlR5labk4EjUGNIVZZpbsG5e"
              }
          );
            _db = muxer.GetDatabase();

            _omdbClient = new OmdbClient("b397a378");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string movieName)
        {
            if (string.IsNullOrWhiteSpace(movieName))
            {
                ViewBag.Message = "Please enter a movie name!";
                return View();
            }

            try
            {
                var movie = _omdbClient.GetItemByTitle(movieName);

                if (movie == null || string.IsNullOrEmpty(movie.Poster) || movie.Poster == "N/A")
                {
                    ViewBag.Message = "Movie not found or no poster available!!!";
                    return View();
                }

                string listKey = "myList";
                _db.ListRightPush(listKey, movie.Poster);

                ViewBag.Message = "Poster added successfully!!!";
                return RedirectToAction("Posters");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View();
            }
        }

        public IActionResult Posters()
        {
            string listKey = "myList";
            var posters = new List<string>();

            foreach (var item in _db.ListRange(listKey, 0, -1))
            {
                posters.Add(item.ToString());
            }

            return View(posters); 
        }
    }
}
