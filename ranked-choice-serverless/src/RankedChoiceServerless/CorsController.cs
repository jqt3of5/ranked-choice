using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace RankedChoiceServerless
{
    public class CorsController
    {
        public async Task<APIGatewayProxyResponse> OnOption(APIGatewayProxyRequest apiProxyEvent,
            ILambdaContext context)
        {
            return new APIGatewayProxyResponse()
            {
                Headers = new Dictionary<string, string>()
                {
                    { "Access-Control-Allow-Headers", "Content-Type,userId" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS,POST,GET" }
                },
                StatusCode = 200
            };
        }
    }
}