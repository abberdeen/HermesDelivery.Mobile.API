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
    
    public partial class CompanyInSocialNetwork
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int SocialNetworkId { get; set; }
        public string Link { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Company Company { get; set; }
        public virtual SocialNetwork SocialNetwork { get; set; }
    }
}