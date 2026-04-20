using Cart.Application.Carts.AddCartItem;
using Cart.Application.Carts.RemoveCartItem;
using Cart.Application.Carts.Shared;
using Cart.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cart.IntegrationTests.Carts;

public sealed class CartValidationBehaviorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public CartValidationBehaviorTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Test"));
    }

    [Fact]
    public async Task AddCartItem_ShouldReturnValidationFailure_ForInvalidRequest()
    {
        Result<CartDto> result = await SendAsync(new AddCartItemCommand(string.Empty, string.Empty, 0, -1m, "EU"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation.failed");
        result.Error.Message.ShouldContain("Sku");
        result.Error.Message.ShouldContain("Quantity");
        result.Error.Message.ShouldContain("Currency");
    }

    [Fact]
    public async Task RemoveCartItem_ShouldReturnValidationFailure_ForEmptyItemId()
    {
        Result<CartDto> result = await SendAsync(new RemoveCartItemCommand(Guid.Empty));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation.failed");
    }

    private async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        return await mediator.Send(request, TestContext.Current.CancellationToken);
    }
}
