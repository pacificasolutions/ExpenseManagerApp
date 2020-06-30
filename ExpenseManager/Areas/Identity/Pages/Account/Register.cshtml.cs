using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ExpenseManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;
using RazorHTMLEmails.Services;
using RazorHTMLEmails.Views.Emails.ConfirmAccount;

namespace ExpenseManager.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ExpenseManagerUser> _signInManager;
        private readonly UserManager<ExpenseManagerUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private IHostingEnvironment _env;
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;

        public RegisterModel(
            UserManager<ExpenseManagerUser> userManager,
            SignInManager<ExpenseManagerUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IHostingEnvironment env,
            IRazorViewToStringRenderer razorViewToStringRenderer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _env = env;
            _razorViewToStringRenderer = razorViewToStringRenderer;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Birth Date")]
            [DataType(DataType.Date)]
            public DateTime DOB { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ExpenseManagerUser
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    DOB = Input.DOB,
                    UserName = Input.Email,
                    Email = Input.Email
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    var webRoot = _env.WebRootPath; //get wwwroot Folder 

                    //Get TemplateFile located at wwwroot/Templates/EmailTemplate/Register_EmailTemplate.html  
                    var pathToFile = webRoot
                            + Path.DirectorySeparatorChar.ToString()
                            + "Templates"
                            + Path.DirectorySeparatorChar.ToString()
                            + "EmailTemplate"
                            + Path.DirectorySeparatorChar.ToString()
                            + "Welcome_EmailTemplate.html";

                    var subject = "Confirm your email";

                    var builder = new StringBuilder(); // BodyBuilder();
                    using (var reader = System.IO.File.OpenText(pathToFile))
                    {
                        builder.Append(reader.ReadToEnd());
                        builder.Replace("{0}", Input.FirstName);
                        builder.Replace("{1}", callbackUrl);
                    }
                    string messageBody = builder.ToString();

                    //var builder = new BodyBuilder();

                    //using (var reader = System.IO.File.OpenText(pathToFile))
                    //{
                    //    builder.HtmlBody = reader.ReadToEnd();
                    //}

                    //string messageBody = string.Format(builder.HtmlBody,
                    //Input.FirstName
                    //);

                    var confirmAccountModel = new ConfirmAccountEmailViewModel(callbackUrl, Input.FirstName);

                    string body = await _razorViewToStringRenderer.RenderViewToStringAsync("/Views/Emails/ConfirmAccount/ConfirmAccountEmail.cshtml", confirmAccountModel);

                    await _emailSender.SendEmailAsync(Input.Email, subject, body);
                    //$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
