using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Trainacc.Filters
{
    public class RoleBasedAuthFilter : IAsyncAuthorizationFilter
    {
        //private readonly string _requiredRole;

        //public RoleBasedAuthFilter(string requiredRole) => _requiredRole = requiredRole;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await Task.CompletedTask;
            //var user = context.HttpContext.User;
            //if (!user.IsInRole(_requiredRole))
            //{
            //    context.Result = new ForbidResult();
            //}
        }
    }
}