using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VTInformatica.Models.Auth;
using VTInformatica.Tests.Infrastructure;

namespace VTInformatica.Tests.Integrity;

/// <summary>
/// M3c2: carrello e ordini usano l'email come chiave, quindi non può esistere più di un
/// utente con la stessa email (RequireUniqueEmail), nemmeno con UserName diverso.
/// </summary>
[Collection(ApiCollection.Name)]
public class UniqueEmailTests
{
    private readonly VtWebApplicationFactory _factory;

    public UniqueEmailTests(VtWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task CreatingASecondUserWithTheSameEmail_IsRejected_EvenWithADifferentUserName()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = TestHelpers.UniqueEmail();

        var first = await userManager.CreateAsync(NewUser($"first-{Guid.NewGuid():N}", email), TestHelpers.ValidPassword);
        var second = await userManager.CreateAsync(NewUser($"second-{Guid.NewGuid():N}", email.ToUpperInvariant()), TestHelpers.ValidPassword);

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Contains(second.Errors, e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail));
    }

    private static ApplicationUser NewUser(string userName, string email) => new()
    {
        UserName = userName,
        Email = email,
        FirstName = "Mario",
        LastName = "Rossi",
        BirthDate = new DateOnly(1990, 1, 1),
        IsActive = true
    };
}
