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
    
    public partial class CourierPenalty
    {
        public int Id { get; set; }
        public int CourierId { get; set; }
        public decimal Sum { get; set; }
        public int CategoryId { get; set; }
        public int OperationStatusId { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Courier Courier { get; set; }
        public virtual OperationStatus OperationStatus { get; set; }
        public virtual PenaltyCategory PenaltyCategory { get; set; }
    }
}