using BloodManagmentSystem.Core;
using BloodManagmentSystem.Core.Models;
using BloodManagmentSystem.Core.ViewModels;
using BloodManagmentSystem.Services;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BloodManagmentSystem.Controllers
{
    public class BloodRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailService _emailService;

        public BloodRequestController(IUnitOfWork unitOfWork, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var requests = _unitOfWork.Requests.GetAllInProgressRequests();
            return View(requests);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new BloodRequestFormViewModel
            {
                Banks = _unitOfWork.Banks.GetBloodBanks()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BloodRequestFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Banks = _unitOfWork.Banks.GetBloodBanks();
                return View("Create", model);
            }

            var request = new BloodRequest
            {
                BloodType = model.BloodType,
                DueDateTime = model.GetDueDateTime(),
                City = model.City,
                BankId = model.Bank
            };

            _unitOfWork.Requests.Add(request);
            _unitOfWork.Complete();
            
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var request = _unitOfWork.Requests.GetRequest(id);
            var requestDetails = new BloodRequestDetailsViewModel
            {
                Request = request,
                Donors = _unitOfWork.Donors.GetDonorsByBloodType(request.BloodType)
            };

            ViewBag.Alert = TempData["Alert"];

            return View(requestDetails);
        }

        public ActionResult Notify(int id)
        {
            try
            {
                var request = _unitOfWork.Requests.GetRequest(id);
                if (request == null)
                    return View("Error");

                var donors = _unitOfWork.Donors.GetDonorsByBloodType(request.BloodType);

                var confirmations = CreateConfirmations(donors, id);

                _unitOfWork.Confirmations.AddRange(confirmations);
                _unitOfWork.Complete();

                Task.Factory.StartNew(() => SendEmailsWithRequest(confirmations));

                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData["Alert"] = "Emails have already been sent";
                return RedirectToAction("Details", new { id=id });
            }
            
        }

        [HttpPost]
        public ActionResult Confirm(string hash)
        {
            var confirmation = _unitOfWork.Confirmations.GetByHash(hash);
            var bank = confirmation.Request.Bank;
            if (confirmation == null)
                return HttpNotFound();

            confirmation.Status = true;

            var request = confirmation.Request;

            var message = new IdentityMessage
            {
                Subject = "BMS Donation details",
                Destination = confirmation.Donor.Email,
                Body = RazorTemplateService.RenderTemplate("DonationDetails.cshtml", model: request)
            };

            Task.Run(() => _emailService.SendAsync(message));

            _unitOfWork.Confirmations.Update(confirmation);
            _unitOfWork.Complete();

            ViewBag.Message = "Thank you for your participation in this blood collection!" +
                              "We have sent you email with additional informations.";

            return View("Info");
        }

        #region PrivateMethods

        private List<Confirmation> CreateConfirmations(IEnumerable<Donor> donors, int requestId)
        {
            return donors.Select(donor => new Confirmation
            {
                Donor = donor,
                RequestId = requestId,
                HashCode = GetMd5HashCode($"{donor.Id}dnr"),
                Status = false
            }).ToList();
        }

        private string GetMd5HashCode(string input)
        {
            var md5 = MD5.Create();

            var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sb = new StringBuilder();

            foreach (var b in result)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        private async void SendEmailsWithRequest(IEnumerable<Confirmation> confirmations)
        {
            foreach (var confirmation in confirmations)
            {
                var message = new IdentityMessage
                {
                    Body = RazorTemplateService.RenderTemplate("Confirmation.cshtml", confirmation),
                    Destination = confirmation.Donor.Email,
                    Subject = "BMS Confirmation"
                };

                await _emailService.SendAsync(message);
            }
            
        }

        #endregion
    }
}