// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Squidex.Hosting;

namespace Notifo.Domain.Channels.Email
{
    public sealed class AmazonSESEmailServer : SmtpEmailServerBase, IEmailServer, IInitializable
    {
        private const int MaxEmailAddressesPerRequest = 100;
        private readonly AmazonSESOptions options;
        private AmazonSimpleEmailServiceClient amazonSES;

        public AmazonSESEmailServer(IOptions<AmazonSESOptions> options)
            : base(options.Value)
        {
            this.options = options.Value;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            amazonSES = new AmazonSimpleEmailServiceClient(
                options.AwsAccessKeyId,
                options.AwsSecretAccessKey,
                RegionEndpoint.GetBySystemName(options.Region));

            return Task.CompletedTask;
        }

        public override async Task RemoveEmailAddressAsync(string emailAddress, CancellationToken ct = default)
        {
            var request = new DeleteIdentityRequest
            {
                Identity = emailAddress
            };

            await amazonSES.DeleteIdentityAsync(request, ct);
        }

        public override async Task<EmailVerificationStatus> AddEmailAddressAsync(string emailAddress, CancellationToken ct = default)
        {
            await RemoveEmailAddressAsync(emailAddress, ct);

            var verifyRequest = new VerifyEmailAddressRequest
            {
                EmailAddress = emailAddress
            };

            await amazonSES.VerifyEmailAddressAsync(verifyRequest, ct);

            return await GetStatusAsync(emailAddress, ct);
        }

        private async Task<EmailVerificationStatus> GetStatusAsync(string emailAddress, CancellationToken ct)
        {
            var request = new GetIdentityVerificationAttributesRequest
            {
                Identities = new List<string> { emailAddress }
            };

            var response = await amazonSES.GetIdentityVerificationAttributesAsync(request, ct);

            var status = response.VerificationAttributes.FirstOrDefault(x => string.Equals(emailAddress, x.Key, StringComparison.OrdinalIgnoreCase));

            return MapStatus(status.Value.VerificationStatus);
        }

        public override async Task<Dictionary<string, EmailVerificationStatus>> GetStatusAsync(HashSet<string> emailAddresses, CancellationToken ct = default)
        {
            var result = new Dictionary<string, EmailVerificationStatus>();

            var buckets = (int)Math.Ceiling((double)emailAddresses.Count / MaxEmailAddressesPerRequest);

            for (var i = 0; i < buckets; i++)
            {
                ct.ThrowIfCancellationRequested();

                var bucketAddresses = emailAddresses.Skip(i * MaxEmailAddressesPerRequest).Take(MaxEmailAddressesPerRequest);

                var request = new GetIdentityVerificationAttributesRequest
                {
                    Identities = bucketAddresses.ToList()
                };

                var response = await amazonSES.GetIdentityVerificationAttributesAsync(request, ct);

                foreach (var (key, attr) in response.VerificationAttributes)
                {
                    result[key] = MapStatus(attr.VerificationStatus);
                }
            }

            return result;
        }

        private static EmailVerificationStatus MapStatus(VerificationStatus status)
        {
            if (status.Equals(VerificationStatus.NotStarted))
            {
                return EmailVerificationStatus.NotStarted;
            }

            if (status.Equals(VerificationStatus.Pending))
            {
                return EmailVerificationStatus.Pending;
            }

            if (status.Equals(VerificationStatus.Success))
            {
                return EmailVerificationStatus.Verified;
            }

            return EmailVerificationStatus.Failed;
        }
    }
}
