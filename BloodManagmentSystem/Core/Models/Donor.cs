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

        public override int GetHashCode()
        {
            var result = 37;

            result *= 397;
            result += Email.GetHashCode();

            result *= 397;
            result += Name.GetHashCode();

            return result;
        }
    }
}