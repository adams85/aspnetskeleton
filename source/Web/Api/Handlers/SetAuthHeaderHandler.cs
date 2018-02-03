using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Common;
using AspNetSkeleton.Api.Helpers;
using AspNetSkeleton.Api.Infrastructure.Security;
using AspNetSkeleton.Api.Contract;

namespace AspNetSkeleton.Api.Handlers
{
    public class SetAuthHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
          
            ApiPrincipal principal;
            if (response != null &&
                (principal = request.GetRequestContext().Principal as ApiPrincipal) != null &&
                principal.Identity.IsAuthenticated)
            {
                var settings = response.RequestMessage.GetService<IApiSettings>();
                var clock = response.RequestMessage.GetService<IClock>();

                response.Headers.Add(ApiContractConstants.AuthTokenHttpHeaderName,
                    principal.RenewToken(clock, settings.AuthTokenExpirationTimeSpan, settings.EncryptionKey));
            }

            return response;
        }
    }
}