using CoParenting.Application.Services;
using FluentAssertions;

namespace CoParenting.Tests.Unit;

public sealed class TokenServiceTests
{
    [Fact]
    public void Link_token_contains_256_bits_and_only_hash_is_stable()
    {
        var token = TokenService.GenerateToken();
        var padded = token.Replace('-', '+').Replace('_', '/') + new string('=', (4 - token.Length % 4) % 4);
        Convert.FromBase64String(padded).Should().HaveCount(32);
        TokenService.HashToken(token).Should().HaveLength(64).And.Be(TokenService.HashToken(token));
    }

    [Fact]
    public void Invitation_code_is_human_friendly_and_normalized()
    {
        var code = TokenService.GenerateInvitationCode();
        code.Should().MatchRegex("^[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{4}-[23456789ABCDEFGHJKLMNPQRSTUVWXYZ]{4}$");
        TokenService.NormalizeInvitationCode(code.ToLowerInvariant()).Should().Be(code.Replace("-", ""));
    }
}
