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
    
    public partial class Answer
    {
        public int Answer_Id { get; set; }
        public int Question_Id { get; set; }
        public string Label { get; set; }
        public double Points { get; set; }
        public bool IsCorrect { get; set; }
    
        public virtual Question Question { get; set; }
    }
}