using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AlexaFunction
{
    public static class Security
    {
        private static readonly string ISSUER = "https://sts.windows.net/XXXXXXXX-XXXX-XXXX-aec1-f1f2a290f738/";
        private static readonly string AUDIENCE = "[Insert App ID URI]";
        private static readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        static Security()
        {
            HttpDocumentRetriever documentRetriever = new HttpDocumentRetriever();
            documentRetriever.RequireHttps = ISSUER.StartsWith("https://");

            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{ISSUER}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever);
        }

        public static async Task<ClaimsPrincipal> ValidateTokenAsync(string value)
        {
            var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
            var issuer = ISSUER;
            var audience = AUDIENCE;

            var validationParameter = new TokenValidationParameters()
            {
                RequireSignedTokens = true,
                ValidAudience = audience,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            ClaimsPrincipal result = null;
            var tries = 0;

            while (result == null && tries <= 1)
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    result = handler.ValidateToken(value, validationParameter, out var token);
                }
                catch (SecurityTokenSignatureKeyNotFoundException)
                {
                    // This exception is thrown if the signature key of the JWT could not be found.
                    // This could be the case when the issuer changed its signing keys, so we trigger a 
                    // refresh and retry validation.
                    _configurationManager.RequestRefresh();
                    tries++;
                }
                catch (SecurityTokenException)
                {
                    return null;
                }
            }

            return result;
        }
    }
}
