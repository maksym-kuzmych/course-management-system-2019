using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CourseManagement.Models
{
    public class ModelForAdmin
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string EmailId { get; set; }
        public bool isLecturer { get; set; }
        public bool isStudent { get; set; }
    } 
}