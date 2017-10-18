namespace BloodManagmentSystem.Core.Models
{
    public class Donor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public BloodType BloodType { get; set; }
        public bool Confirmed { get; set; }
    }
}