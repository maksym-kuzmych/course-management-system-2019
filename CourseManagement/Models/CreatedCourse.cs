//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CourseManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CreatedCourse
    {
        public int UserId { get; set; }
        public int CoursesId { get; set; }
        public string CourseName { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
    
        public virtual Course Course { get; set; }
        public virtual User User { get; set; }
    }
}
