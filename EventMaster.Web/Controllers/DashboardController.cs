using EventMaster.Web.Models;
using EventMaster.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventMaster.Web.Controllers;

public class DashboardController : Controller
{
    private readonly DashboardService _dash;

    public DashboardController(DashboardService dash)
    {
        _dash = dash;
    }

    // GET: /Dashboard/User
    [HttpGet]
    public async Task<IActionResult> User()
    {
        var vm = new UserDashboardVm();

        try
        {
            vm.Me = await _dash.GetMeAsync();
            var all = await _dash.GetDashboardBookingsAsync();

            // Upcoming vs Past:
            // Past = Completed OR start time in the past
            var nowUtc = DateTime.UtcNow;

            vm.Upcoming = all
                .Where(x => x.Status == "Scheduled" && x.StartDateTimeUtc >= nowUtc)
                .OrderBy(x => x.StartDateTimeUtc)
                .ToList();

            vm.Past = all
                .Where(x => x.Status != "Scheduled" || x.StartDateTimeUtc < nowUtc)
                .OrderByDescending(x => x.StartDateTimeUtc)
                .ToList();
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        return View(vm);
    }

    // GET: /Dashboard/BookingDetails/5
    [HttpGet]
    public async Task<IActionResult> BookingDetails(int id)
    {
        var vm = new BookingDetailsVm();

        try
        {
            vm.Booking = await _dash.GetBookingAsync(id);
            vm.Payment = await _dash.GetPaymentByBookingAsync(id);
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        return View(vm);
    }


    // POST: /Dashboard/SaveSettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(string mode, string? email, string? phone, string? currentPwd, string? newPwd, string? confirmPwd)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(currentPwd))
                throw new InvalidOperationException("Current password is required.");

            if (mode == "email")
            {
                await _dash.UpdateEmailAsync((email ?? "").Trim(), currentPwd);
                TempData["Toast"] = "Email updated successfully.";
            }
            else if (mode == "phone")
            {
                await _dash.UpdatePhoneAsync((phone ?? "").Trim(), currentPwd);
                TempData["Toast"] = "Phone updated successfully.";
            }
            else if (mode == "password")
            {
                if ((newPwd ?? "") != (confirmPwd ?? ""))
                    throw new InvalidOperationException("New password and confirm password do not match.");

                await _dash.ChangePasswordAsync(currentPwd, newPwd ?? "");
                TempData["Toast"] = "Password changed successfully.";
            }
            else
            {
                throw new InvalidOperationException("Select one field to edit before saving.");
            }
        }
        catch (Exception ex)
        {
            TempData["ToastError"] = ex.Message;
        }

        return RedirectToAction(nameof(User));
    }

    // POST: /Dashboard/CancelRefund/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRefund(int id)
    {
        try
        {
            await _dash.CancelAndRefundAsync(id);
            TempData["Toast"] = "Booking cancelled and refund processed.";
        }
        catch (Exception ex)
        {
            TempData["ToastError"] = ex.Message;
        }

        return RedirectToAction(nameof(BookingDetails), new { id });
    }
}