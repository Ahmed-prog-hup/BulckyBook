using Bulcky.DataAccess.Repository;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;
using Bulcky.Models.ViewModels;
using Bulcky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulckyBook.Areas.Admin.Controllers
{
	[Area("admin")]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; } 
        public OrderController(IUnitOfWork unitOfWork)
        {
				_unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHaeder = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin +","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHaeder.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHaeder.Name;
            orderHeaderFromDb.PhoneNumber=OrderVM.OrderHaeder.PhoneNumber;
            orderHeaderFromDb.StreetAddress=OrderVM.OrderHaeder.StreetAddress;
            orderHeaderFromDb.City=OrderVM.OrderHaeder.City;
            orderHeaderFromDb.State=OrderVM.OrderHaeder.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHaeder.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHaeder.Carrier))
            {
                orderHeaderFromDb.Carrier=OrderVM.OrderHaeder.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHaeder.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHaeder.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order details Updated Successfully";
            return RedirectToAction(nameof(Details),new {orderId = orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult startProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHaeder.Id, SD.StatusProcces);
            _unitOfWork.Save();
            TempData["Success"] = "Status Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHaeder.Id });

        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHaeder.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHaeder.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHaeder.Carrier;
            orderHeader.OrderStatus = SD.StatusShiped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shiped Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHaeder.Id });

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancellOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHaeder.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId,
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Shiped Successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHaeder.Id });
        }

        [HttpPost]
        [ActionName("Details")]
        public IActionResult Details_Pay_Now()
        {
             OrderVM.OrderHaeder = _unitOfWork.OrderHeader.
                Get(u => u.Id == OrderVM.OrderHaeder.Id,includeProperties:"ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.
                GetAll(u => u.OrderHeaderId == OrderVM.OrderHaeder.Id, includeProperties: "Product");

            var domain = "https://localhost:7244/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?OrderHeaderId={OrderVM.OrderHaeder.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHaeder.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripPaymentId(OrderVM.OrderHaeder.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHaeder orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripPaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }

            }
            return View(orderHeaderId);
        }

        #region API Calls
        [HttpGet]
		public IActionResult GetAll(string Status)
		{
            IEnumerable<OrderHaeder> objOrderHeaders;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader.GetAll
                    (u => u.ApplicationId == userId, includeProperties: "ApplicationUser");
            }

                switch (Status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusProcces); 
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShiped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new
            {
                data = objOrderHeaders
            });
        }
		#endregion
	}
}
