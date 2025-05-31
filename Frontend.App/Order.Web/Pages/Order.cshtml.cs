using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Order.Application.Interfaces;
using Shared.Application.DTOs;
using System.Diagnostics.Eventing.Reader;

namespace Order.Web.Pages
{
    [Authorize]
    public class OrderModel : PageModel
    {
        private readonly IStringLocalizer<OrderModel> _localizer;
        private readonly IOrderService _orderManager;

        public OrderModel(IStringLocalizer<OrderModel> localizer
            , IOrderService orderManager)
        {
            _localizer = localizer;
            _orderManager = orderManager;
        }

        [BindProperty]
        public List<OrderDto> Orders { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Orders = await _orderManager.GetOrders() ?? new List<OrderDto>();

            return Page();
        }

        public class OrderRequest
        {
            public Guid OrderId { get; set; }
        }

        public async Task<IActionResult> OnPostDeleteOrderAsync([FromBody] OrderRequest request)
        {
            if (await _orderManager.DeleteOrder(request.OrderId))
                return new OkResult();
            return new BadRequestResult();

        }

        public async Task<IActionResult> OnPostEditAsync(Guid orderId)
        {
            return RedirectToPage("/Orders/Edit", new { id = orderId });
        }
    }
}
