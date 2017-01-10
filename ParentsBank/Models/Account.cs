using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ParentsBank.Models
{
    public class Account
    {

        public Account()
        {
            OpenDate = DateTime.Now;
        }
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        [EmailAddress]
        public string Recipient { get; set; }
        
        [EmailAddress]
        public string Owner { get; set; }
        //[CustomValidation(typeof(Account), "EmailCheck")]
        [Required, StringLength(50)]
        public string Name { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Open Date")]
        public DateTime OpenDate { get; set; }
        [Range(0,100), DisplayFormat(DataFormatString ="{0:0.00}",ApplyFormatInEditMode =true)]
        [Display(Name = "Rate of Interest")]
        public float InterestRate { get; set; }
        [Required]
        public string Description { get; set; }
        public string Username { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Current Principle Balance")]
        public float TotalAmount { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Interest Earned")]
        public float InterestEarned { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Last Deposit")]
        public DateTime LastDeposit { get; set; }
        public virtual List<Transaction> transactions { get; set; }
        public virtual List<WishList> wishlist { get; set; }
        //public virtual Manager manager { get; set; }
        
       
       
        public float AccruedAmount()
        {
            float t = TotalAmount;
            t = t + InterestEarned;
            return t;
        }

        public float PrincipalPercentage()
        {
            float FA = TotalAmount + InterestEarned;
            float Pper = (TotalAmount / FA) * 100;
            return Pper;
        }

        public float InterestPercentage()
        {
            float FA = TotalAmount + InterestEarned;
            float Pper = (InterestEarned / FA) * 100;
            return Pper;
        }

    }
}