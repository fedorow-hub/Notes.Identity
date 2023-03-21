using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes.Identity.Models;

namespace Notes.Identity.Controllers;

public class AuthController : Controller
{
    //Для реализации входа пользователя
    private readonly SignInManager<AppUser> _signInManager;
    //Для реализации поиска пользователя
    private readonly UserManager<AppUser> _userManager;
    //Для Logout
    private readonly IIdentityServerInteractionService _interactionService;

    public AuthController(SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IIdentityServerInteractionService interactionService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _interactionService = interactionService;
    }

    /// <summary>
    /// возвращает форму для ввода логина и пароля пользователем
    /// </summary>
    /// <param name="returnUrl">чтобы захватывать URL</param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        var viewModel = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };
        return View(viewModel);
    }

    /// <summary>
    /// место, куда будет переходить управление из формы логина, он будет перенаправлять туда, откуда
    /// пришел запрос
    /// </summary>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        //проверяем модель на валидность данных 
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        //ищем пользователя
        var user = await _userManager.FindByNameAsync(viewModel.UserName);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found");
            return View(viewModel);
        }
        //если пользователь найден, то с помощью _signInManager пытаемся сделать логин
        //параметр isPersistent относится к кукам (Persistent куки это когда мы закрываем браузер, а они сохраняются)
        //параметр logoutOnfailure предназначен, чтобы заблокировать аккаунт, если было несколько неуспешных попыток
        var result = await _signInManager.PasswordSignInAsync(viewModel.UserName,
            viewModel.Password, false, false);
        //в случае успеха делаем переход по адресу Url возврата
        if (result.Succeeded)
        {
            return Redirect(viewModel.ReturnUrl);
        }
        //иначе сообщаем об ошибке и возвращаем View, передав в нее нашу viewModel
        ModelState.AddModelError(string.Empty, "Login error");
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Register(string returnUrl)
    {
        var viewModel = new RegisterViewModel { ReturnUrl = returnUrl };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }
        //создаем пользователя с помощью _userManager
        var user = new AppUser { UserName = viewModel.UserName };
        var result = await _userManager.CreateAsync(user, viewModel.Password);

        if (result.Succeeded)
        {
            //делаем вход с помощью _signInManager и перенаправление
            await _signInManager.SignInAsync(user, false);
            return Redirect(viewModel.ReturnUrl);
        }
        //иначе возвращаем View с ошибкой
        ModelState.AddModelError(string.Empty, "Error occured");
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string logoutUd)
    {
        await _signInManager.SignOutAsync();
        //используем _interactionService чтобы получить LogoutContext и из него достать PostLogoutRedirectUri чтобы перейти
        var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutUd);
        return Redirect(logoutRequest.PostLogoutRedirectUri);
    }
}
