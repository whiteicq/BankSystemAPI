using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAuthService
    {
        Task RegisterClientAsync(string email, string password, string name, string? patronymic, string surname, string phoneNumber, DateOnly birthDate, string identificationNumber, string passportSeries, string passportNumber);
        Task<string> LoginAsync(string email, string password);
        Task Logout();
    }
}
