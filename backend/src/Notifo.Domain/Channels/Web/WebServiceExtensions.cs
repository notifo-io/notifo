// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Web;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebServiceExtensions
    {
        public static void AddMyWebChannel(this IServiceCollection services)
        {
            services.AddSingletonAs<WebChannel>()
                .As<ICommunicationChannel>();
        }
    }
}
