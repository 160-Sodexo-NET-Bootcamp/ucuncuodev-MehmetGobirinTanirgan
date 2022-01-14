using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace SwcsAPI.Middlewares
{
    public class GetVehicleByIdBlockerMiddleware
    {
        private readonly RequestDelegate next;

        public GetVehicleByIdBlockerMiddleware(RequestDelegate next) // Bir sonra gelecek olan middlware'i uyandırmak için burda parametre
                                                                     // olarak RequestDelegate alıyoruz.
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.RouteValues.ContainsKey("action")) // Route içerisinde bu key değerinin olup olmadığı kontrolü.
            {
                // Eğer o key var ise ve o key'e karşılık gelen değer, engellemek istediğim action name'i ile aynı ise kısa devre yaptırarak,
                // forbidden response dönüyorum.
                if (context.Request.RouteValues["action"].ToString() == "GetVehicleById") 
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync("This endpoint is not available for now.");
                    return;
                }
            }
           
            await next(context);
        }
    }
}
