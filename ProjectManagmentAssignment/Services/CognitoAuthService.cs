using Amazon.CognitoIdentityProvider.Model;
using Amazon.CognitoIdentityProvider;
using Amazon;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ProjectManagmentAssignment.Controllers;

namespace ProjectManagmentApp.Services
{
    public class CognitoAuthService
    {
        private readonly AmazonCognitoIdentityProviderClient _cognitoClient;
        private readonly string _clientId;
        private readonly string _userPoolId;
        private readonly string _clientSecret;
        private readonly ILogger<CognitoAuthService> _logger;

        public CognitoAuthService(IConfiguration configuration, ILogger<CognitoAuthService> logger)
        {
            // Read configuration from appsettings.json
            _clientId = configuration["AWS:Cognito:ClientId"];
            _userPoolId = configuration["AWS:Cognito:UserPoolId"];
            _clientSecret = configuration["AWS:Cognito:ClientSecret"];
            string region = configuration["AWS:Region"];

            // Initialize AWS Cognito client
            _cognitoClient = new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(region));
            _logger = logger;
        }

        /// <summary>
        /// Generates a secret hash for AWS Cognito authentication
        /// </summary>
        public static string GenerateSecretHash(string username, string clientId, string clientSecret)
        {
            var message = Encoding.UTF8.GetBytes(username + clientId);
            var key = Encoding.UTF8.GetBytes(clientSecret);
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(message);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// User sign-in method using ADMIN_USER_PASSWORD_AUTH flow
        /// </summary>
        public async Task<AuthenticationResultType> SignInAsync(string username, string password)
        {
            try
            {
                var secretHash = GenerateSecretHash(username, _clientId, _clientSecret);

                // Initiate authentication request
                var authRequest = new AdminInitiateAuthRequest
                {
                    UserPoolId = _userPoolId,
                    ClientId = _clientId,
                    AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
                    AuthParameters = new Dictionary<string, string>
                    {
                        {"USERNAME", username},
                        {"PASSWORD", password},
                        {"SECRET_HASH", secretHash}
                    }
                };

                // Send the authentication request
                var authResponse = await _cognitoClient.AdminInitiateAuthAsync(authRequest);
                if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
                {
                    var challengeResponse = new RespondToAuthChallengeRequest
                    {
                        ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
                        ClientId = _clientId,
                        ChallengeResponses = new Dictionary<string, string>
                        {
                            { "USERNAME", username },
                            { "NEW_PASSWORD", "Tomer!234" }  // your new permanent password
                        },
                        Session = authResponse.Session
                    };

                    var challengeResult = await _cognitoClient.RespondToAuthChallengeAsync(challengeResponse);

                    return challengeResult.AuthenticationResult;
                }

                if (authResponse.ChallengeName != null)
                {
                    throw new Exception($"Authentication challenge required: {authResponse.ChallengeName}");
                }

                if (authResponse.AuthenticationResult == null)
                {
                    throw new Exception("Authentication failed, no result returned.");
                }
                // Return the authentication result with tokens
                return authResponse.AuthenticationResult;
            }
            catch (NotAuthorizedException ex)
            {
                _logger.LogWarning(ex, "Authentication failed for user {Username}: Incorrect credentials", username);
                throw new UnauthorizedAccessException("Incorrect username or password.");
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Authentication failed for user {Username}: User not found", username);
                throw new KeyNotFoundException("User does not exist.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user {Username}: Unexpected error", username);
                throw new ApplicationException($"An error occurred during sign-in: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user information from Cognito
        /// </summary>
        public async Task<AdminGetUserResponse> GetUserAsync(string username)
        {
            try
            {
                var userRequest = new AdminGetUserRequest
                {
                    UserPoolId = _userPoolId,
                    Username = username
                };

                return await _cognitoClient.AdminGetUserAsync(userRequest);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {Username} not found", username);
                throw new KeyNotFoundException($"User {username} does not exist.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user information for {Username}", username);
                throw new ApplicationException($"Error getting user information: {ex.Message}");
            }
        }

      
    }
}