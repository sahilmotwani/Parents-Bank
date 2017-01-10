using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParentsBank.Models
{
    public class Transaction
    {
        public Transaction()
        {
            TransactionDate = DateTime.Now;
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true), Editable(false)]
        public DateTime TransactionDate { get; set; }
        [CustomValidation(typeof(Transaction), "balanceCheck")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public float Amount { get; set;  }
        [Required]
        public string Note { get; set; }
        //public string transactionType { get; set; }
        public string username { get; set; }
        [Display(Name ="Recipient Account")]
        public virtual int AccountId { get; set; }
        public virtual Account account { get; set;  }



        public static ValidationResult balanceCheck(float Amount, ValidationContext context)
        {
            if (Amount == 0)
            {
                return new ValidationResult("Amount can not be 0");
            }

            else
                return ValidationResult.Success;

        }
    }
}