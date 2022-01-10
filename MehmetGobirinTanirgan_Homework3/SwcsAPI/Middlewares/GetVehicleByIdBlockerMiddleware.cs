using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace SwcsAPI.Middlewares
{
    public class GetVehicleByIdBlockerMiddleware
    {
        private readonly RequestDelegate next;

        public GetVehicleByIdBlockerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.RouteValues.Count > 0)
            {
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
