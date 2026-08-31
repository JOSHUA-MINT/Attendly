using Microsoft.AspNetCore.Mvc;

namespace Attendly.Controllers;

public class HomeController : Controller
{
 [HttpGet("")]
 public IActionResult Index()
 {
 if (User.Identity?.IsAuthenticated == true)
 return RedirectToAction("Index", "Dashboard");

 return View();
 }

 [HttpGet("features")]
 public IActionResult Features()
 {
 return View();
 }

 [HttpGet("how-it-works")]
 public IActionResult HowItWorks()
 {
 return View();
 }

 [HttpGet("premium")]
 public IActionResult Premium()
 {
 return RedirectToAction("Index", "Premium");
 }

 [HttpGet("connect")]
 public IActionResult Connect()
 {
 if (User.Identity?.IsAuthenticated != true)
 return RedirectToAction("Login", "Account");
 return RedirectToAction("Index", "Connect");
 }

 [HttpGet("privacy")]
 public IActionResult Privacy()
 {
 return View();
 }

 [HttpGet("terms")]
 public IActionResult Terms()
 {
 return View();
 }
}
