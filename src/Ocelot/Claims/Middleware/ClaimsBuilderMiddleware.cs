﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Infrastructure.RequestData;
using Ocelot.Logging;
using Ocelot.Middleware;

namespace Ocelot.Claims.Middleware
{
    public class ClaimsBuilderMiddleware : OcelotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAddClaimsToRequest _addClaimsToRequest;
        private readonly IOcelotLogger _logger;

        public ClaimsBuilderMiddleware(RequestDelegate next, 
            IRequestScopedDataRepository requestScopedDataRepository,
            IOcelotLoggerFactory loggerFactory,
            IAddClaimsToRequest addClaimsToRequest) 
            : base(requestScopedDataRepository)
        {
            _next = next;
            _addClaimsToRequest = addClaimsToRequest;
            _logger = loggerFactory.CreateLogger<ClaimsBuilderMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            _logger.LogDebug("started claims middleware");

            if (DownstreamRoute.ReRoute.ClaimsToClaims.Any())
            {
                _logger.LogDebug("this route has instructions to convert claims to other claims");

                var result = _addClaimsToRequest.SetClaimsOnContext(DownstreamRoute.ReRoute.ClaimsToClaims, context);

                if (result.IsError)
                {
                    _logger.LogDebug("error converting claims to other claims, setting pipeline error");

                    SetPipelineError(result.Errors);
                    return;
                }
            }

            _logger.LogDebug("calling next middleware");

            await _next.Invoke(context);

            _logger.LogDebug("succesfully called next middleware");
        }
    }
}
