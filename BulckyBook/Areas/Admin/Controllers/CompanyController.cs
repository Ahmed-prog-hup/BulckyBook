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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CompanyController(IUnitOfWork dp)
        {
            _unitofwork = dp;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitofwork.Company.GetAll().ToList();
            
            return View(objCompanyList);
        }

        public IActionResult UpSert(int? id)
        {
            if (id == 0 || id == null)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //update
                Company CompanyObj = _unitofwork.Company.Get(u => u.Id == id);
                return View(CompanyObj);
            }
        }
            // projection in EF Core
           // ViewBag.CategoryList = CategoryList;
           // ViewData["CategoryList"] = CategoryList;

        [HttpPost]
            public IActionResult UpSert(Company CompanyObj)
        {

            if (ModelState.IsValid) //Server Validation 
            {
                
                if(CompanyObj.Id==0)
                {
                    _unitofwork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitofwork.Company.Update(CompanyObj);
                }
                
                _unitofwork.Save();
                TempData["success"] = "Company Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(CompanyObj);
            }
            
        }

       

       
    #region API Calls
    [HttpGet]
    public IActionResult GetAll()
    {
        List<Company> objCompanyList = _unitofwork.Company.GetAll().ToList();
        return Json(new { data = objCompanyList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
        {
            var CompanyToBeDeleted = _unitofwork.Company.Get(u=>u.Id == id);
            if(CompanyToBeDeleted==null)
            {
                return Json(new { success = false, message = "Error, While Deleting" });
            }
            _unitofwork.Company.Remove(CompanyToBeDeleted);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
