using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Interfaces
{
    public interface IAuthService
    {
        Task RegisterClientAsync(RegisterClientDto clientDto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
