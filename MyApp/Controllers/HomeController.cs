using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Data;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Rotativa;
using Microsoft.AspNetCore.Authorization;

namespace MyApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext,
            UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor, SignInManager<IdentityUser> signInManager)
        {


            _logger = logger;
            _appDbContext = applicationDbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;

        }

        [HttpGet]
        public IActionResult Index()
        {


            var userId = _userManager.GetUserId(User);
            SelectedItemsViewModel model = new SelectedItemsViewModel();
            var ItemsList = _appDbContext.Items.ToList();
            model.Items = ItemsList;

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(SelectedItemsViewModel model)
        {
            if (model.SelecedIds != null)
            {

                string[] ItemsIDs = model.SelecedIds.Split(',');
                int[] myInts = Array.ConvertAll(ItemsIDs, int.Parse);


                var selected = model.Items.Where(t => myInts.Contains(t.ItemD)).ToList();

                if (selected != null && selected.Count > 0)
                {
                    List<Order_Details> order_DetailsList = new List<Order_Details>();

                    var userId = _userManager.GetUserId(User);
                    var CheckUserHasOrder = _appDbContext.order_Header.FirstOrDefault(i => i.CustomerID == userId && i.Status == "Pending");
                    if (CheckUserHasOrder != null)
                    {
                        foreach (var item in selected)
                        {
                            Order_Details order_Details = new Order_Details();
                            order_Details.ItemName = item.Name;
                            order_Details.ItemID = item.ItemD;
                            order_Details.ItemPrice = item.Price;
                            order_Details.Quantity = item.Quantity;
                            order_Details.TotalPrice = (item.Price - item.Discount) * item.Quantity;
                            order_Details.Uom = item.Uom;
                            order_Details.Discount = item.Discount;
                            order_Details.Order_Header = CheckUserHasOrder;
                            order_DetailsList.Add(order_Details);
                        }
                        _appDbContext.order_Details.AddRange(order_DetailsList);
                        _appDbContext.SaveChanges();
                        return RedirectToAction("GetCardItems", "Home", new { @OrderID = CheckUserHasOrder.Order_Header_ID });
                    }
                    else
                    {
                        Order_Header order_Header = new Order_Header();
                        order_Header.OrderDate = DateTime.Now;
                        order_Header.CustomerID = userId;
                        order_Header.Status = "Pending";
                        order_Header.Tax = 14;

                        foreach (var item in selected)
                        {
                            Order_Details order_Details = new Order_Details();
                            order_Details.ItemName = item.Name;
                            order_Details.ItemID = item.ItemD;
                            order_Details.ItemPrice = item.Price;
                            order_Details.Quantity = item.Quantity;
                            order_Details.TotalPrice = (item.Price - item.Discount) * item.Quantity;
                            order_Details.Uom = item.Uom;
                            order_Details.Discount = item.Discount;
                            order_DetailsList.Add(order_Details);

                        }
                        order_Header.Order_Details = order_DetailsList;

                        _appDbContext.order_Header.Add(order_Header);
                        _appDbContext.SaveChanges();

                        return RedirectToAction("GetCardItems", "Home", new { @OrderID = order_Header.Order_Header_ID });

                    }

                }

            }
            return View();
        }

        [HttpGet]
        public IActionResult AddItem()
        {
            List<UOM> UomList = _appDbContext.uOM.ToList();
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (UOM uom in UomList)
            {
                list.Add(new SelectListItem()
                {
                    Text = uom.Uom.ToString(),
                    Value = uom.Uom.ToString()
                });
            }
            ViewBag.UomList = list;



            return View();
        }
        [HttpPost]
        public IActionResult AddItem(Items item)
        {
            if (ModelState.IsValid)
            {
                this._appDbContext.Items.Add(item);
                this._appDbContext.SaveChanges();
            }
            return RedirectToAction("AllItems");
        }

        [HttpGet]
        public IActionResult AllItems()
        {
            try
            {
                var Items = _appDbContext.Items.ToList();
                return View(Items);
            }
            catch { }

            return View(null);

        }
        [HttpPost]
        public IActionResult ConfirmOrder(decimal TotalDiscount, decimal TotalValue)
        {
            GetUserOrder();
            var OrderHeaderID = SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext.Session, "OrderHeaderID");

            var entity = _appDbContext.order_Header.FirstOrDefault(item => item.Order_Header_ID == Convert.ToInt32(OrderHeaderID));
            if (entity != null)
            {
                entity.Status = "Closed";
                entity.TotalDiscount = TotalDiscount;
                entity.TotalValue = TotalValue + (decimal.Parse(0.14.ToString()) * TotalValue);
                _appDbContext.SaveChanges();
            }

            return RedirectToAction("GetCardItems", "Home", new { @OrderID = OrderHeaderID });

        }

        [HttpPost]
        public IActionResult DeleteOrderItem(int Order_Details_ID)
        {
            GetUserOrder();
            var OrderHeaderID = SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext.Session, "OrderHeaderID");
            var DeletedOrderDetails = new Order_Details { Order_Details_ID = Order_Details_ID };
            _appDbContext.order_Details.Attach(DeletedOrderDetails);
            _appDbContext.order_Details.Remove(DeletedOrderDetails);
            _appDbContext.SaveChanges();

            var CurrentOrderItems = _appDbContext.order_Details
                                    .Where(o => o.Order_Header.Order_Header_ID == Convert.ToInt32(OrderHeaderID)).Count();
            if (CurrentOrderItems == 0)
            {
                var DeletedOrderHeader = new Order_Header { Order_Header_ID = Convert.ToInt32(OrderHeaderID) };
                _appDbContext.order_Header.Remove(DeletedOrderHeader);
                _appDbContext.SaveChanges();

            }
                                 
            return RedirectToAction("GetCardItems");
        }

        public void GetUserOrder()
        {
            try
            {

                var userId = _userManager.GetUserId(User);

                if (userId != null)
                {
                    var Order = _appDbContext.order_Header.FirstOrDefault(i => i.CustomerID == userId && i.Status == "Pending");
                    if (Order != null)
                    {
                        SessionHelper.SetObjectAsJson(_httpContextAccessor.HttpContext.Session, "OrderHeaderID", Order.Order_Header_ID.ToString());
                    }
                    else
                    {
                        var ClosdedOrder = _appDbContext.order_Header.Where(i => i.CustomerID == userId && i.Status == "Closed")
                                             .OrderByDescending(u => u.Order_Header_ID).FirstOrDefault();
                        if (ClosdedOrder != null)
                        {
                            SessionHelper.SetObjectAsJson(_httpContextAccessor.HttpContext.Session, "OrderHeaderID", ClosdedOrder.Order_Header_ID);
                        }
                        else
                        {
                            SessionHelper.SetObjectAsJson(_httpContextAccessor.HttpContext.Session, "OrderHeaderID", null);
                        }
                    }
                }
            }

            catch (Exception e) { }
        }

        public IActionResult GetCardItems(int OrderID)
        {
            GetUserOrder();
            int OrderHeaderID = 0;
            if (OrderID == 0)
            {
                if (SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext.Session, "OrderHeaderID") != null)
                {
                    OrderHeaderID = Convert.ToInt32(SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext.Session, "OrderHeaderID"));
                }
            }
            else
            {
                OrderHeaderID = OrderID;
            }
            if (OrderHeaderID != 0)
            {

                var OrderItems = _appDbContext.order_Details.Where(x => x.Order_Header.Order_Header_ID == OrderHeaderID).ToList();
                if (OrderItems != null)
                {
                    var OrderHeader = _appDbContext.order_Header.Where(x => x.Order_Header_ID == OrderHeaderID).FirstOrDefault();
                    if (OrderHeader != null)
                    {
                        if (OrderHeader.Status == "Pending")
                        {
                            return View("ConfirmOrder", OrderItems);
                        }
                        else if (OrderHeader.Status == "Closed")
                        {
                            return View("ClosedOrder", OrderItems);
                        }
                    }
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View("ClosedOrder", null);
            }
            return View("ClosedOrder", null);
        }

        [HttpPost]
        public IActionResult ChangeQuantity(int ItemID, bool IsIncrease)
        {
            int OrderHeaderID = Convert.ToInt32(SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext.Session, "OrderHeaderID"));
            var EditedItem = _appDbContext.order_Details.FirstOrDefault(a => a.ItemID == ItemID && a.Order_Header.Order_Header_ID == OrderHeaderID);
            if (IsIncrease)
            {
                EditedItem.Quantity = EditedItem.Quantity + 1;
                EditedItem.TotalPrice = (EditedItem.Quantity ) * (EditedItem.ItemPrice - EditedItem.Discount);
            }
            else
            {
                if (EditedItem.Quantity > 1)
                {
                    EditedItem.Quantity = EditedItem.Quantity - 1;
                    EditedItem.TotalPrice = (EditedItem.Quantity ) * (EditedItem.ItemPrice - EditedItem.Discount);

                }
            }
            _appDbContext.SaveChanges();
            return RedirectToAction("GetCardItems");
        }

    }
}
