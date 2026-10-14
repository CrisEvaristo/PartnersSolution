using Partners.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Partners.Domain.Entities
{
    public class Partner
    {
        private Partner()
        {
        }

        public Partner(
            string name,
            string taxNumber,
            PartnerType type,
            string? email,
            decimal creditLimit)
        {
            Id = Guid.NewGuid();
            Name = name;
            TaxNumber = taxNumber;
            Type = type;
            Email = email;
            CreditLimit = creditLimit;
            IsActive = true;

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string TaxNumber { get; private set; } = string.Empty;

        public PartnerType Type { get; private set; }

        public string? Email { get; private set; }

        public decimal CreditLimit { get; private set; }

        public bool IsActive { get; private set; }

        public byte[] Version { get; private set; } = [];

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public void Update(
            string name,
            string taxNumber,
            PartnerType type,
            string? email,
            decimal creditLimit,
            bool isActive)
        {
            Name = name;
            TaxNumber = taxNumber;
            Type = type;
            Email = email;
            CreditLimit = creditLimit;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
