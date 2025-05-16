using Microsoft.AspNetCore.Mvc;
using ProjectManagmentApp.Services;


namespace ProjectManagmentAssignment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CognitoAuthService _cognitoAuthService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(CognitoAuthService cognitoAuthService, ILogger<UsersController> logger)
        {
            _cognitoAuthService = cognitoAuthService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user using AWS Cognito
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication result with tokens</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("User login attempt: {Username}", request.Username);
                var authResult = await _cognitoAuthService.SignInAsync(request.Username, request.Password);

                var response = new LoginResponse
                {
                    AccessToken = authResult.AccessToken,
                    IdToken = authResult.IdToken,
                    RefreshToken = authResult.RefreshToken,
                    ExpiresIn = authResult.ExpiresIn,
                    TokenType = authResult.TokenType
                };

                _logger.LogInformation("User login successful: {Username}", request.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", request.Username);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets information about a user
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <returns>User information</returns>
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            try
            {
                _logger.LogInformation("Fetching user information: {Username}", username);
                var userInfo = await _cognitoAuthService.GetUserAsync(username);

                var userAttributes = userInfo.UserAttributes
                    .ToDictionary(attr => attr.Name, attr => attr.Value);

                var response = new UserInfoResponse
                {
                    Username = userInfo.Username,
                    UserStatus = userInfo.UserStatus.Value,
                    UserCreateDate = userInfo.UserCreateDate,
                    UserLastModifiedDate = userInfo.UserLastModifiedDate,
                    Attributes = userAttributes
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user information for {Username}", username);
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Request and response models
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public int? ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }

    public class UserInfoResponse
    {
        public string Username { get; set; }
        public string UserStatus { get; set; }
        public DateTime? UserCreateDate { get; set; }
        public DateTime? UserLastModifiedDate { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}
