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
    
    public partial class Restaurant
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Restaurant()
        {
            this.RestaurantInCategories = new HashSet<RestaurantInCategory>();
            this.RestaurantMenus = new HashSet<RestaurantMenu>();
        }
    
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Name { get; set; }
        public string Banner { get; set; }
        public string Logo { get; set; }
        public string CoockTime { get; set; }
        public decimal MinOrderCost { get; set; }
        public string DeliveryCost { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Schedule { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RestaurantInCategory> RestaurantInCategories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RestaurantMenu> RestaurantMenus { get; set; }
    }
}