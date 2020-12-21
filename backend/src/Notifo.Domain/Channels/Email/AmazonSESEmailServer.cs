// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Notifo.Infrastructure.Initialization;

namespace Notifo.Domain.Channels.Email
{
    public sealed class AmazonSESEmailServer : IEmailServer, IInitializable
    {
        private const int MaxEmailAddressesPerRequest = 100;
        private static readonly ContentType Plain = new ContentType("text/plain");
        private static readonly ContentType Html = new ContentType("text/html");
        private readonly ObjectPool<SmtpClient> clientPool;
        private readonly AmazonSESOptions options;
        private AmazonSimpleEmailServiceClient amazonSES;

        internal sealed class SmtpClientPolicy : PooledObjectPolicy<SmtpClient>
        {
            private readonly AmazonSESOptions options;

            public SmtpClientPolicy(AmazonSESOptions options)
            {
                this.options = options;
            }

            public override SmtpClient Create()
            {
                return new SmtpClient(options.Host, options.Port)
                {
                    Credentials = new NetworkCredential(
                        options.Username,
                        options.Password),

                    EnableSsl = options.Secure,

                    Timeout = options.Timeout
                };
            }

            public override bool Return(SmtpClient obj)
            {
                return true;
            }
        }

        public AmazonSESEmailServer(IOptions<AmazonSESOptions> options)
        {
            clientPool = new DefaultObjectPoolProvider().Create(new SmtpClientPolicy(options.Value));

            this.options = options.Value;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            amazonSES = new AmazonSimpleEmailServiceClient(options.AwsAccessKeyId, options.AwsSecretAccessKey, RegionEndpoint.EUCentral1);

            return Task.CompletedTask;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            var smtpClient = clientPool.Get();
            try
            {
                using (ct.Register(smtpClient.SendAsyncCancel))
                {
                    var smtpMessage = new MailMessage
                    {
                        From = new MailAddress(message.FromEmail, message.FromName)
                    };

                    smtpMessage.To.Add(new MailAddress(message.ToEmail, message.ToName));

                    var hasHtml = !string.IsNullOrWhiteSpace(message.BodyHtml);
                    var hasText = !string.IsNullOrWhiteSpace(message.BodyText);

                    smtpMessage.IsBodyHtml = hasHtml;

                    if (hasHtml && hasText)
                    {
                        smtpMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.BodyHtml!, Html));
                        smtpMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.BodyText!, Plain));
                    }
                    else if (hasHtml)
                    {
                        smtpMessage.Body = message.BodyHtml;
                    }
                    else if (hasText)
                    {
                        smtpMessage.Body = message.BodyText;
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot send email without text body or html body");
                    }

                    smtpMessage.Subject = message.Subject;

                    await smtpClient.SendMailAsync(smtpMessage, ct);
                }
            }
            finally
            {
                clientPool.Return(smtpClient);
            }
        }

        public async Task RemoveEmailAddressAsync(string emailAddress, CancellationToken ct = default)
        {
            var request = new DeleteIdentityRequest
            {
                Identity = emailAddress
            };

            await amazonSES.DeleteIdentityAsync(request, ct);
        }

        public async Task<EmailVerificationStatus> AddEmailAddressAsync(string emailAddress, CancellationToken ct = default)
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

        public async Task<Dictionary<string, EmailVerificationStatus>> GetStatusAsync(HashSet<string> emailAddresses, CancellationToken ct = default)
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
