using BloodManagmentSystem.Core;
using BloodManagmentSystem.Core.Models;
using BloodManagmentSystem.Core.ViewModels;
using Microsoft.AspNet.Identity;
using RazorEngine.Templating;
using System.Threading.Tasks;
using System.Web.Mvc;
using TemplateService = BloodManagmentSystem.Services.TemplateService;

namespace BloodManagmentSystem.Controllers
{
    public class DonorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityMessageService _emailService;

        public DonorController(IUnitOfWork unitOfWork, IIdentityMessageService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
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

        [HttpGet]
        public ActionResult Activate(int userId, int code)
        {
            var donor = _unitOfWork.Donors.Get(userId);
            if (donor == null)
                return HttpNotFound();

            if (donor.GetHashCode() != code)
                return View("Error");

            donor.Confirmed = true;
            _unitOfWork.Donors.Update(donor);
            _unitOfWork.Complete();

            ViewBag.Message = "Thank you for confirming you email. We are going to contact you as soon as your blood type will be needed.";
            return View("Info");
        }
        
        #region Helpers

        private IdentityMessage PrepareMessage(Donor donor)
        {
            if (donor == null) return null;
            var viewBag = new DynamicViewBag();
            viewBag.AddValue("CallbackUrl", Url.Action("Activate", "Donor", new { userId = donor.Id, code = donor.GetHashCode() }, protocol: Request.Url.Scheme));

            return new IdentityMessage
            {
                Body = TemplateService.RenderTemplate("DonorActivation.cshtml", donor, viewBag),
                Subject = "Confirm email",
                Destination = donor.Email
            };
        }

        #endregion

    }
}