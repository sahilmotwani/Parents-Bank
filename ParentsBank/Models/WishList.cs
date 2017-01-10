using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParentsBank.Models
{
    public class WishList
    {
        public WishList()
        {
            DateAdded = DateTime.Now;
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true),Editable(false)]
        public DateTime DateAdded { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public float Cost { get; set; }
        [Required]
        public string Description { get; set; }
        [Url]
        public string Link { get; set;  }
        public bool Purchased { get; set; }
        public string Owner { get; set; }
        [Range(0,2)]
        public int Purchasable { get; set; }

        public virtual int AccountId { get; set; }
        public virtual Account account { get; set; }

        public float AmountNeededLeft()
        {
            float TotalAmount = account.TotalAmount;

            if (Purchasable == 0)
            {
                return Cost - TotalAmount;
            }
            if (Purchasable == 1)
            {
                return TotalAmount - Cost;
            }
            else
                return 0;
        }

    }
}