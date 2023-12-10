using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CourseManagement.Models
{
    [MetadataType(typeof(CourseMetadata))]
    public partial class Course
    {
        [Display(Name = "Image")]
        public HttpPostedFileBase ImageObj { get; set; }
    }

    public class CourseMetadata
    {
        [Display(Name = "Name:")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name required")]
        public string Name { get; set; }
    }
}