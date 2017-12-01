using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AlexaFunction
{
    public static class Alexa
    {
        [FunctionName("Alexa")]
        public static async Task<SkillResponse> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [FromBody]SkillRequest request, TraceWriter log)
        {
            SkillResponse response = null;
            if (request?.Session?.User?.AccessToken != null)
            {
                ClaimsPrincipal principal = await Security.ValidateTokenAsync(request.Session.User.AccessToken);
                if (principal != null)
                {
                    PlainTextOutputSpeech outputSpeech = new PlainTextOutputSpeech();
                    string firstName = (request.Request as IntentRequest)?.Intent.Slots.FirstOrDefault(s => s.Key == "FirstName").Value?.Value;
                    outputSpeech.Text = "Hello " + firstName;
                    response = ResponseBuilder.Tell(outputSpeech);
                }
            }

            return response;
        }
    }
}
