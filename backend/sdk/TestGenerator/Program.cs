// ==========================================================================
//  Notifications System
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Notifo.SDK;

namespace TestGenerator
{
    public static class Program
    {
        private const string AppId = "d51d6feb-87ff-407e-85fe-3d480d0d8bad";
        private const string ApiKey = "cyluprnqyya9h57c1xrlnzdbqkbixhj4dnxj8ruaaysx";
        private const string TopicPrefix = "products";
        private const string Topic = "users/123";

        public static async Task Main(string[] args)
        {
            var client =
                NotifoClientBuilder.Create()
                    .SetApiKey(ApiKey)
                    .SetApiUrl("https://localhost:5002")
                    .Build();

            var users = new List<string>();

            for (var i = 1; i <= 20; i++)
            {
                users.Add(i.ToString());
            }

            if (args?.Contains("--users") == true)
            {
                Console.WriteLine("Generating Users...");

                foreach (var userId in users)
                {
                    var request = new UpsertUserDto
                    {
                        EmailAddress = $"hello@squidex.io", Id = userId
                    };

                    await client.Users.PostUsersAsync(AppId, new UpsertUsersDto
                    {
                        Requests = new List<UpsertUserDto>
                        {
                            request
                        },
                    });
                }

                Console.WriteLine("Generated Users...");
            }

            if (args?.Contains("--subscriptions") == true)
            {
                Console.WriteLine("Generating Subscriptions...");

                foreach (var userId in users)
                {
                    var request = new SubscriptionDto { TopicPrefix = TopicPrefix };

                    await client.Users.PostSubscriptionAsync(AppId, userId, request);
                }

                Console.WriteLine("Generated Subscriptions...");
            }

            if (args?.Contains("--no-events") != true)
            {
                Console.WriteLine("Generating Events...");

                for (var i = 0; i < 1; i++)
                {
                    var request = new PublishDto
                    {
                        Topic = Topic
                    };

                    var formatting = new NotificationFormattingDto
                    {
                        Body = new LocalizedText
                        {
                            ["en"] = "Hello Body {{var}}",
                            ["de"] = "Hallo Body {{var}}"
                        },
                        Subject = new LocalizedText
                        {
                            ["en"] = "Hello Title {{var}}",
                            ["de"] = "Hallo Title {{var}}"
                        }
                    };

                    request.Properties = new EventProperties
                    {
                        ["var"] = "123"
                    };

                    request.Preformatted = formatting;

                    request.Timestamp = DateTimeOffset.UtcNow;

                    await client.Events.PostEventsAsync(AppId, new PublishManyDto
                    {
                        Requests = new List<PublishDto>
                        {
                            request
                        }
                    });
                }

                Console.WriteLine("Generated Events...");
            }
        }
    }
}
