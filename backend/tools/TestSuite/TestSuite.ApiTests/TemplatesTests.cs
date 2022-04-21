// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class TemplatesTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; set; }

        public TemplatesTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_create_templates()
        {
            // STEP 0: Create template.
            var templateCode1 = Guid.NewGuid().ToString();
            var templateCode2 = Guid.NewGuid().ToString();

            var templateRequest = new UpsertTemplatesDto
            {
                Requests = new List<UpsertTemplateDto>
                {
                    new UpsertTemplateDto
                    {
                        Code = templateCode1,
                        Formatting = new NotificationFormattingDto
                        {
                            Subject = new LocalizedText
                            {
                                ["en"] = "subject1_0"
                            }
                        }
                    },
                    new UpsertTemplateDto
                    {
                        Code = templateCode2,
                        Formatting = new NotificationFormattingDto
                        {
                            Subject = new LocalizedText
                            {
                                ["en"] = "subject2_0"
                            }
                        }
                    }
                }
            };


            var templates = await _.Client.Templates.PostTemplatesAsync(_.AppId, templateRequest);

            Assert.Equal(2, templates.Count);

            var template1 = templates.ElementAt(0);
            var template2 = templates.ElementAt(1);

            Assert.Equal(templateCode1, template1.Code);
            Assert.Equal(templateCode2, template2.Code);
            Assert.Equal("subject1_0", template1.Formatting.Subject["en"]);
            Assert.Equal("subject2_0", template2.Formatting.Subject["en"]);
        }

        [Fact]
        public async Task Should_update_templates()
        {
            // STEP 0: Create template.
            var templateCode1 = Guid.NewGuid().ToString();
            var templateCode2 = Guid.NewGuid().ToString();

            var templateRequest = new UpsertTemplatesDto
            {
                Requests = new List<UpsertTemplateDto>
                {
                    new UpsertTemplateDto
                    {
                        Code = templateCode1,
                        Formatting = new NotificationFormattingDto
                        {
                            Subject = new LocalizedText
                            {
                                ["en"] = "subject1_0"
                            }
                        }
                    },
                    new UpsertTemplateDto
                    {
                        Code = templateCode2,
                        Formatting = new NotificationFormattingDto
                        {
                            Subject = new LocalizedText
                            {
                                ["en"] = "subject2_0"
                            }
                        }
                    }
                }
            };

            await _.Client.Templates.PostTemplatesAsync(_.AppId, templateRequest);


            // STEP 1: Update template.
            var templateRequest2 = new UpsertTemplatesDto
            {
                Requests = new List<UpsertTemplateDto>
                {
                    new UpsertTemplateDto
                    {
                        Code = templateCode1,
                        Formatting = new NotificationFormattingDto
                        {
                            Subject = new LocalizedText
                            {
                                ["en"] = "subject1_1"
                            }
                        }
                    }
                }
            };

            await _.Client.Templates.PostTemplatesAsync(_.AppId, templateRequest2);


            // Get templates
            var templates = await _.Client.Templates.GetTemplatesAsync(_.AppId, take: 100000);

            var template1 = templates.Items.SingleOrDefault(x => x.Code == templateCode1);
            var template2 = templates.Items.SingleOrDefault(x => x.Code == templateCode2);

            Assert.Equal("subject1_1", template1?.Formatting.Subject["en"]);
            Assert.Equal("subject2_0", template2?.Formatting.Subject["en"]);
        }

        [Fact]
        public async Task Should_delete_templates()
        {
            // STEP 0: Create template.
            var templateCode1 = Guid.NewGuid().ToString();
            var templateCode2 = Guid.NewGuid().ToString();

            var templateRequest = new UpsertTemplatesDto
            {
                Requests = new List<UpsertTemplateDto>
                {
                    new UpsertTemplateDto
                    {
                        Code = templateCode1,
                        Formatting = new NotificationFormattingDto
                        {
                            Subject = new LocalizedText
                            {
                                ["en"] = "subject1"
                            }
                        }
                    },
                    new UpsertTemplateDto
                    {
                        Code = templateCode2,
                        Formatting = new NotificationFormattingDto
                        {
                            Subject = new LocalizedText
                            {
                                ["en"] = "subject2"
                            }
                        }
                    }
                }
            };

            await _.Client.Templates.PostTemplatesAsync(_.AppId, templateRequest);


            // STEP 1: Delete template.
            await _.Client.Templates.DeleteTemplateAsync(_.AppId, templateCode1);


            // Get templates
            var templates = await _.Client.Templates.GetTemplatesAsync(_.AppId, take: 100000);

            var template1 = templates.Items.SingleOrDefault(x => x.Code == templateCode1);
            var template2 = templates.Items.SingleOrDefault(x => x.Code == templateCode2);

            Assert.Null(template1);
            Assert.NotNull(template2);
        }
    }
}
