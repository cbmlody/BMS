using BloodManagmentSystem.Core;
using BloodManagmentSystem.Core.Models;
using BloodManagmentSystem.Core.ViewModels;
using System.Web.Mvc;

namespace BloodManagmentSystem.Controllers
{
    public class DonorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DonorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new DonorFormViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DonorFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            var donor = new Donor
            {
                Name = model.Name,
                Email = model.Email,
                City = model.City,
                BloodType = model.BloodType,
                Confirmed = false
            };

            _unitOfWork.Donors.Add(donor);
            _unitOfWork.Complete();

            return RedirectToAction("Index", "Home");
        }
    }
}