//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CourierAPI.Infrastructure.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Slider
    {
        public int Id { get; set; }
        public string Slider1 { get; set; }
        public string Title { get; set; }
        public int PlatformId { get; set; }
        public int OrderId { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Platform Platform { get; set; }
    }
}