using System.ComponentModel;

namespace HermesDMobAPI.Models.DTO
{
    /// <summary>
    /// 
    /// </summary>
    public enum AppMessage
    {

        #region App

        /// <summary>
        /// Ok
        /// </summary>
        [Description("Ok")]
        Ok = 200,


        /// <summary>
        /// 
        /// </summary>
        [Description("Invalid model")]
        InvalidModel = 1000,

        /// <summary>
        /// 
        /// </summary>
        [Description("Message service error")]
        SmsServiceError = 1001,

        #endregion


        #region Auth

        /// <summary>
        /// Invalid login or password.
        /// </summary>
        [Description("Invalid username")]
        InvalidUsername = 1501,


        /// <summary>
        /// Invalid login or password.
        /// </summary>
        [Description("Invalid password")]
        InvalidPassword = 1502,

        /// <summary>
        /// Invalid login or password.
        /// </summary>
        [Description("Invalid username or password")]
        InvalidLoginOrPassword = 1503, 
 

        #endregion

    }
}