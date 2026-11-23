using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Security;

/// <summary>
/// Identità utente nel JWT e protezione di PUT /Account/UpdateUser/{id}.
/// </summary>
[Collection(ApiCollection.Name)]
public class AccountSecurityTests
{
    private readonly VtWebApplicationFactory _factory;

    public AccountSecurityTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_Token_ContainsNameIdentifierClaimWithUserId()
    {
        var email = TestHelpers.UniqueEmail();
        var token = await TestHelpers.RegisterAndLoginAsync(_factory, email);
        var userId = await TestHelpers.GetUserIdAsync(_factory, email);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(userId, jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }
}
