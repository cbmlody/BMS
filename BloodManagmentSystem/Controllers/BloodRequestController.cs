using BloodManagmentSystem.Core;
using BloodManagmentSystem.Core.Models;
using BloodManagmentSystem.Core.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BloodManagmentSystem.Services;
using Microsoft.AspNet.Identity;

namespace BloodManagmentSystem.Controllers
{
    public class BloodRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private EmailService _emailService;

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

            return View(requestDetails);
        }

        public ActionResult Notify(int id)
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

        [HttpPost]
        public ActionResult Confirm(string hash)
        {
            var confirmation = _unitOfWork.Confirmations.GetByHash(hash);
            if (confirmation == null)
                return HttpNotFound();

            confirmation.Status = true;

            _unitOfWork.Confirmations.Update(confirmation);
            _unitOfWork.Complete();

            return RedirectToAction("Index");
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
                    Body = TemplateService.RenderTemplate("Confirmation.cshtml", confirmation),
                    Destination = confirmation.Donor.Email,
                    Subject = "BMS Confirmation"
                };

                await _emailService.SendAsync(message);
            }
            
        }

        #endregion
    }
}