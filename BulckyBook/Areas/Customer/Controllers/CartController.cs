using Bulcky.DataAccess.Data;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;
using Bulcky.Models.ViewModels;
using Bulcky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulckyBook.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationId == userId,
                includeProperties: ("Product")),
                OrderHaeder = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceOnQuantity(cart);
                ShoppingCartVM.OrderHaeder.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationId == userId,
                includeProperties: ("Product")),
                OrderHaeder = new()
            };

            ShoppingCartVM.OrderHaeder.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHaeder.Name = ShoppingCartVM.OrderHaeder.ApplicationUser.Name;
            ShoppingCartVM.OrderHaeder.City = ShoppingCartVM.OrderHaeder.ApplicationUser.City;
            ShoppingCartVM.OrderHaeder.PhoneNumber = ShoppingCartVM.OrderHaeder.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHaeder.PostalCode = ShoppingCartVM.OrderHaeder.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHaeder.State = ShoppingCartVM.OrderHaeder.ApplicationUser.State;
            ShoppingCartVM.OrderHaeder.StreetAddress = ShoppingCartVM.OrderHaeder.ApplicationUser.StreetAddress;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceOnQuantity(cart);
                ShoppingCartVM.OrderHaeder.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationId == userId,
                includeProperties: ("Product"));
            ShoppingCartVM.OrderHaeder.ApplicationId = userId;
            ShoppingCartVM.OrderHaeder.OrederDate = System.DateTime.Now;

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
			
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceOnQuantity(cart);
				ShoppingCartVM.OrderHaeder.OrderTotal += (cart.Price * cart.Count);
			}
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer
                ShoppingCartVM.OrderHaeder.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHaeder.OrderStatus = SD.StatusPending;
            }
            else
            {
                // it is a compny user
                ShoppingCartVM.OrderHaeder.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHaeder.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHaeder);
            _unitOfWork.Save();

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    Count = cart.Count,
                    Price = cart.Price,
                    OrderHeaderId = ShoppingCartVM.OrderHaeder.Id
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer
                //need to capture payment
                var domain = "https://localhost:7244/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain+$"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHaeder.Id}",
                    CancelUrl = domain +$"customer/cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

                foreach(var item in ShoppingCartVM.ShoppingCartList)
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
                _unitOfWork.OrderHeader.UpdateStripPaymentId(ShoppingCartVM.OrderHaeder.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

			}
			return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVM.OrderHaeder.Id});
		}
        public IActionResult OrderConfirmation (int id)
        {
            OrderHaeder orderHaeder = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties:("ApplicationUser"));
            if (orderHaeder.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHaeder.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
					_unitOfWork.OrderHeader.UpdateStripPaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                    HttpContext.Session.Clear();
                }

                List<ShoppingCart> shoppingCarts=_unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationId==orderHaeder.ApplicationId
                ).ToList();
                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();

            }
            
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);

            if (cartFromDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationId == cartFromDb.ApplicationId).Count() - 1);
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId,tracked:true);
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationId == cartFromDb.ApplicationId).Count() - 1);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        private double GetPriceOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
