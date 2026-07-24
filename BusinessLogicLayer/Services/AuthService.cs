using BusinessLogicLayer.DTO;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly DbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(DbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task RegisterClientAsync(RegisterClientDto clientDto)
        {
            using (var _transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    ApplicationUser user = new ApplicationUser
                    {
                        UserName = $"{clientDto.Surname} {clientDto.Name} {clientDto.Patronymic}",
                        PhoneNumber = clientDto.PhoneNumber,
                        Email = clientDto.Email
                    };

                    var identityResult = await _userManager.CreateAsync(user, clientDto.Password);
                    if (!identityResult.Succeeded)
                    {
                        throw new Exception();
                    }

                    await _userManager.AddToRoleAsync(user, "Client");

                    Passport passport = new Passport
                    {
                        IdentificationNumber = clientDto.Passport.IdentificationNumber,
                        Series = clientDto.Passport.Series,
                        Number = clientDto.Passport.Number
                    };

                    _context.Set<Passport>().Add(passport);
                    _context.SaveChanges();

                    var clientProfile = new Client
                    {
                        UserId = user.Id,          
                        PassportId = passport.Id,  
                        Name = clientDto.Name,
                        Surname = clientDto.Surname,
                        Patronymic = clientDto.Patronymic,
                        PhoneNumber = clientDto.PhoneNumber,
                        BirthDate = clientDto.BirthDate,
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

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
