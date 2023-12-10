using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CourseManagement.Models
{
    public partial class Material
    {
        [Display(Name = "Material")]
        public IEnumerable<HttpPostedFileBase> MaterialsObj { get; set; }
    }
}