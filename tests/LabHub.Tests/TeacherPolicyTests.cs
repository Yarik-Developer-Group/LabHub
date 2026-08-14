using System.Security.Claims;
using LabHub.Web.Security;

namespace LabHub.Tests;

public class TeacherPolicyTests
{
    private static readonly string[] Teachers = ["teacher@example.com"];

    private static ClaimsPrincipal UserWithEmail(string? email, bool authenticated = true)
    {
        var claims = email is null ? [] : new[] { new Claim(ClaimTypes.Email, email) };
        var identity = authenticated ? new ClaimsIdentity(claims, "test") : new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }

    [Test]
    public void AllowlistedEmailIsTeacher()
    {
        Check.True(TeacherPolicy.IsTeacher(UserWithEmail("teacher@example.com"), Teachers), "teacher");
    }

    [Test]
    public void EmailComparisonIgnoresCase()
    {
        Check.True(TeacherPolicy.IsTeacher(UserWithEmail("TEACHER@Example.Com"), Teachers), "case-insensitive");
    }

    [Test]
    public void UnknownEmailIsNotTeacher()
    {
        Check.False(TeacherPolicy.IsTeacher(UserWithEmail("student@example.com"), Teachers), "student");
    }

    [Test]
    public void MissingEmailClaimIsNotTeacher()
    {
        Check.False(TeacherPolicy.IsTeacher(UserWithEmail(null), Teachers), "no email claim");
    }

    [Test]
    public void UnauthenticatedUserIsNotTeacher()
    {
        Check.False(TeacherPolicy.IsTeacher(UserWithEmail("teacher@example.com", authenticated: false), Teachers),
            "unauthenticated");
    }

    [Test]
    public void EmptyAllowlistRejectsEveryone()
    {
        Check.False(TeacherPolicy.IsTeacher(UserWithEmail("teacher@example.com"), []), "empty allowlist");
    }
}
