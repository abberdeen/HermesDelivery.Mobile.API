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
    
    public partial class Courier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Courier()
        {
            this.CourierFiles = new HashSet<CourierFile>();
            this.CourierOAuthDatas = new HashSet<CourierOAuthData>();
            this.CourierPenalties = new HashSet<CourierPenalty>();
            this.CourierShifts = new HashSet<CourierShift>();
            this.Orders = new HashSet<Order>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public string Photo { get; set; }
        public int GenderId { get; set; }
        public string Address { get; set; }
        public string PassportNo { get; set; }
        public string LicenceNo { get; set; }
        public Nullable<System.DateTime> HireDate { get; set; }
        public Nullable<System.DateTime> FireDate { get; set; }
        public Nullable<int> LockedReasonId { get; set; }
        public Nullable<System.DateTime> LockedDate { get; set; }
        public string LockedDescription { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> MovementMethodId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string pass_tmp { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourierFile> CourierFiles { get; set; }
        public virtual CourierLockReason CourierLockReason { get; set; }
        public virtual CourierMovementMethod CourierMovementMethod { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourierOAuthData> CourierOAuthDatas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourierPenalty> CourierPenalties { get; set; }
        public virtual Gender Gender { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CourierShift> CourierShifts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Orders { get; set; }
    }
}