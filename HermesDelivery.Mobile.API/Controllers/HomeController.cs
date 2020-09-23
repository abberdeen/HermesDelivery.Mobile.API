using System.Web.Mvc;

namespace HermesDelivery.Mobile.API.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/swagger");
        }
    }
}