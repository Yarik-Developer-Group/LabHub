using System.Security.Claims;
using LabHub.Web.Security;
using Xunit;

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

    [Theory]
    [InlineData("teacher@example.com")]
    [InlineData("TEACHER@Example.Com")]
    public void AllowlistedEmailIsTeacherIgnoringCase(string email)
    {
        Assert.True(TeacherPolicy.IsTeacher(UserWithEmail(email), Teachers));
    }

    [Fact]
    public void UnknownEmailIsNotTeacher()
    {
        Assert.False(TeacherPolicy.IsTeacher(UserWithEmail("student@example.com"), Teachers));
    }

    [Fact]
    public void MissingEmailClaimIsNotTeacher()
    {
        Assert.False(TeacherPolicy.IsTeacher(UserWithEmail(null), Teachers));
    }

    [Fact]
    public void UnauthenticatedUserIsNotTeacher()
    {
        Assert.False(TeacherPolicy.IsTeacher(UserWithEmail("teacher@example.com", authenticated: false), Teachers));
    }

    [Fact]
    public void EmptyAllowlistRejectsEveryone()
    {
        Assert.False(TeacherPolicy.IsTeacher(UserWithEmail("teacher@example.com"), []));
    }
}
