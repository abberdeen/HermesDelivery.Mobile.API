using HermesDMobAPI.Infrastructure.Extensions;
using System;

namespace HermesDMobAPI.Models.DTO
{
    public class AppMessageDto
    {
        /// <summary>
        /// 
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }

        public AppMessageDto(AppMessage message)
        {
            Code = (int)message;
            Text = message.GetDescription();
        }
         
        public AppMessageDto(int code)
        {
            AppMessage message = (AppMessage) code;
            Code = code;
            Text = message.GetDescription();
        }
    }
}