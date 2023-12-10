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
    
    public partial class StudentProgress
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int QuizId { get; set; }
        public decimal CourseMark { get; set; }
        public decimal QuizMark { get; set; }
        public string QuizName { get; set; }
        public decimal QuizPercent { get; set; }
        public decimal CoursePercent { get; set; }
    
        public virtual Course Course { get; set; }
        public virtual Quiz Quiz { get; set; }
        public virtual User User { get; set; }
    }
}
