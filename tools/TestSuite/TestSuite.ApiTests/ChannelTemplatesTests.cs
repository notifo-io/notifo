// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class ChannelTemplatesTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; set; }

    public ChannelTemplatesTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_and_update_email_template()
    {
        var name = Guid.NewGuid().ToString();

        // STEP 0: Create template. The default language of the app is used.
        var template_0 = await _.Client.EmailTemplates.PostTemplateAsync(_.AppId, new CreateChannelTemplateDto());

        Assert.Contains("en", template_0.Languages.Keys);


        // STEP 1: Add another language.
        var template_1 = await _.Client.EmailTemplates.PostTemplateLanguageAsync(_.AppId, template_0.Id, new CreateChannelTemplateLanguageDto
        {
            Language = "de"
        });

        Assert.Contains("de", template_1.Languages.Keys);


        // STEP 2: Update the name and the english template. The bodies of the default template are valid and reused.
        var default_0 = template_0.Languages["en"];

        var template_2 = await _.Client.EmailTemplates.PutTemplateAsync(_.AppId, template_0.Id, new UpdateChannelTemplateDtoOfEmailTemplateDto
        {
            Name = name,
            Primary = true,
            Languages = new Dictionary<string, EmailTemplateDto>
            {
                ["en"] = new EmailTemplateDto
                {
                    Subject = "subject_en",
                    BodyHtml = default_0.BodyHtml,
                    BodyText = default_0.BodyText,
                    FromEmail = "hello@notifo.io",
                    FromName = "Hello Notifo"
                }
            }
        });

        Assert.Equal(name, template_2.Name);
        Assert.True(template_2.Primary);
        Assert.Equal("subject_en", template_2.Languages["en"].Subject);
        Assert.DoesNotContain("de", template_2.Languages.Keys);


        // STEP 3: Update a single language.
        await _.Client.EmailTemplates.PutTemplateLanguageAsync(_.AppId, template_0.Id, "de", new EmailTemplateDto
        {
            Subject = "subject_de",
            BodyHtml = default_0.BodyHtml,
            BodyText = default_0.BodyText
        });

        var template_3 = await _.Client.EmailTemplates.GetTemplateAsync(_.AppId, template_0.Id);

        Assert.Equal("subject_en", template_3.Languages["en"].Subject);
        Assert.Equal("subject_de", template_3.Languages["de"].Subject);


        // STEP 4: Delete a single language.
        var template_4 = await _.Client.EmailTemplates.DeleteTemplateLanguageAsync(_.AppId, template_0.Id, "de");

        Assert.DoesNotContain("de", template_4.Languages.Keys);


        // STEP 5: Query the templates.
        var templates = await _.Client.EmailTemplates.GetTemplatesAsync(_.AppId, name);

        Assert.Contains(templates.Items, x => x.Id == template_0.Id && x.Name == name);


        // STEP 6: Delete the template.
        await _.Client.EmailTemplates.DeleteTemplateAsync(_.AppId, template_0.Id);

        var ex = await Assert.ThrowsAsync<NotifoException>(() => _.Client.EmailTemplates.GetTemplateAsync(_.AppId, template_0.Id));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Should_create_and_delete_sms_template()
    {
        var name = Guid.NewGuid().ToString();

        // STEP 0: Create template.
        var template_0 = await _.Client.SmsTemplates.PostTemplateAsync(_.AppId, new CreateChannelTemplateDto());

        Assert.Contains("en", template_0.Languages.Keys);


        // STEP 1: Update the template.
        var template_1 = await _.Client.SmsTemplates.PutTemplateAsync(_.AppId, template_0.Id, new UpdateChannelTemplateDtoOfSmsTemplateDto
        {
            Name = name,
            Primary = true,
            Languages = new Dictionary<string, SmsTemplateDto>
            {
                ["en"] = new SmsTemplateDto
                {
                    Text = "text_en"
                }
            }
        });

        Assert.Equal(name, template_1.Name);
        Assert.Equal("text_en", template_1.Languages["en"].Text);


        // STEP 2: Query the templates.
        var templates = await _.Client.SmsTemplates.GetTemplatesAsync(_.AppId, name);

        Assert.Contains(templates.Items, x => x.Id == template_0.Id && x.Name == name);


        // STEP 3: Delete the template.
        await _.Client.SmsTemplates.DeleteTemplateAsync(_.AppId, template_0.Id);

        var ex = await Assert.ThrowsAsync<NotifoException>(() => _.Client.SmsTemplates.GetTemplateAsync(_.AppId, template_0.Id));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Should_create_and_delete_messaging_template()
    {
        var name = Guid.NewGuid().ToString();

        // STEP 0: Create template.
        var template_0 = await _.Client.MessagingTemplates.PostTemplateAsync(_.AppId, new CreateChannelTemplateDto());

        Assert.Contains("en", template_0.Languages.Keys);


        // STEP 1: Update the template.
        var template_1 = await _.Client.MessagingTemplates.PutTemplateAsync(_.AppId, template_0.Id, new UpdateChannelTemplateDtoOfMessagingTemplateDto
        {
            Name = name,
            Primary = true,
            Languages = new Dictionary<string, MessagingTemplateDto>
            {
                ["en"] = new MessagingTemplateDto
                {
                    Text = "text_en"
                }
            }
        });

        Assert.Equal(name, template_1.Name);
        Assert.Equal("text_en", template_1.Languages["en"].Text);


        // STEP 2: Query the templates.
        var templates = await _.Client.MessagingTemplates.GetTemplatesAsync(_.AppId, name);

        Assert.Contains(templates.Items, x => x.Id == template_0.Id && x.Name == name);


        // STEP 3: Delete the template.
        await _.Client.MessagingTemplates.DeleteTemplateAsync(_.AppId, template_0.Id);

        var ex = await Assert.ThrowsAsync<NotifoException>(() => _.Client.MessagingTemplates.GetTemplateAsync(_.AppId, template_0.Id));

        Assert.Equal(404, ex.StatusCode);
    }
}
