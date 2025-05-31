using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Order.Application.Interfaces;
using Shared.Application.DTOs;

namespace Order.Web.Pages
{
	[Authorize]
	public class EditModel : PageModel
	{
		private readonly IOrderService _orderService;

		public EditModel(IOrderService orderManager)
		{
			_orderService = orderManager;
		}

		[BindProperty]
		public OrderDto Order { get; set; }

		public List<SelectListItem> Clients { get; set; }
		public List<SelectListItem> Products { get; set; }

		public async Task<IActionResult> OnGetAsync(Guid? id)
		{
			if (id.HasValue)
				Order = await _orderService.GetOrderById(id.Value);

			if (Order == null)
				Order = new OrderDto
				{
					OrderDate = DateTime.Now
				};


			LoadDropdownData();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			LoadDropdownData();

			if (!ModelState.IsValid)
			{
				return Page();
			}
			LoadDropdownData();

			var success = Order.Id == null
			? await _orderService.CreateOrder(Order)
			: await _orderService.EditOrder(Order);

			if (!success)
				return Page();

			return RedirectToPage("/Order");
		}

		private void LoadDropdownData()
		{
			Clients = new List<SelectListItem>
			{
				new SelectListItem
				{
					Value = "460fc027-50e6-4e42-9cb1-1946206fcd97",
					Text = "Client 1"
				},
				new SelectListItem
				{
					Value = "d4a5372b-7cdd-4c07-a27e-47ede649170e",
					Text = "Client 2"
				},
				new SelectListItem
				{
					Value="2dbd3498-1100-45d6-bd92-c94d449b1b00",
					Text="Client3"
				}
			};

			Products = _orderService.GetProducts().Result?.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })?.ToList();
		}
	}
}
