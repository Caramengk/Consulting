using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Consulting.Models
{
    [ModelMetadataType(typeof(WorkSessionMetaData))]

    public partial class WorkSession : IValidatableObject
    {
        ConsultingContext _context = new ConsultingContext();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var contract = _context.Contract.Where(a=>a.ContractId == ContractId).FirstOrDefault();
            if(contract != null)
            {
                if (contract.Closed)
                    yield return new ValidationResult("Contract is closed", new[] { "ContractId" });
            }
            else
                yield return new ValidationResult("Contract is closed", new[] { "ContractId" });
            var consultant = _context.Consultant.Where(a=>a.ConsultantId == ConsultantId).FirstOrDefault();
            if(consultant == null)
                yield return new ValidationResult("Consutant ID is not correct", new[] { "ContractId" });

            if(DateWorked != null)
            {
                if (DateWorked > DateTime.Now)
                    yield return new ValidationResult("DateWorked cannot be in future", new[] { "DateWorked" });

                if (DateWorked < contract.StartDate)
                    yield return new ValidationResult("DateWorked cannot be before contract start date", new[] { "DateWorked" });

            }

            if(HoursWorked <=0)
                yield return new ValidationResult("HoursWorked cannot be less than zero", new[] { "HoursWorked" });

            var totalhoursinDatabase = _context.WorkSession
                .Where(a => a.ConsultantId == ConsultantId && a.DateWorked == DateWorked).Sum(a => a.HoursWorked);

            if(totalhoursinDatabase + HoursWorked >24)
                yield return new ValidationResult("HoursWorked cannot be greater than 24", new[] { "HoursWorked" });

            if(WorkSessionId == 0)
            {
                HourlyRate = consultant.HourlyRate;
            }
            else
            {
                if(HourlyRate <=0)
                {
                    yield return new ValidationResult("Hourly Rate cannot be less or equal to 0", new[] { "HourlyRate" });

                }
                if(HourlyRate *1.5 > Consultant.HourlyRate)
                {
                    yield return new ValidationResult("Hourly Rate cannot be greater than 1.5 times than the consultant hourly rate", new[] { "HourlyRate" });

                }

                TotalChargeBeforeTax = HourlyRate * HoursWorked;

                var customer = _context.Customer.Where(a=>a.CustomerId == contract.CustomerId).FirstOrDefault();
                var province = _context.Province.Where(a=>a.ProvinceCode == customer.ProvinceCode).FirstOrDefault();

                ProvincialTax = TotalChargeBeforeTax * province.ProvincialTax;

            }
        }
        public class WorkSessionMetaData
        {
            public int WorkSessionId { get; set; }
            public int ContractId { get; set; }
            public DateTime DateWorked { get; set; }
            public int ConsultantId { get; set; }
            public double HoursWorked { get; set; }
            public string WorkDescription { get; set; }
            public double HourlyRate { get; set; }
            public double ProvincialTax { get; set; }
            public double TotalChargeBeforeTax { get; set; }
        }
    }
   
}
