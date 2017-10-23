using BloodManagmentSystem.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace BloodManagmentSystem.Persistance.EntityConfigurations
{
    public class DonorConfiguration : EntityTypeConfiguration<Donor>
    {
        public DonorConfiguration()
        {
            Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(50);

            Property(d => d.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName, 
                    new IndexAnnotation(
                        new IndexAttribute("Email", 1) {IsUnique = true}));

            Property(d => d.City)
                .IsRequired()
                .HasMaxLength(50);

            Property(d => d.BloodType)
                .IsRequired();

            Property(d => d.Confirmed)
                .IsRequired();
        }
    }
}