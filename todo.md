# Backend: Delivery Issues

Paths are relative to `backend/src/`. Each item has a status: **Fixed**, **By design** or **Open**.

## Round 1

| # | Status | Issue |
|---|--------|-------|
| 1 | By design | **Missing translation drops the notification.** `UserNotificationFactory.cs:49` does not fall back to another language. |
| 2 | Fixed | **Web push and webhook jobs overwrite each other.** The schedule key now includes `ConfigurationId`. |
| 3 | Fixed | **Redelivered events stopped the fan-out.** Events are stored as `Pending` until the fan-out completes, and a pending duplicate is published again. |
| 4 | Fixed | **Scheduler deleted jobs silently.** Covers inline retries, exhausted jobs after a crash, and the retry backoff counting from `DueTime`. |
| 5 | Fixed | **One store exception broke a scheduler queue.** The `ActionBlock` delegate now catches all exceptions. |
| 6 | Fixed | **Fallback to the next integration was skipped.** The check now compares integration IDs instead of shared sender instances. |
| 7 | Fixed | **Transient provider errors became permanent failures.** Affected SMTP, Twilio, Seven and Telekom. Also fixed: stale SMTP connections in the pool. |
| 8 | Fixed | **HTTP webhook ignored the response status.** |
| 9 | Fixed | **Status updates stopped after one DB error.** Covers `CompletionTimer`, `StatisticsCollector`, and the missing flush on shutdown. |
| 10 | By design | **MessageBird trims trailing zeros from phone numbers.** |
| 11 | Fixed | **Messaging status stayed `Attempt`.** `MessagingChannel.cs`: `lastResult` was never set on success. |
| 12 | Fixed | **User scheduling overrode event scheduling.** `UserNotificationService.cs:64`: the `Scheduling.Merged` arguments were swapped. |
| 13 | Fixed | **Subscription scheduling was never saved.** `MongoDbSubscription.cs` now maps `Scheduling`. |
| 14 | Fixed | **Grouped cancel raced with enqueue.** `MongoDbSchedulerStore.CompleteByKeyAsync` now only deletes batches that are still empty and not in progress. |
| 15 | Fixed | **iOS wakeup job shared a key with the real push.** `MobilePushJob.ScheduleKey`: wakeup jobs now use their own key. |
| 16 | Fixed | **`AtStrictly` threw on DST gaps and overlaps.** Replaced with `AtLeniently` in `Scheduling.cs`. |
| 17 | Fixed | **SMS and Messaging never retried.** Both queues now retry with `[5000, 30000, 60000]`. Risk: a timeout after the provider has accepted the message can cause a duplicate. |
| 18 | Fixed | **Telekom SMS always failed.** `TelekomSmsIntegration.Sms.cs:22` read the number property `PhoneNumber` with `GetString`, which throws. Found by the new tests. |

### Test coverage

- **Unit tests:** `TimerConsumerTests`, `CompletionTimerTests`, `MessagingChannelTests`, `ScheduleKeyTests`, `StatisticsCollectorTests`, `UserNotificationServiceTests`, `UserEventPublisherTests`, `SchedulingTests`, `TransientErrorsTests`, `IntegrationErrorTests`, `HttpIntegrationTests`.
- **Testcontainers (MongoDB):** `MongoDbSchedulerStoreTests`, `MongoDbEventRepositoryTests`, `MongoDbSubscriptionRepositoryTests`, `MongoDbUserNotificationRepositoryTests`.
- **Not tested:** the SMTP reconnect after a stale pooled connection (`SmtpEmailServer.cs`). It needs a real SMTP server that closes idle connections. The SMS and Messaging retry configuration is also untested.

## Round 2

1. **Open: seen state is never pushed to iOS devices**
   - **Where:** `Channels/MobilePush/MobilePushChannel.cs:53`
   - **Bug:** `HandleSeenAsync` is missing the `!` before `TryGetValue`. It returns exactly when a token exists.

2. **Open: Mailchimp treats accepted emails as failed**
   - **Where:** `Notifo.Domain.Integrations/Mailchimp/MailchimpIntegration.Email.cs:84`
   - **`queued`:** The status is treated as a permanent failure, although Mandrill still delivers the email.
   - **`scheduled`:** The status is missing from the enum. Deserialization throws, the job is retried, and the email is sent again.

3. **Open: Telegram rejects messages with special characters**
   - **Where:** `Notifo.Domain.Integrations/Telegram/TelegramIntegration.Messaging.cs:60`
   - **Bug:** Text is sent with `ParseMode.Markdown` without escaping.
   - **Example:** A lone `_`, `*` or `[` (common in URLs) makes Telegram return 400 on every attempt.

4. **Open: Discord turns temporary errors into permanent failures**
   - **Where:** `Notifo.Domain.Integrations/Discord/DiscordIntegration.Messaging.cs:93-98`
   - **Bug:** The catch-all retries 5 times immediately, including after timeouts and 5xx responses, then returns `Failed`. The scheduler never gets a chance to retry.

5. **Open: grouped emails are sent before their own delay**
   - **Where:** `MongoDbSchedulerStore.EnqueueGroupedAsync` together with `Channels/Email/EmailChannel.cs:67`
   - **Bug:** A job joins any pending batch that is due earlier, so its delay and `IfNotSeen` window are shortened.

6. **Open: one invalid event drops its whole group**
   - **Where:** `UserNotifications/UserNotificationService.cs:47, 91-96`
   - **Bug:** If the factory returns null for the last job in a group, all child events of that batch are dropped and the batch is completed.

7. **Open: exhausted user event jobs are never marked failed**
   - **Where:** `UserNotifications/UserNotificationService.cs:39`
   - **Bug:** `HandleExceptionAsync` is a no-op. Jobs that run out of attempts, for example after crashes, never get the `Failed` status.

8. **Open: SMS status is not updated when the app is missing**
   - **Where:** `Channels/Sms/SmsChannel.cs:115`
   - **Bug:** The channel only returns, so the notification stays at `Unknown`. Email and Messaging mark it `Handled` in this case.

9. **Open: cancelled fan-out to all users counts as complete**
   - **Where:** `Users/MongoDb/MongoDbUserRepository.cs:53`, `Subscriptions/MongoDb/MongoDbSubscriptionRepository.cs:85`
   - **Bug:** `while (await cursor.MoveNextAsync(ct) && !ct.IsCancellationRequested)` exits without throwing.
   - **Example:** For `users/all`, the fan-out ends normally on shutdown and is marked published, so the remaining users never get the event.

10. **Open: non-grouped scheduling problems**
    - **Where:** `MongoDbSchedulerStore.EnqueueAsync`
    - **Due time:** A reschedule with the same key cannot change the due time, because `DueTime` is only set on insert.
    - **Duplicates:** There is no unique index, so two nodes enqueuing at the same time can insert duplicate batches, and the message is sent twice.
