using Bulcky.DataAccess.Repository;
using Bulcky.DataAccess.Repository.IRepository;
using Bulcky.Models;
using Bulcky.Models.ViewModels;
using Bulcky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Data;

namespace BulckyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork dp, IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = dp;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties:"Category").ToList();
            
            return View(objProductList);
        }

        public IActionResult UpSert(int? id)
        {

            ProductVM productVM = new()
            {
                CategoryList = _unitofwork.Category.GetAll().
                Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == 0 || id == null)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitofwork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
            // projection in EF Core
           // ViewBag.CategoryList = CategoryList;
           // ViewData["CategoryList"] = CategoryList;

        [HttpPost]
            public IActionResult UpSert(ProductVM productvm, IFormFile? file)
        {

            if (ModelState.IsValid) //Server Validation 
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string ProductPath = Path.Combine(wwwRootPath, @"Images\Product");
                    if (!string.IsNullOrEmpty(productvm.Product.ImageUrl))
                    {
                        var oldImagePath = 
                            Path.Combine(wwwRootPath,productvm.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    
                    using (var fileSteram = new FileStream(Path.Combine(ProductPath, fileName),FileMode.Create))
                    {
                        file.CopyTo(fileSteram);
                    }
                    productvm.Product.ImageUrl = @"\Images\Product\" + fileName;
                }
                if(productvm.Product.Id==0)
                {
                    _unitofwork.Product.Add(productvm.Product);
                }
                else
                {
                    _unitofwork.Product.Update(productvm.Product);
                }
                
                _unitofwork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productvm.CategoryList = _unitofwork.Category.GetAll().
                Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productvm);
            }
            
        }

       

       
    #region API Calls
    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product> objProductList = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
        return Json(new { data = objProductList });
    }

    //[HttpDelete]
    public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitofwork.Product.Get(u=>u.Id == id);
            if(productToBeDeleted==null)
            {
                return Json(new { success = false, message = "Error, While Deleting" });
            }
            var oldImagePath =
                    Path.Combine(_webHostEnvironment.WebRootPath,
                    productToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitofwork.Product.Remove(productToBeDeleted);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
