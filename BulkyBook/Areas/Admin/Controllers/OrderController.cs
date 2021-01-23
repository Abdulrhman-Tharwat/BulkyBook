using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderDetailsVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            orderVM = new OrderDetailsVM()
            {
                orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includproperties: "ApplicationUser"),
                orderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderId == id, includproperties: "Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [ActionName("Details")]
        [ValidateAntiForgeryToken]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeader.Id, includproperties: "ApplicationUser");

            if (stripeToken != null)
            {
                var Options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var Service = new ChargeService();

                Charge charge = Service.Create(Options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();
            }
                return RedirectToAction("Details", "Order", new { id = orderHeader.Id});
        }


        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader order = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            order.OrderStatus = SD.StatusInProcess;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader order = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeader.Id);
            order.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            order.Carrier = orderVM.orderHeader.Carrier;
            order.OrderStatus = SD.StatusShipped;
            order.ShippingDate = DateTime.Now;

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader order = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

            if (order.PaymentStatus == SD.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(order.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = order.TransactionId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                order.OrderStatus = SD.StatusRefunded;
                order.PaymentStatus = SD.StatusRefunded;

            }
            else
            {
                order.OrderStatus = SD.StatusCancelled;
                order.PaymentStatus = SD.StatusCancelled;

            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var Claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includproperties: "ApplicationUser");

            }
            else
            {

                orderHeaderList = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == Claim.Value, includproperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedpayment);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(u => u.OrderStatus == SD.StatusApproved ||
                                                              u.OrderStatus == SD.StatusPending ||
                                                              u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(u => u.OrderStatus == SD.StatusShipped);

                    break;
                case "rejected":
                    orderHeaderList = orderHeaderList.Where(u => u.OrderStatus == SD.StatusCancelled ||
                                                              u.OrderStatus == SD.StatusRefunded ||
                                                              u.OrderStatus == SD.PaymentStatusRejected);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaderList });
        }
        #endregion
    }
}
