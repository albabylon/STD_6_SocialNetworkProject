using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkWebApp.Data.Repository;
using SocialNetworkWebApp.Data.UoW;
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
        private readonly IUnitOfWork _unitOfWork;

        public AccountManagerController(IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        #region Вход
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
        #endregion

        #region Моя Страница
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

            var model = new UserViewModel(result);

            model.Friends = await GetAllFriend(model.User);

            return View("User", model);
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
        #endregion

        #region Друзья
        [Route("UserList")]
        [HttpPost]
        public async Task<IActionResult> UserList(string search)
        {
            var model = await CreateSearch(search);

            return View("UserList", model);
        }

        [Route("AddFriend")]
        [HttpPost]
        public async Task<IActionResult> AddFriend(string id)
        {
            var currentuser = User;
            var result = await _userManager.GetUserAsync(currentuser);
            var friend = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;

            await repository.AddFriend(result, friend);

            return RedirectToAction("MyPage", "AccountManager");
        }

        [Route("DeleteFriend")]
        [HttpPost]
        public async Task<IActionResult> DeleteFriend(string id)
        {
            var currentuser = User;
            var result = await _userManager.GetUserAsync(currentuser);
            var friend = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;

            await repository.DeleteFriend(result, friend);

            return RedirectToAction("MyPage", "AccountManager");
        }
        #endregion

        #region Выход
        [Route("Logout")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion



        private async Task<SearchViewModel> CreateSearch(string search)
        {
            var currentuser = User;

            var result = await _userManager.GetUserAsync(currentuser);

            var list = await _userManager.Users
                .Where(x => x.GetFullName().Contains(search, StringComparison.CurrentCultureIgnoreCase))
                .ToListAsync();

            var withfriend = await GetAllFriend();

            var data = new List<UserWithFriendExt>();
            list.ForEach(x =>
            {
                var t = _mapper.Map<UserWithFriendExt>(x);
                t.IsFriendWithCurrent = withfriend.Where(y => y.Id == x.Id || x.Id == result.Id).Any();
                data.Add(t);
            });

            var model = new SearchViewModel()
            {
                UserList = data
            };

            return model;
        }

        private async Task<List<User>> GetAllFriend()
        {
            var user = User;

            var result = await _userManager.GetUserAsync(user);

            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;

            return await repository.GetFriendsByUser(result);
        }

        private async Task<List<User>> GetAllFriend(User user)
        {
            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;

            return await repository.GetFriendsByUser(user);
        }
    }
}
