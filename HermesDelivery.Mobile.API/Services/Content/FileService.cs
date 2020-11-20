using System;

namespace CourierAPI.Services.Content
{
    public static class FileService
    {
        private static readonly Uri BaseUrl = new Uri("https://admin.kenguru.tj/files/");
         
        public static string GetImageUrl(string fileName)
        {
         return  new Uri(BaseUrl, fileName).ToString();
        } 
    }
}