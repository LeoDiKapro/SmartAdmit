using AdmissionsPortal.Models;
using AdmissionsPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdmissionsPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // ── GET /Profile/Complete ─────────────────────────────────────────────
        public async Task<IActionResult> Complete()
        {
            var user = await _userManager.GetUserAsync(User);
            var vm = new ProfileViewModel
            {
                FirstName = user!.FirstName,
                LastName = user.LastName,
                Address = user.Address ?? string.Empty,
                City = user.City ?? string.Empty,
                Country = user.Country ?? string.Empty,
                PhoneNumber2 = user.PhoneNumber2 ?? string.Empty,
                DateOfBirth = user.DateOfBirth ?? DateTime.Today,
                NationalId = user.NationalId ?? string.Empty,
                Nationality = user.Nationality ?? string.Empty,
                UndergraduateField = user.UndergraduateField ?? EducationField.Other
            };
            return View(vm);
        }

        // ── POST /Profile/Complete ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(ProfileViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            user!.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Address = vm.Address;
            user.City = vm.City;
            user.Country = vm.Country;
            user.PhoneNumber2 = vm.PhoneNumber2;
            user.DateOfBirth = vm.DateOfBirth;
            user.NationalId = vm.NationalId;
            user.Nationality = vm.Nationality;
            user.UndergraduateField = vm.UndergraduateField;
            user.ProfileCompleted = true;

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Apply", "Application");
        }

        // ── GET /Profile/Edit ─────────────────────────────────────────────────
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var vm = new ProfileViewModel
            {
                FirstName = user!.FirstName,
                LastName = user.LastName,
                Address = user.Address ?? string.Empty,
                City = user.City ?? string.Empty,
                Country = user.Country ?? string.Empty,
                PhoneNumber2 = user.PhoneNumber2 ?? string.Empty,
                DateOfBirth = user.DateOfBirth ?? DateTime.Today,
                NationalId = user.NationalId ?? string.Empty,
                Nationality = user.Nationality ?? string.Empty,
                UndergraduateField = user.UndergraduateField ?? EducationField.Other
            };
            return View(vm);
        }

        // ── POST /Profile/Edit ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);
            user!.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Address = vm.Address;
            user.City = vm.City;
            user.Country = vm.Country;
            user.PhoneNumber2 = vm.PhoneNumber2;
            user.DateOfBirth = vm.DateOfBirth;
            user.NationalId = vm.NationalId;
            user.Nationality = vm.Nationality;
            user.UndergraduateField = vm.UndergraduateField;

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Edit));
        }
    }
}