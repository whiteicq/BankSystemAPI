using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankSystemAPI.Tests.TokenServiceTests
{
    public class TokenServiceTests : IDisposable
    {
        protected readonly Mock<IConfiguration> _configurationMock;
        protected readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        protected readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();

            // Setup mandatory JWT configuration values required by the TokenService
            // We use a 32-character key because HmacSha256 requires a key size of at least 256 bits
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("SUPER_SECRET_SECURITY_KEY_1234567890");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("BankSystemAPI");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("BankSystemClients");

            // UserManager requires a dummy IUserStore implementation in its constructor to prevent internal null reference errors
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _tokenService = new TokenService(_configurationMock.Object, _userManagerMock.Object);
        }

        // Helper method to create a clean test ApplicationUser in your style
        protected ApplicationUser CreateTestUser(long id = 1L, string email = "testuser@bank.by")
        {
            return new ApplicationUser
            {
                Id = id,
                Email = email,
                UserName = email
            };
        }

        public void Dispose()
        {

        }

        [Fact]
        public async Task GenerateJwtTokenAsync_UserWithRoles_ShouldReturnValidSignedJwtToken()
        {
            // Arrange
            ApplicationUser user = CreateTestUser(id: 42L, email: "ivanov@bank.by");
            var userRoles = new List<string> { "Client", "PremiumUser" };

            // Setup UserManager mock to return specific roles for this user
            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(userRoles);

            // Act
            string tokenString = await _tokenService.GenerateJwtTokenAsync(user);

            // Assert
            // Verify that the returned token is not empty or malformed
            Assert.NotNull(tokenString);
            Assert.NotEmpty(tokenString);

            // Decode the generated token string back into an object to read its inside state and claims
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(tokenString);

            // 1. Verify standard JWT structural configuration headers and signatures
            Assert.Equal("BankSystemAPI", jwtToken.Issuer);
            Assert.Contains("BankSystemClients", jwtToken.Audiences);
            Assert.Equal(SecurityAlgorithms.HmacSha256, jwtToken.SignatureAlgorithm);

            // 2. Extract and verify core identity claims generated inside the token structure
            string nameIdentifierClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string emailClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;

            Assert.Equal("42", nameIdentifierClaim);
            Assert.Equal("ivanov@bank.by", emailClaim);

            // 3. Verify that all assigned user roles were successfully written into the token payload collection
            var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.Equal(2, roleClaims.Count);
            Assert.Contains("Client", roleClaims);
            Assert.Contains("PremiumUser", roleClaims);

            // Verify that UserManager interface was triggered exactly once to fetch roles
            _userManagerMock.Verify(m => m.GetRolesAsync(user), Times.Once);
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_UserWithoutRoles_ShouldReturnTokenWithoutRoleClaims()
        {
            // Arrange
            ApplicationUser user = CreateTestUser(id: 10L, email: "anonymous@bank.by");

            // Setup UserManager mock to return an empty list (user has zero security roles assigned)
            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            string tokenString = await _tokenService.GenerateJwtTokenAsync(user);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(tokenString);

            // Verify that identity claims are present, but security role claims collection is completely empty
            Assert.Equal("10", jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Empty(jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role));

            _userManagerMock.Verify(m => m.GetRolesAsync(user), Times.Once);
        }
    }
}
