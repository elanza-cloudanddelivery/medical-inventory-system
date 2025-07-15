using Microsoft.AspNetCore.Mvc;
using MedicalInventory.API.Services.CartService;
using Microsoft.AspNetCore.Authorization;
using MedicalInventory.API.Models.DTOs.Common;
using MedicalInventory.API.Models.DTOs.Request;
using MedicalInventory.API.Models.DTOs.Response;

namespace MedicalInventory.API.Controllers.CartController;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;
    private readonly ICartDispenseService _cartDispenseService;

    public CartController(ICartService cartService, ICartDispenseService cartDispenseService)
    {
        _cartService = cartService;
        _cartDispenseService = cartDispenseService;
    }

    [HttpPost("add")]
    public async Task<ActionResult<CartResponse>> AddToCart([FromBody] AddToCartRequest request)
    {
        var result = await _cartService.AddItemToCartAsync(GetUserId(), request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var cart = await _cartService.GetActiveCartAsync(GetUserId());

        if (cart == null)
            return Ok(new CartDto { IsEmpty = true });

        return Ok(cart);
    }

    [HttpPut("item/{itemId}/quantity")]
    public async Task<ActionResult<CartResponse>> UpdateItemQuantity(int itemId, [FromBody] UpdateItemCartRequest request)
    {
        var result = await _cartService.UpdateCartItemAsync(GetUserId(), itemId, request.NewQuantity);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("clear")]
    public async Task<ActionResult<CartResponse>> ClearCart()
    {
        var result = await _cartService.ClearCartAsync(GetUserId());

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("dispense")]
    public async Task<ActionResult<CartResponse>> DispenseCart([FromBody] DispenseCartRequest request)
    {
        var result = await _cartDispenseService.DispenseCartAsync(GetUserId(), request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("can-dispense")]
    public async Task<ActionResult<bool>> CanDispenseCart()
    {
        var canDispense = await _cartDispenseService.CanUserDispenseCartAsync(GetUserId());

        return Ok(canDispense);
    }

}