using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkWebApp.Models.Users;
using SocialNetworkWebApp.ViewModels;
using SocialNetworkWebApp.ViewModels.Account;
using SocialNetworkWebApp.ViewModels.AccountManager;

namespace SocialNetworkWebApp.Controllers.Account
{
    public class AccountManagerController : Controller
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountManagerController(IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("Login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View("Home/Login");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [Route("Login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                //var user = _mapper.Map<User>(model);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Пользователь не найден");
                    return View("Views/Home/Index.cshtml");
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("MyPage", "AccountManager");
                        //return View("Views/AccountManager/User.cshtml", new UserViewModel(user));
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View("Views/Home/Index.cshtml", new MainViewModel());
        }

        [Route("MyPage")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyPage()
        {
            var user = User;

            var result = await _userManager.GetUserAsync(user);

            if (result == null)
            {
                ModelState.AddModelError("", "Пользователь не найден");
                return View("Views/Home/Index.cshtml");
            }

            return View("User", new UserViewModel(result));
        }

        [Route("Edit")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = User;

            var result = await _userManager.GetUserAsync(user);
            
            if (result == null)
            {
                ModelState.AddModelError("", "Пользователь не найден");
                return View("Views/Home/Index.cshtml");
            }

            var editUser = _mapper.Map<UserEditViewModel>(result);

            return View("Edit", editUser);
        }

        [Route("Update")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update(UserEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);

                user.Convert(model); //через маппер нельзя, так как не обновить конкретного пользователя

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("MyPage", "AccountManager");
                }
                else
                {
                    return RedirectToAction("Edit", "AccountManager");
                }
            }
            else
            {
                ModelState.AddModelError("", "Некорректные данные");
                return View("Edit", model);
            }
        }

        [Route("Logout")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
