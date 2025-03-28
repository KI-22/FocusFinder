using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FocusFinderApp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;

namespace FocusFinderApp.Controllers;

public class ReviewController : Controller
{
    private readonly ILogger<ReviewController> _logger;
    private readonly FocusFinderDbContext _dbContext;

    public ReviewController(ILogger<ReviewController> logger, FocusFinderDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        Console.WriteLine("ReviewController instantiated!");
    }


    [Route("/Reviews")]
    [HttpGet]
    public IActionResult AllReviews()
    {
        ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
        int? currentUserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");

        var reviews = _dbContext.Reviews
            .Where( l => l.userId == currentUserId)
            .ToList();

        if (reviews == null)
        {
            Console.WriteLine("Location not found");
            return RedirectToAction("Index");
        }
        else
        {
            return View("~/Views/Reviews/AllReviews.cshtml", reviews);
        }

    }

 
    [Route("/Reviews/{id}")]
    [HttpGet]
    public IActionResult IndivReview(int id)
    {

        ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
        int? currentUserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.currentUserId = currentUserId;

        Console.WriteLine("Review Id: " + id);

        if (id <= 0)
        {
            Console.WriteLine("Review ID not found");
            return View("~/Views/Reviews/IndivReview.cshtml");
        }
        else {
            var review = _dbContext.Reviews
            .FirstOrDefault(l => l.id == id);

            if (review == null)
            {
                Console.WriteLine("Review not found");
                return View("~/Views/Reviews/IndivReview.cshtml");
            }
            else{
                var location =_dbContext.Locations
                .FirstOrDefault(l => l.Id == review.locationId);

            if (location == null)
            {
                ViewBag.LocationName = "no location found";
                Console.WriteLine("Location Name: NOT found");
            }
            else{
                ViewBag.Location = location;
                Console.WriteLine("Location Name: " + ViewBag.Location.LocationName);
            }
            ViewBag.Review = review;
            Console.WriteLine("TBC 'review': " + review);

            return View("~/Views/Reviews/IndivReview.cshtml");
            }
        }
    }



    [HttpPost]
    public IActionResult AddExtReview(
        int locationId,
        string? comments = null,
        int rating = -1,
        int cleanliness = -1,
        int noiseLevel = -1,
        int wifiSpeed = -1,
        int chargingPointAvailability = -1,
        int seatingAvailability = -1,
        bool? petFriendly = null,
        bool? groupFriendly = null,
        bool? queerFriendly = null,
        bool? homeLike = null,
        bool? officeLike = null,
        bool? veganFriendly = null,
        bool? glutenFreeOptions = null,
        bool? neurodivergentFriendly = null,
        bool? airConditioning = null,
        bool? heating= null,
        bool? wheelchairAccessible = null,
        bool? babychanging = null,
        bool? toilets = null,
        bool? freeWifi = null
        )
    {
        Console.WriteLine("Reached - 'AddExtReview' HttpPost");
        ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        Console.WriteLine("loc id: " + locationId);

        var newReview = new Review
        {
            userId = currentUserId,
            locationId = locationId,
            comments = comments
        };

        if (rating != -1)
        {
            newReview.overallRating = rating;
        }
        if (cleanliness != -1)
        {
            newReview.cleanliness = cleanliness;
        }
        if (noiseLevel != -1)
        {
            newReview.noiseLevel = noiseLevel;
        }
        if (wifiSpeed != -1)
        {
            newReview.wifiSpeed = wifiSpeed;
        }
        if (chargingPointAvailability != -1)
        {
            newReview.chargingPointAvailability = chargingPointAvailability;
        }
        if (seatingAvailability != -1)
        {
            newReview.seatingAvailability = seatingAvailability;
        }
        Console.WriteLine($"petFriendly = {petFriendly}");
        if (
            (petFriendly ?? false) ||
            (groupFriendly ?? false) ||
            (queerFriendly ?? false) ||
            (homeLike ?? false) ||
            (officeLike ?? false) ||
            (veganFriendly ?? false) ||
            (glutenFreeOptions ?? false) ||
            (neurodivergentFriendly ?? false) ||
            (airConditioning ?? false) ||
            (heating ?? false) ||
            (wheelchairAccessible ?? false) ||
            (babychanging ?? false) ||
            (toilets ?? false) ||
            (freeWifi ?? false)
            ) {
                petFriendly ??= false;
                groupFriendly ??= false;
                queerFriendly ??= false;
                homeLike ??= false;
                officeLike ??= false;
                veganFriendly ??= false;
                glutenFreeOptions ??= false;
                neurodivergentFriendly ??= false;
                airConditioning ??= false;
                heating ??= false;
                wheelchairAccessible ??= false;
                babychanging ??= false;
                toilets ??= false;
                freeWifi ??= false;
                newReview.petFriendly = petFriendly;
                newReview.groupFriendly = groupFriendly;
                newReview.queerFriendly = queerFriendly;
                newReview.homeLike = homeLike;
                newReview.officeLike = officeLike;
                newReview.veganFriendly = veganFriendly;
                newReview.glutenFreeOptions = glutenFreeOptions;
                newReview.neurodivergentFriendly = neurodivergentFriendly;
                newReview.airConditioning = airConditioning;
                newReview.heating = heating;
                newReview.wheelchairAccessible = wheelchairAccessible;
                newReview.babychanging = babychanging;
                newReview.toilets = toilets;
                newReview.freeWifi = freeWifi;
            }

        _dbContext.Reviews.Add(newReview);
        _dbContext.SaveChanges();
        Achievement.UpdateUserAchievements(_dbContext, currentUserId.Value, "review");
        Console.WriteLine("Review added");
        
        // return View("~/Views/Reviews/AllReviews.cshtml");
        return Redirect("/Reviews");
    }




    [Route("/Reviews/NewReview")]
    [HttpGet]
    public IActionResult NewReviewForm(int locationId)
    {
        Console.WriteLine("Reached - HttpGet /Reviews/NewReview");
        ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;

        Console.WriteLine("loc id: " + locationId);
        ViewBag.locId = locationId;
        
        return View("~/Views/Reviews/NewReview.cshtml");
    }

}