﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<ChatHub> _hubContext;

        public AccountManagerController(IMapper mapper, UserManager<User> userManager, SignInManager<User> signInManager, IUnitOfWork unitOfWork, IHubContext<ChatHub> hubContext)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
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

        #region Чат
        [Route("Chat")]
        [HttpPost]
        public async Task<IActionResult> Chat(string id)
        {
            var currentuser = User;

            var sender = await _userManager.GetUserAsync(currentuser);
            var recepient = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Message>() as MessageRepository;
            var mess = await repository.GetMessages(sender, recepient);

            var model = new ChatViewModel()
            {
                Sender = sender,
                Recepient = recepient,
                MessageHistory = mess.OrderBy(x => x.Id).ToList(),
            };

            return View("Chat", model);
        }

        [Route("Chat")]
        [HttpGet]
        public async Task<IActionResult> Chat()
        {
            var id = Request.Query["id"];
            var model = await GenerateChat(id);
            
            return View("Chat", model);
        }

        [Route("NewMessage")]
        [HttpPost]
        public async Task<IActionResult> NewMessage(string id, ChatViewModel chat)
        {
            var currentuser = User;

            var sender = await _userManager.GetUserAsync(currentuser);
            var recepient = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Message>() as MessageRepository;
            var item = new Message()
            {
                Sender = sender,
                Recipient = recepient,
                Text = chat.NewMessage.Text,
            };
            await repository.Create(item);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", item.Sender, item.Text);

            var mess = await repository.GetMessages(sender, recepient);

            var model = new ChatViewModel()
            {
                Sender = sender,
                Recepient = recepient,
                MessageHistory = mess.OrderBy(x => x.Id).ToList(),
            };

            return View("Chat", model);
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

        #region Generate.Test
        [Route("Generate")]
        [HttpGet]
        public async Task<IActionResult> Generate()
        {

            var usergen = new GenetateUsers();
            var userlist = usergen.Populate(35);

            foreach (var user in userlist)
            {
                var result = await _userManager.CreateAsync(user, "123456");

                if (!result.Succeeded)
                    continue;
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion

        private async Task<SearchViewModel> CreateSearch(string search)
        {
            var currentuser = User;

            var result = await _userManager.GetUserAsync(currentuser);

            var usersBySearch = new List<User>();

            if (string.IsNullOrEmpty(search))
                usersBySearch = await _userManager.Users.ToListAsync();
            else
                usersBySearch = await _userManager.Users
                    .Where(x => (x.FirstName + " " + x.MiddleName + " " + x.LastName).ToLower().Contains(search.ToLower()))
                    .ToListAsync();

            var withfriend = await GetAllFriend();

            var data = new List<UserWithFriendExt>();
            usersBySearch.ForEach(x =>
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

        private async Task<IEnumerable<User>> GetAllFriend()
        {
            var user = User;

            var result = await _userManager.GetUserAsync(user);

            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;

            return await repository.GetFriendsByUser(result);
        }

        private async Task<IEnumerable<User>> GetAllFriend(User user)
        {
            var repository = _unitOfWork.GetRepository<Friend>() as FriendsRepository;

            return await repository.GetFriendsByUser(user);
        }

        private async Task<ChatViewModel> GenerateChat(string id)
        {
            var currentuser = User;

            var sender = await _userManager.GetUserAsync(currentuser);
            var recepient = await _userManager.FindByIdAsync(id);

            var repository = _unitOfWork.GetRepository<Message>() as MessageRepository;
            var mess = await repository.GetMessages(sender, recepient);

            var model = new ChatViewModel()
            {
                Sender = sender,
                Recepient = recepient,
                MessageHistory = mess.OrderBy(x => x.Id).ToList(),
            };

            return model;
        }
    }
}
