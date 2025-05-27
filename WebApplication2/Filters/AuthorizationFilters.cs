using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Trainacc.Filters
{
    public class RoleBasedAuthFilter : IAsyncAuthorizationFilter
    {
        private readonly string _requiredRole;

        public RoleBasedAuthFilter(string requiredRole) => _requiredRole = requiredRole;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!string.IsNullOrEmpty(_requiredRole) && !user.IsInRole(_requiredRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            await Task.CompletedTask;
        }
    }
}