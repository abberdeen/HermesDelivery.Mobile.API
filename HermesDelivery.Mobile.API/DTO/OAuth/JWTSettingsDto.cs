namespace CourierAPI.DTO.OAuth
{
    public class JwtSettingsDto
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessExpiration { get; set; }
        public int RefreshExpiration { get; set; }
        public int RememberMeExpiration { get; set; }
    }
}