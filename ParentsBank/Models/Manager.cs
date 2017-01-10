using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParentsBank.Models
{
    public class Manager
    {
        [Key]
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public int Id { get; set; }
        public Double InterestRate { get; set; }

        public virtual Account account { get; set; }
    }
}