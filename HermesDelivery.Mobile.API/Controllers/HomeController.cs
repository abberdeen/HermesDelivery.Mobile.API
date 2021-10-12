using System.Web.Http; 
using System.Web.Mvc;
using CourierAPI.Infrastructure.Extensions;

namespace CourierAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/swagger");
        }
         
    }
}