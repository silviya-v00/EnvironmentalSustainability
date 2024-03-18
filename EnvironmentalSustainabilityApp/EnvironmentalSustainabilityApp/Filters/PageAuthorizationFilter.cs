using EnvironmentalSustainabilityApp.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Filters
{
    public class PageAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly UserManager<IdentityUser> _userManager;

        public PageAuthorizationFilter(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor != null && actionDescriptor.ControllerTypeInfo.BaseType == typeof(Controller))
            {
                var actionName = actionDescriptor.ActionName;
                if (CommonUtil.IsLoginAction(actionName))
                {
                    return;
                }
            }

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectResult("/Home/LoginPage");
                return;
            }

            var currentUser = await _userManager.GetUserAsync(context.HttpContext.User);
            var userRole = (await _userManager.IsInRoleAsync(currentUser, CommonUtil.AdminRole) ? CommonUtil.AdminRole : CommonUtil.RegularUserRole);

            if (actionDescriptor != null && actionDescriptor.ControllerTypeInfo.BaseType == typeof(Controller))
            {
                var actionName = actionDescriptor.ActionName;
                if (!CommonUtil.IsUserAuthorizedForPage(actionName, userRole) || CommonUtil.IsLoginAction(actionName))
                {
                    if (userRole == CommonUtil.AdminRole)
                        context.Result = new RedirectResult("/Home/AdminHome");
                    else
                        context.Result = new RedirectResult("/Home/Index");

                    return;
                }
            }
        }
    }
}
