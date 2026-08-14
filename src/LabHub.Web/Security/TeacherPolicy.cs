using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using LabHub.Web.Core;

namespace LabHub.Web.Security;

/// <summary>Правило «преподаватель»: аутентифицирован и email в allowlist.</summary>
public static class TeacherPolicy
{
    public const string Name = "Teacher";

    public static bool IsTeacher(ClaimsPrincipal user, IReadOnlyCollection<string> teachers)
    {
        if (user.Identity?.IsAuthenticated != true) return false;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        return email is not null && teachers.Contains(email, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class TeacherRequirement : IAuthorizationRequirement;

public sealed class TeacherAuthorizationHandler : AuthorizationHandler<TeacherRequirement>
{
    private readonly IOptionsMonitor<LabHubOptions> _options;

    public TeacherAuthorizationHandler(IOptionsMonitor<LabHubOptions> options) => _options = options;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        TeacherRequirement requirement)
    {
        if (TeacherPolicy.IsTeacher(context.User, _options.CurrentValue.Teachers))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
