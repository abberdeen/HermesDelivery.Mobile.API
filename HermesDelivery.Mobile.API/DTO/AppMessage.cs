using System.Net;

namespace CourierAPI.DTO
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
        public static AppMessage InvalidModel = new AppMessage(1000, "Неверная модель", HttpStatusCode.BadRequest);

        /// <summary>
        /// HttpStatusCode.InternalServerError
        /// </summary>
        public static AppMessage SmsServiceError = new AppMessage(1001, "Ошибка службы сообщений", HttpStatusCode.InternalServerError);

        /// <summary>
        /// Do not use unnecessarily.
        /// <br/>
        /// HttpStatusCode.NotFound
        /// </summary>
        public static AppMessage RecordNotExists = new AppMessage(1002, "Запись не существует", HttpStatusCode.NotFound);

        #endregion App

        #region AuthService - 1500

        /// <summary>
        /// Invalid username.
        /// <br/>
        /// HttpStatusCode.BadRequest
        /// </summary>
        public static AppMessage InvalidUsername = new AppMessage(1501, "Неверное имя пользователя", HttpStatusCode.BadRequest);

        /// <summary>
        /// Invalid password.
        /// <br/>
        /// HttpStatusCode.BadRequest
        /// </summary>
        public static AppMessage InvalidPassword = new AppMessage(1502, "Неправильный пароль", HttpStatusCode.BadRequest);

        /// <summary>
        /// Invalid username or password.
        /// <br/>
        /// <b>HttpStatusCode</b>.BadRequest
        /// </summary>
        public static AppMessage InvalidLoginOrPassword = new AppMessage(1503, "Неправильное имя пользователя или пароль", HttpStatusCode.BadRequest);

        #endregion "AuthService"

        #region Shift - 1600

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage NowNotShiftTime = new AppMessage(1601, "Сейчас не время текущей смены.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage ShiftNotAssigned = new AppMessage(1602, "Курьеру не назначена активная смена.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage ShiftNotStartedOrPaused = new AppMessage(1603, "Нельзя закрыть смену которая не запущена.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage ShiftEnded = new AppMessage(1604, "Нельзя запускать или ставить на паузу закрытую смену, ок?", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage NotTimeOfStart = new AppMessage(1605, "Пока рано начинать.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage ShiftNotStarted = new AppMessage(1606, "Нельзя ставить на паузу не запущенную смену.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage ClosingUncompletedShift = new AppMessage(1607, "Нельзя закрывать или ставить на паузу когда есть активные заказы", HttpStatusCode.BadRequest);


        #endregion

        #region Supplier - 1700

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage UndefinedSupplier = new AppMessage(1701, "Неизвестный поставщик.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage CantFindRestaurant = new AppMessage(1702, "Не удается найти информацию о ресторане.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage CantFindStore= new AppMessage(1703, "Не удается найти информацию о магазине.", HttpStatusCode.BadRequest);


        #endregion

        #region Order - 1800

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage OrderNotExists = new AppMessage(1801, "Заказ с указанным идентификатором не существует.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage BadAcceptRequest = new AppMessage(1802, "Передаваемый идентификатор заказа не совпадает идентификатором заказа в ожидании.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage OrderStatusIsCompleted = new AppMessage(1803, "Нельзя принять или отклонить завершенный заказ.", HttpStatusCode.BadRequest);

        /// <summary>
        /// 
        /// </summary>
        public static AppMessage UndefinedCourierShiftHistory = new AppMessage(1804, "Сначала запусти смену!", HttpStatusCode.BadRequest);

        #endregion
    }
}