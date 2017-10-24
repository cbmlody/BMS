using BloodManagmentSystem.Core.Models;
using BloodManagmentSystem.Core.Repositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace BloodManagmentSystem.Persistance.Repositories
{
    public class ConfirmationRepository : IConfirmationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConfirmationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddRange(IEnumerable<Confirmation> confirmations)
        {
            _context.Confirmations.AddRange(confirmations);
        }

        public IEnumerable<Confirmation> GetConfirmationsWithDonorsToRequest(int requestId)
        {
            return _context.Confirmations
                .Where(c => c.RequestId == requestId)
                .Include(c => c.Request)
                .Include(c => c.Donor)
                .ToList();
        }

        public Confirmation GetByHash(string hash)
        {
            return _context.Confirmations
                .Where(c => c.HashCode.Equals(hash))
                .Include(c => c.Donor)
                .Include(c => c.Request)
                .SingleOrDefault();
        }

        public void Update(Confirmation confirmation)
        {
            _context.Entry(confirmation).State = EntityState.Modified;
        }
    }
}