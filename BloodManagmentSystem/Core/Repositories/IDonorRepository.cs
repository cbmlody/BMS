using System.Collections.Generic;
using BloodManagmentSystem.Core.Models;

namespace BloodManagmentSystem.Core.Repositories
{
    public interface IDonorRepository
    {
        IEnumerable<Donor> GetDonorsByBloodType(BloodType type);
        void Add(Donor donor);
        Donor GetByHashCode(int hashCode);
        void Update(Donor donor);
    }
}