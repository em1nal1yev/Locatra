using LocatraMain.Models;
using LocatraMain.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace LocatraMain.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(ApplicationUserVm appUserVm)
        {
            if (!ModelState.IsValid)
            {
                return View(appUserVm);
            }
            var existingUser = await _userManager.FindByEmailAsync(appUserVm.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Bu email artıq qeydiyyatdan keçib.");
                return View(appUserVm);
            }

            var code = new Random().Next(100000, 999999).ToString();

            
            await _emailSender.SendEmailAsync(appUserVm.Email, "Təsdiqləmə Kodu", $"Kodunuz: <strong>{code}</strong>");

            
            TempData["Code"] = code;
            TempData["User"] = System.Text.Json.JsonSerializer.Serialize(appUserVm);

            return RedirectToAction("ConfirmEmail");
        }

        public IActionResult ConfirmEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailVm confirmVm)
        {
            string sentCode = TempData["Code"] as string;
            string userJson = TempData["User"] as string;

            if (confirmVm.VerificationCode != sentCode)
            {
                ModelState.AddModelError("", "Kod səhvdir");
                return View();
            }

            var userVm = System.Text.Json.JsonSerializer.Deserialize<ApplicationUserVm>(userJson);

            var appUser = new ApplicationUser
            {
                Name = userVm.Name,
                Surname = userVm.Surname,
                UserName = userVm.Username,
                Email = userVm.Email
            };

            var result = await _userManager.CreateAsync(appUser, userVm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View();
            }

            await _userManager.AddToRoleAsync(appUser, "User");
            return RedirectToAction("Login");
        }

        public IActionResult Login() { return View(); }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm loginVm)
        {
            if (!ModelState.IsValid) { return View(loginVm); }
            ApplicationUser user = await _userManager.FindByEmailAsync(loginVm.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email ve ya Password sehvdir");
                return View(loginVm);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginVm.Password, true);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Birazdan yeniden daxil olmagi sinayin");
                return View();
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Email ve ya Password sehvdir");
                return View();
            }

            await _signInManager.SignInAsync(user, true);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }



        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Belə bir istifadəçi tapılmadı");
                return View(model);
            }

            
            string code = new Random().Next(100000, 999999).ToString();

            
            try
            {
                await _emailSender.SendEmailAsync(model.Email, "Parol sıfırlama kodu", $"Kodunuz: <strong>{code}</strong>");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Email göndərilə bilmədi: {ex.Message}");
                return View(model);
            }

            
            TempData["ResetEmail"] = model.Email;
            TempData["ResetCode"] = code;

            return RedirectToAction("ConfirmResetCode");
        }


        public IActionResult ConfirmResetCode()
        {
            
            var email = TempData.Peek("ResetEmail") as string;

            return View(new ConfirmResetVm
            {
                Email = email
            });
        }


        [HttpPost]
        public IActionResult ConfirmResetCode(ConfirmResetVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string code = TempData["ResetCode"] as string;
            string email = model.Email; 

            if (string.IsNullOrEmpty(code) || model.Code != code)
            {
                ModelState.AddModelError("", "Kod düzgün deyil və ya vaxtı bitib");
                return View(model);
            }

            
            TempData["ResetConfirmedEmail"] = email;

            return RedirectToAction("ResetPassword");
        }



        public IActionResult ResetPassword()
        {
            var email = TempData.Peek("ResetConfirmedEmail") as string;

            if (email == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordVm
            {
                Email = email
            });
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = TempData["ResetConfirmedEmail"] as string;
            if (email == null)
            {
                ModelState.AddModelError("", "Sessiya vaxtı bitmişdir. Yenidən cəhd edin.");
                return RedirectToAction("ForgotPassword");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "İstifadəçi tapılmadı");
                return View(model);
            }

            
            var removePass = await _userManager.RemovePasswordAsync(user);
            if (!removePass.Succeeded)
            {
                ModelState.AddModelError("", "Parol silinə bilmədi");
                return View(model);
            }

            var setPass = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!setPass.Succeeded)
            {
                foreach (var err in setPass.Errors)
                    ModelState.AddModelError("", err.Description);

                return View(model);
            }

            return RedirectToAction("Login");
        }



    }
}
