using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> ProductList = _unitOfWork.Product.GetAll(includproperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
            }
            return View(ProductList);
        }
        public IActionResult Details(int id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includproperties: "Category,CoverType");
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = obj,
                ProductId = obj.Id
            };
            return View(cartObj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart Shopping)
        {
            Shopping.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                Shopping.ApplicationUserId = claim.Value;
                ShoppingCart cartFromDDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                        u => u.ApplicationUserId == Shopping.ApplicationUserId && u.ProductId == Shopping.ProductId,
                        includproperties: "Product"
                    );
                if (cartFromDDb == null)
                {
                    _unitOfWork.ShoppingCart.Add(Shopping);
                }
                else
                {
                    cartFromDDb.Count += Shopping.Count;
                    //_unitOfWork.ShoppingCart.Update(cartFromDDb);
                }
                _unitOfWork.Save();
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == Shopping.ApplicationUserId).ToList().Count();
                //HttpContext.Session.SetObject(SD.ssShoppingCart, Shopping);
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == Shopping.ProductId, includproperties: "Category,CoverType");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = obj,
                    ProductId = obj.Id
                };
                return View(cartObj);

            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
