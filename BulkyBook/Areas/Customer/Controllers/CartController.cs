using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using BulkyBook.Utility;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(U => U.ApplicationUserId == Claim.Value, includproperties: "Product"),
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                            .GetFirstOrDefault(u => u.Id == Claim.Value, includproperties: "Company");

            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQunatity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + ".....";
                }
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == Claim.Value);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "verification email is empy!");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "verification email sent, please check your email");
            return RedirectToPage("Index");
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includproperties: "Product");
            cart.Count += 1;
            cart.Price = SD.GetPriceBasedOnQunatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includproperties: "Product");

            if (cart.Count == 1)
            {
                var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = SD.GetPriceBasedOnQunatity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includproperties: "Product");

            var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(o => o.ApplicationUserId == Claim.Value, includproperties: "Product")
            };


            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u =>
                u.Id == Claim.Value,
                includproperties: "Company");

            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQunatity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
            }
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVM);
        }
    }
}
