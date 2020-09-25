using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HermesDMobAPI.Models.DTO
{
    public class HttpErrorMessageDto
    {
        public ErrorCode Code { get; set; }
        public String Description { get; set; }

        public HttpErrorMessageDto()
        {

        } 
        public HttpErrorMessageDto(string code, string description)
        {
            Code = (ErrorCode)Enum.Parse(typeof(ErrorCode), code);
            Description = description;
        }
        public HttpErrorMessageDto(ErrorCode code)
        {
            Code = code;
            Description = code.ToStringX();
        }
    }
}