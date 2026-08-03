using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Enums.Logs;
namespace BusinessLogicLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly DbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILoggerService _loggerService;

        public AuthService(DbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, ILoggerService loggerService)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _loggerService = loggerService;
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
                        throw new Exception("Не удалось создать пользователя");
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
                        Passport = passport,
                        Name = name,
                        Surname = surname,
                        Patronymic = patronymic,
                        PhoneNumber = phoneNumber,
                        BirthDate = birthDate,
                    };

                    _context.Set<Client>().Add(clientProfile);

                    await _context.SaveChangesAsync();

                    _loggerService.MakeLog(OperationType.CLIENT_ADDED, nameof(Client), clientProfile.Id);

                    await _transaction.CommitAsync();
                }
                catch
                {
                    _transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task RegisterEmployeeAsync(string email, string password, string name, string? patronymic, string surname, string phoneNumber, DateOnly birthDate, string identificationNumber, string passportSeries, string passportNumber)
        {
            using (var _transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    ApplicationUser user = new ApplicationUser
                    {
                        UserName = $"{surname} {name} {patronymic}".Trim(),
                        Email = email
                    };

                    var identityResult = await _userManager.CreateAsync(user, password);
                    if (!identityResult.Succeeded)
                    {
                        throw new Exception("Не удалось создать пользователя");
                    }

                    await _userManager.AddToRoleAsync(user, "Employee");

                    Passport passport = new Passport
                    {
                        Series = passportSeries,
                        Number = passportNumber,
                        IdentificationNumber = identificationNumber
                    };

                    _context.Set<Passport>().Add(passport);
                    _context.SaveChanges();

                    Employee employee = new Employee
                    {
                        UserId = user.Id,
                        Name = name,
                        Patronymic = patronymic,
                        Surname = surname,
                        BirthDate = birthDate,
                        PhoneNumber = phoneNumber,
                        Passport = passport,
                        HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        Role = "Operator",
                    };

                    _context.Set<Employee>().Add(employee);

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
