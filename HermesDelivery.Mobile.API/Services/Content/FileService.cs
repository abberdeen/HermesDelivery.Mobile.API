using System;
using System.Configuration;

namespace CourierAPI.Services.Content
{
    public static class FileService
    {

        public static string GetImageUrl(string fileName)
        {
            var baseUrl = ConfigurationManager.AppSettings["FileUrl"];
            return new Uri(new Uri(baseUrl), fileName).ToString();
        }
    }
}