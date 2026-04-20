using Asp.Versioning;
using Cart.Api.Contracts.Carts;
using Cart.Application.Carts.AddCartItem;
using Cart.Application.Carts.ClearCart;
using Cart.Application.Carts.CreateCart;
using Cart.Application.Carts.GetCart;
using Cart.Application.Carts.RemoveCartItem;
using Cart.Application.Carts.Shared;
using Cart.Application.Carts.UpdateCartItemQuantity;
using Cart.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
public sealed class CartController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartResponse>> CreateCartAsync(CancellationToken cancellationToken)
    {
        Result<CartDto> result = await mediator.Send(new CreateCartCommand(), cancellationToken);

        return result.Match(
            success => Ok(success.ToResponse()),
            failure => this.ToProblemDetails(failure));
    }

    [HttpGet]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> GetCartAsync(CancellationToken cancellationToken)
    {
        Result<CartDto> result = await mediator.Send(new GetCartQuery(), cancellationToken);

        return result.Match(
            success => Ok(success.ToResponse()),
            failure => this.ToProblemDetails(failure));
    }

    [HttpPost("items")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartResponse>> AddItemAsync(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        AddCartItemCommand command = new(request.Sku, request.Name, request.Quantity, request.UnitPrice, request.Currency);
        Result<CartDto> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Ok(success.ToResponse()),
            failure => this.ToProblemDetails(failure));
    }

    [HttpPut("items/{itemId:guid}")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> UpdateItemQuantityAsync(
        Guid itemId,
        [FromBody] UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        UpdateCartItemQuantityCommand command = new(itemId, request.Quantity);
        Result<CartDto> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Ok(success.ToResponse()),
            failure => this.ToProblemDetails(failure));
    }

    [HttpDelete("items/{itemId:guid}")]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> RemoveItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        Result<CartDto> result = await mediator.Send(new RemoveCartItemCommand(itemId), cancellationToken);

        return result.Match(
            success => Ok(success.ToResponse()),
            failure => this.ToProblemDetails(failure));
    }

    [HttpDelete]
    [ProducesResponseType<CartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> ClearCartAsync(CancellationToken cancellationToken)
    {
        Result<CartDto> result = await mediator.Send(new ClearCartCommand(), cancellationToken);

        return result.Match(
            success => Ok(success.ToResponse()),
            failure => this.ToProblemDetails(failure));
    }
}
