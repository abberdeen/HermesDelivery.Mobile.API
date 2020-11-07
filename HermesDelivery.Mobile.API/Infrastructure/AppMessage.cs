using System.Net;

namespace CourierAPI.Infrastructure
{
    public class AppMessage
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }

        public AppMessage(int code, string description, HttpStatusCode httpStatusCode)
        {
            Code = code;
            Description = description;
            HttpStatusCode = httpStatusCode;
        }

        #region App - 1000

        /// <summary>
        ///
        /// </summary>
        public static AppMessage Ok = new AppMessage(200, "Ok", HttpStatusCode.OK);

        /// <summary>
        /// Do not use unnecessarily.
        /// </summary>
        public static AppMessage BadRequest = new AppMessage(400, "BadRequest", HttpStatusCode.BadRequest);

        /// <summary>
        /// Do not use unnecessarily.
        /// </summary>
        public static AppMessage NotFound = new AppMessage(404, "NotFound", HttpStatusCode.NotFound);

        /// <summary>
        ///
        /// </summary>
        public static AppMessage InvalidModel = new AppMessage(1000, "Invalid model", HttpStatusCode.BadRequest);

        /// <summary>
        /// HttpStatusCode.InternalServerError
        /// </summary>
        public static AppMessage SmsServiceError = new AppMessage(1001, "Message service error", HttpStatusCode.InternalServerError);

        /// <summary>
        /// Do not use unnecessarily.
        /// <br/>
        /// HttpStatusCode.NotFound
        /// </summary>
        public static AppMessage RecordNotExists = new AppMessage(1002, "Record not exists", HttpStatusCode.NotFound);

        #endregion App

        #region AuthService - 1500

        /// <summary>
        /// Invalid username.
        /// <br/>
        /// HttpStatusCode.BadRequest
        /// </summary>
        public static AppMessage InvalidUsername = new AppMessage(1501, "Invalid username", HttpStatusCode.BadRequest);

        /// <summary>
        /// Invalid password.
        /// <br/>
        /// HttpStatusCode.BadRequest
        /// </summary>
        public static AppMessage InvalidPassword = new AppMessage(1502, "Invalid password", HttpStatusCode.BadRequest);

        /// <summary>
        /// Invalid username or password.
        /// <br/>
        /// <b>HttpStatusCode</b>.BadRequest
        /// </summary>
        public static AppMessage InvalidLoginOrPassword = new AppMessage(1503, "Invalid username or password", HttpStatusCode.BadRequest);

        #endregion "AuthService"

        #region WorkShift - 1600

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage NowNotShiftTime = new AppMessage(1601, "Now is not the time of the current shift", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage WorkShiftNotAssigned = new AppMessage(1602, "There is not a single active assignment to the shift courier.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage WorkShiftNotStarted = new AppMessage(1603, "No running shift to end or pause.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage WorkShiftEnded = new AppMessage(1603, "Work shift is ended.", HttpStatusCode.BadRequest);



        #endregion
    }
}