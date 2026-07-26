using BusinessLogicLayer.DTO;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BusinessLogicLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly DbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(DbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        public async Task RegisterClientAsync(string email, string password, string name, string? patronymic, string surname, string phoneNumber, DateOnly birthDate, string identificationNumber, string passportSeries, string passportNumber)
        {
            using (var _transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    ApplicationUser user = new ApplicationUser
                    {
                        UserName = $"{surname} {name} {patronymic}".Trim(),
                        PhoneNumber = phoneNumber,
                        Email = email
                    };

                    var identityResult = await _userManager.CreateAsync(user, password);
                    if (!identityResult.Succeeded)
                    {
                        throw new Exception();
                    }

                    await _userManager.AddToRoleAsync(user, "Client");

                    Passport passport = new Passport
                    {
                        IdentificationNumber = identificationNumber,
                        Series = passportSeries,
                        Number = passportNumber
                    };

                    _context.Set<Passport>().Add(passport);
                    _context.SaveChanges();

                    var clientProfile = new Client
                    {
                        UserId = user.Id,
                        PassportId = passport.Id,
                        Name = name,
                        Surname = surname,
                        Patronymic = patronymic,
                        PhoneNumber = phoneNumber,
                        BirthDate = birthDate,
                    };

                    _context.Set<Client>().Add(clientProfile);
                    
                    await _context.SaveChangesAsync();
                    await _transaction.CommitAsync();
                }
                catch
                {
                    _transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return string.Empty;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            // если аккаунт уже временно заморожен
            if (result.IsLockedOut)
            {
                return string.Empty;
            }

            if (!result.Succeeded)
            {
                return string.Empty;
            }

            var token = await _tokenService.GenerateJwtTokenAsync(user);

            return token;
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
