using System;
using System.Threading.Tasks;
using BloodManagmentSystem.Core;
using BloodManagmentSystem.Core.Models;
using BloodManagmentSystem.Core.ViewModels;
using System.Web.Mvc;
using BloodManagmentSystem.Services;
using Microsoft.AspNet.Identity;

namespace BloodManagmentSystem.Controllers
{
    public class DonorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityMessageService _emailService;

        public DonorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _emailService = new EmailService();
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new DonorFormViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DonorFormViewModel model)
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

            try
            {
                _unitOfWork.Donors.Add(donor);
                _unitOfWork.Complete();

                var message = PrepareMessage(donor);
                if (message == null)
                {
                    ViewBag.errorMessage = "Oh noes. Something terrible happened.";
                    return View("Error");
                }

                await _emailService.SendAsync(message);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ViewBag.errorMessage = "Uh oh... Something went wrong.";
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Activate(int hashCode)
        {
            var donor = _unitOfWork.Donors.GetByHashCode(hashCode);
            if (donor == null)
                return View("Error");

            donor.Confirmed = true;
            _unitOfWork.Donors.Update(donor);
            _unitOfWork.Complete();

            return RedirectToAction("Index", "Home");
        }
        
        #region Helpers

        private IdentityMessage PrepareMessage(Donor donor)
        {
            if (donor == null) return null;
            var code = donor.GetHashCode();
            return new IdentityMessage
            {
                Body = TemplateService.RenderTemplate("DonorActivation.cshtml", donor),
                Subject = "Confirm email",
                Destination = donor.Email
            };
        }

        #endregion

    }
}