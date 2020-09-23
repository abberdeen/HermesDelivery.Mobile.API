using JWT; 
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Script.Serialization; 
using Microsoft.IdentityModel.JsonWebTokens;

namespace HermesDelivery.Mobile.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
        }

        public class CrossDomainHandler : DelegatingHandler
        {
            const string Origin = "Origin";
            const string AccessControlRequestMethod = "Access-Control-Request-Method";
            const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
            const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
            const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
            const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                bool isCorsRequest = request.Headers.Contains(Origin);
                bool isPreflightRequest = request.Method == HttpMethod.Options;
                if (isCorsRequest)
                {
                    if (isPreflightRequest)
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                            response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());

                            string accessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();
                            if (accessControlRequestMethod != null)
                            {
                                response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);
                            }

                            string requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));
                            if (!string.IsNullOrEmpty(requestedHeaders))
                            {
                                response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);
                            }

                            //response.Headers.Add(AccessControlExposeHeaders, request.Headers.GetValues(Origin).First());
                            return response;
                        }, cancellationToken);
                    }
                    else
                    {
                        return base.SendAsync(request, cancellationToken).ContinueWith(t =>
                        {
                            HttpResponseMessage resp = t.Result;
                            resp.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                            return resp;
                        });
                    }
                }
                else
                {
                    return base.SendAsync(request, cancellationToken);
                }
            }
        }


        public abstract class MessageHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);

                byte[] responseMessage;

                if (response != null && response.StatusCode != HttpStatusCode.NoContent)
                {
                    if (response.IsSuccessStatusCode)
                        responseMessage = await response.Content.ReadAsByteArrayAsync();
                    else
                        responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

                    var body = Encoding.UTF8.GetString(responseMessage);

                    try
                    {
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        dynamic dobj = jsonSerializer.Deserialize<dynamic>(body);
                        var result = dobj["errors"];
                        try
                        {
                            var f = result[0];
                            if (f["code"].ToString() == "1000")
                                response.StatusCode = HttpStatusCode.BadRequest;
                            else
                                response.StatusCode = HttpStatusCode.BadRequest;
                        }
                        catch (Exception ex) { }
                    }
                    catch (Exception ex) { }
                }
                //await OutgoingMessageAsync(corrId, requestInfo, responseMessage);

                return response;
            }

            protected abstract Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message);
        }

        public class MessageLoggingHandler : MessageHandler
        {
            protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
            {
                await Task.Run(() =>
                    Debug.WriteLine(string.Format("{0} - Response: {1}\r\n{2}", correlationId, requestInfo, Encoding.UTF8.GetString(message))));
            }
        }

        public class PreflightRequestsHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                        CancellationToken cancellationToken)
            {
                HttpResponseMessage errorResponse = null;

                try
                {
                    IEnumerable<string> authHeaderValues;
                    request.Headers.TryGetValues("Authorization", out authHeaderValues);

                    if (authHeaderValues == null)
                    {
                        return base.SendAsync(request, cancellationToken); // cross fingers
                    }
                    var bearerToken = authHeaderValues.ElementAt(0);
                    var token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;

                    //var memCacher = new MemoryCacher();
                    //if (memCacher.GetValue(token) == null)
                    //{
                    //    errorResponse = request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized");
                    //}

                    var secret = ConfigurationManager.AppSettings.Get("secret");

                    Thread.CurrentPrincipal = ValidateToken(
                        token,
                        secret,
                        true
                        );

                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = Thread.CurrentPrincipal;
                    }
                }
                catch (SignatureVerificationException ex)
                {
                    errorResponse = request.CreateErrorResponse(HttpStatusCode.Forbidden, ex.Message);
                }
                catch (Exception ex)
                {
                    errorResponse = request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }

                return errorResponse != null
                    ? Task.FromResult(errorResponse)
                    : base.SendAsync(request, cancellationToken);
            }

            private static ClaimsPrincipal ValidateToken(string token, string secret, bool checkExpiration)
            {
                var jsonSerializer = new JavaScriptSerializer();
                var payloadJson = JWT.JsonWebToken.Decode(token, secret);
                var payloadData = jsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);

                object exp;
                if (payloadData != null && (checkExpiration && payloadData.TryGetValue("exp", out exp)))
                {
                    var validTo = FromUnixTime(long.Parse(exp.ToString()));
                    if (DateTime.Compare(validTo, DateTime.UtcNow) <= 0)
                    {
                        throw new Exception(
                            string.Format("Token is expired. Expiration: '{0}'. Current: '{1}'", validTo, DateTime.UtcNow));
                    }
                }

                if (payloadData != null && (checkExpiration && payloadData.TryGetValue("exp", out exp)))
                {
                    var validTo = FromUnixTime(long.Parse(exp.ToString()));
                    if (DateTime.Compare(validTo, DateTime.UtcNow) <= 0)
                    {
                        throw new Exception(
                            string.Format("Token is expired. Expiration: '{0}'. Current: '{1}'", validTo, DateTime.UtcNow));
                    }
                }

                var subject = new ClaimsIdentity("Federation", ClaimTypes.Name, ClaimTypes.Role);

                var claims = new List<Claim>();

                claims.Add(new Claim("name", "test name"));
                claims.Add(new Claim("position", "test position"));
                claims.Add(new Claim("photo", "test photo"));

                if (payloadData != null)
                    foreach (var pair in payloadData)
                    {
                        var claimType = pair.Key;

                        var source = pair.Value as ArrayList;

                        if (source != null)
                        {
                            claims.AddRange(from object item in source
                                            select new Claim(claimType, item.ToString(), ClaimValueTypes.String));

                            continue;
                        }

                        switch (pair.Key)
                        {
                            case "name":
                                claims.Add(new Claim(ClaimTypes.Name, pair.Value.ToString(), ClaimValueTypes.String));
                                break;

                            case "surname":
                                claims.Add(new Claim(ClaimTypes.Surname, pair.Value.ToString(), ClaimValueTypes.String));
                                break;

                            case "email":
                                claims.Add(new Claim(ClaimTypes.Email, pair.Value.ToString(), ClaimValueTypes.Email));
                                break;

                            case "role":
                                claims.Add(new Claim(ClaimTypes.Role, pair.Value.ToString(), ClaimValueTypes.String));
                                break;

                            case "userId":
                                claims.Add(new Claim(ClaimTypes.UserData, pair.Value.ToString(), ClaimValueTypes.Integer));
                                break;

                            default:
                                claims.Add(new Claim(claimType, pair.Value.ToString(), ClaimValueTypes.String));
                                break;
                        }
                    }

                subject.AddClaims(claims);
                return new ClaimsPrincipal(subject);
            }

            private static DateTime FromUnixTime(long unixTime)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(unixTime);
            }
        }
    }
}