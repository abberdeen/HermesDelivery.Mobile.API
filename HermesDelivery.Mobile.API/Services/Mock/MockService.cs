using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Web;
using CourierAPI.DTO.Orders;
using CourierAPI.DTO.Shift;

namespace CourierAPI.Services.Mock
{
    public static class MockService
    {
        private static readonly string _mockDataParentPath =
            Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data", "MockData");

        #region IncomingOrderResponse

        public static IncomingOrderDetailsDto IncomingOrderResponse_getDetails()
        {
            var json = GetFileContent("IncomingOrderResponse/getDetails.json");
            var obj = JsonConvert.DeserializeObject<IncomingOrderDetailsDto>(json);
            return obj;
        }

        public static IEnumerable<IncomingOrderDto> IncomingOrderResponse_getOrders()
        {
            var json = GetFileContent("IncomingOrderResponse/getOrders.json");
            var obj = JsonConvert.DeserializeObject<IEnumerable<IncomingOrderDto>>(json);
            return obj;
        }

        public static IncomingOrderInfoDto IncomingOrderResponse_getPending()
        {
            var json = GetFileContent("IncomingOrderResponse/getPending.json");
            var obj = JsonConvert.DeserializeObject<IncomingOrderInfoDto>(json);
            return obj;
        }

        #endregion IncomingOrderResponse

        #region WorkShiftResponse

        public static CourierShiftHistoryDto WorkShiftResponse_start()
        {
            var json = GetFileContent("WorkShiftResponse/start.json");
            var obj = JsonConvert.DeserializeObject<CourierShiftHistoryDto>(json);
            return obj;
        }

        public static CourierShiftHistoryDto WorkShiftResponse_end()
        {
            var json = GetFileContent("WorkShiftResponse/end.json");
            var obj = JsonConvert.DeserializeObject<CourierShiftHistoryDto>(json);
            return obj;
        }

        public static CourierShiftHistoryDto WorkShiftResponse_getCurrent()
        {
            var json = GetFileContent("WorkShiftResponse/getCurrent.json");
            var obj = JsonConvert.DeserializeObject<CourierShiftHistoryDto>(json);
            return obj;
        }

        public static IEnumerable<CourierShiftHistoryListItemDto> WorkShiftResponse_getHistory()
        {
            var json = GetFileContent("WorkShiftResponse/getHistory.json");
            var obj = JsonConvert.DeserializeObject<IEnumerable<CourierShiftHistoryListItemDto>>(json);
            return obj;
        }

        #endregion WorkShiftResponse

        #region WorkShiftPauseReasonResponse

        public static IEnumerable<ShiftPauseReasonDto> WorkShiftPauseReasonResponse_getReasons()
        {
            var json = GetFileContent("WorkShiftPauseReasonResponse/getReasons.json");
            var obj = JsonConvert.DeserializeObject<IEnumerable<ShiftPauseReasonDto>>(json);
            return obj;
        }

        #endregion WorkShiftPauseReasonResponse

        #region MyRegion

        private static string GetFileContent(string fileName)
        {
            return File.ReadAllText(Path.Combine(_mockDataParentPath, fileName));
        }

        #endregion MyRegion
    }
}