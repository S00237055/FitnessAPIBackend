using Xunit;

namespace FitnessAPI.Tests;

/// <summary>
/// Unit tests for the Argon2id password hashing helper.
/// These are pure unit tests: no database, no HTTP, no external dependency.
/// </summary>
public class PasswordHelperTests
{
    [Fact]
    public void CreateSalt_ReturnsSixteenBytes()
    {
        var salt = PasswordHelper.CreateSalt();

        Assert.NotNull(salt);
        Assert.Equal(16, salt.Length);
    }

    [Fact]
    public void CreateSalt_ProducesADifferentValueEachCall()
    {
        var first = PasswordHelper.CreateSalt();
        var second = PasswordHelper.CreateSalt();

        // Two random 16-byte salts colliding is effectively impossible.
        Assert.NotEqual(Convert.ToBase64String(first), Convert.ToBase64String(second));
    }

    [Fact]
    public void HashPassword_IsDeterministic_ForSamePasswordAndSalt()
    {
        var salt = PasswordHelper.CreateSalt();

        var first = PasswordHelper.HashPassword("Password123", salt);
        var second = PasswordHelper.HashPassword("Password123", salt);

        Assert.Equal(first, second);
    }

    [Fact]
    public void HashPassword_SamePassword_DifferentSalts_ProducesDifferentHashes()
    {
        var saltA = PasswordHelper.CreateSalt();
        var saltB = PasswordHelper.CreateSalt();

        var hashA = PasswordHelper.HashPassword("Password123", saltA);
        var hashB = PasswordHelper.HashPassword("Password123", saltB);

        // This is the property that defeats rainbow tables and stops two users
        // with the same password sharing a hash.
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

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        var salt = PasswordHelper.CreateSalt();
        var hash = PasswordHelper.HashPassword("Password123", salt);

        Assert.False(PasswordHelper.VerifyPassword("password123", salt, hash));
    }
}
