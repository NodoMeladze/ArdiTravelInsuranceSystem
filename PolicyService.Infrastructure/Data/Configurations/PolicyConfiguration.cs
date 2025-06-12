using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyService.Domain.Entities;

namespace PolicyService.Infrastructure.Data.Configurations
{
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.ToTable("Policies");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .IsRequired()
                .ValueGeneratedNever();

            builder.Property(p => p.CoverageType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(p => p.PremiumAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.PaymentId)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired();

            builder.OwnsOne(p => p.Customer, customer =>
            {
                customer.Property(c => c.Name)
                    .HasColumnName("CustomerName")
                    .HasMaxLength(100)
                    .IsRequired();

                customer.Property(c => c.Email)
                    .HasColumnName("CustomerEmail")
                    .HasMaxLength(255)
                    .IsRequired();

                customer.Property(c => c.PhoneNumber)
                    .HasColumnName("CustomerPhoneNumber")
                    .HasMaxLength(20);

                customer.HasIndex(c => c.Email)
                    .HasDatabaseName("IX_Policies_CustomerEmail");
            });

            builder.OwnsOne(p => p.TripDetails, trip =>
            {
                trip.Property(t => t.Destination)
                    .HasColumnName("Destination")
                    .HasMaxLength(100)
                    .IsRequired();

                trip.Property(t => t.StartDate)
                    .HasColumnName("TripStartDate")
                    .IsRequired();

                trip.Property(t => t.EndDate)
                    .HasColumnName("TripEndDate")
                    .IsRequired();

                trip.Ignore(t => t.DurationInDays);

                trip.HasIndex(t => new { t.StartDate, t.EndDate })
                    .HasDatabaseName("IX_Policies_TripDates");
            });

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Policies_Status");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Policies_CreatedAt");

            builder.HasIndex(p => p.PaymentId)
                .HasDatabaseName("IX_Policies_PaymentId");
        }
    }
}