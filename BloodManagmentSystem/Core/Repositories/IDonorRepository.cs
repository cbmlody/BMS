using System.Collections.Generic;
using BloodManagmentSystem.Core.Models;

namespace BloodManagmentSystem.Core.Repositories
{
    public interface IDonorRepository
    {
        Donor Get(int id);
        IEnumerable<Donor> GetDonorsByBloodType(BloodType type);
        void Add(Donor donor);
        void Update(Donor donor);
    }
}