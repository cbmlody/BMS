using BloodManagmentSystem.Core.Models;
using BloodManagmentSystem.Core.Repositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace BloodManagmentSystem.Persistance.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly ApplicationDbContext _context;

        public DonorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Donor> GetDonorsByBloodType(BloodType type)
        {
            return _context.Donors
                .Where(d => d.BloodType == type)
                .ToList();
        }

        public void Add(Donor donor)
        {
            _context.Donors.Add(donor);
        }

        public Donor GetByHashCode(int hashCode)
        {
            return _context.Donors.SingleOrDefault(d => d.GetHashCode() == hashCode);
        }

        public void Update(Donor donor)
        {
            _context.Entry(donor).State = EntityState.Modified;
        }
    }
}