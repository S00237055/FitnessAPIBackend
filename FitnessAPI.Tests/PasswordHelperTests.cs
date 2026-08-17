using Xunit;

namespace FitnessAPI.Tests;

public class PasswordHelperTests
{
    [Fact]
    public void CreateSalt_ProducesADifferentValueEachCall()
    {
        var first = PasswordHelper.CreateSalt();
        var second = PasswordHelper.CreateSalt();

        Assert.Equal(16, first.Length);
        Assert.NotEqual(Convert.ToBase64String(first), Convert.ToBase64String(second));
    }

    [Fact]
    public void HashPassword_SamePassword_DifferentSalts_ProducesDifferentHashes()
    {
        var saltA = PasswordHelper.CreateSalt();
        var saltB = PasswordHelper.CreateSalt();

        var hashA = PasswordHelper.HashPassword("Password123", saltA);
        var hashB = PasswordHelper.HashPassword("Password123", saltB);


        Assert.NotEqual(hashA, hashB);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForTheCorrectPassword()
    {
        var salt = PasswordHelper.CreateSalt();
        var hash = PasswordHelper.HashPassword("CorrectHorse1", salt);

        Assert.True(PasswordHelper.VerifyPassword("CorrectHorse1", salt, hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForAnIncorrectPassword()
    {
        var salt = PasswordHelper.CreateSalt();
        var hash = PasswordHelper.HashPassword("CorrectHorse1", salt);

        Assert.False(PasswordHelper.VerifyPassword("WrongHorse1", salt, hash));
    }
}
