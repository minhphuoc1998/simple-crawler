using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Services;

public class CustomAuthorizeFilter : Attribute, IAuthorizationFilter
{
    private readonly AuthService _authService;
    private readonly UserService _userService;

    public CustomAuthorizeFilter(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        if (token == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var principal = _authService.VerifyToken(token);
        if (principal == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var username = principal.Identity?.Name;
        if (username == null || _userService.FindOne(username) == null)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
