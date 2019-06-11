using System.Web;
using System.Web.Http;
using WebApiADO.NetCrudPagination.Seeds;

namespace WebApiADO.NetCrudPagination
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DbSeeder.Seed();
        }
    }
}