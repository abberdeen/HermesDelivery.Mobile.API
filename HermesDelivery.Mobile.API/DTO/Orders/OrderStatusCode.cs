namespace HermesDAdmin.Dto.Orders
{
    /// <summary>
    ///
    /// </summary>
    public enum OrderStatusCode
    {
        /// <summary>
        ///
        /// </summary>
        Pending = 1,

        /// <summary>
        ///
        /// </summary>
        Canceled = 2,

        /// <summary>
        ///
        /// </summary>
        OrderAccepted = 3,

        /// <summary>
        ///
        /// </summary>
        CourierSelected = 4,

        /// <summary>
        ///
        /// </summary>
        CourierAccepted = 5,

        /// <summary>
        ///
        /// </summary>
        Prepared = 6,

        /// <summary>
        ///
        /// </summary>
        Delivered = 7,

        /// <summary>
        ///
        /// </summary>
        Transefered = 10,

        /// <summary>
        ///
        /// </summary>
        Completed = 8,

        /// <summary>
        ///
        /// </summary>
        CustomerRejected = 9
    }
}