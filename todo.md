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

| # | Status | Issue |
|---|--------|-------|
| 1 | Fixed | **Seen state was never pushed to iOS devices.** `MobilePushChannel.cs`: `HandleSeenAsync` was missing the `!` before `TryGetValue`. Covered by `MobilePushChannelTests`. |
| 2 | Fixed | **Mailchimp treated accepted emails as failed.** `queued` and `scheduled` are accepted now, and the status is no longer deserialized to an enum, so unknown values do not throw. Not tested (needs the Mandrill API). |
| 3 | Fixed | **Telegram rejected messages with special characters.** The text is sent again as plain text when Telegram cannot parse the markdown. Not tested (needs the Telegram API). |
| 4 | Fixed | **Discord turned temporary errors into permanent failures.** Rate limits, 5xx responses and timeouts are thrown now, so the scheduler retries them. Not tested (needs the Discord API). |
| 5 | Fixed | **Grouped emails were sent before their own delay.** `EnqueueGroupedAsync` raises the due time of the batch to the due time of the last job (`$max`). |
| 6 | Fixed | **One invalid event dropped its whole group.** `UserNotificationService` now takes the last event of the group that produces a notification. |
| 7 | Fixed | **Exhausted user event jobs were never marked failed.** `HandleExceptionAsync` tracks a failure for every job of the batch. |
| 8 | Fixed | **SMS status was not updated when the app was missing.** The jobs are marked as `Handled`, like in the other channels. |
| 9 | Fixed | **A cancelled fan-out counted as complete.** The cursor loops no longer swallow the cancellation. |
| 10 | Partly fixed | **Non-grouped scheduling.** A job scheduled again with an earlier due time now updates the batch (`$min`). **Still open:** there is no unique index on the group key, so two nodes enqueuing at the same time can insert duplicate batches and send the message twice. |

## Round 3

All items verified in the code and open. Paths are relative to `backend/src/`.

1. **Open: a malformed tracking token returns HTTP 500 and the event is lost**
   - **Where:** `Notifo.Domain/TrackingToken.cs:49`
   - **Bug:** The guard is `decoded.Length >= 1` but then reads `decoded[1]`, and the catch only handles `FormatException`.
   - **Example:** A seen, delivered or confirm call with a base64 id that contains no `|` throws `IndexOutOfRangeException`. The confirmation is lost, so the channels that react to it never send.

2. **Open: pending integrations are never verified, so their notifications are dropped**
   - **Where:** `Notifo.Domain/Integrations/IntegrationManager.cs:243-256`
   - **Bug:** The result of `CheckStatusAsync` is discarded, and `status != configured.Status` compares a copy with itself, so it is never true. `newStatus` is assigned and never used.
   - **Example:** An Amazon SES integration stays `Pending` after AWS has confirmed the sender address. `Resolve` skips it and every email of that app is dropped.

3. **Open: the "already handled" check never matches**
   - **Where:** `Notifo.Domain/UserNotifications/MongoDb/MongoDbUserNotificationRepository.cs:135, 155, 173`
   - **Bug:** The filter compares the status as a number, while `StatusDictionarySerializer` stores it as a string.
   - **Example:** A delayed job asks `IsHandledAsync`, gets false although the channel already reported `Handled`, and sends the notification twice.

4. **Open: a dot in the schedule key destroys a mobile push job**
   - **Where:** `Notifo.Domain/Channels/MobilePush/MobilePushJob.cs:22-30`, used as a field path in `MongoDbSchedulerStore.cs:128`
   - **Bug:** The key contains the raw user id and group key. MongoDB treats a dot as a path separator, so the job is nested and comes back without its notification.
   - **Example:** A user id like `john.doe` makes the push fail with a `NullReferenceException` in `SchedulingChannelBase.HandleAsync`, and the notification is never marked failed.

5. **Open: dead Firebase tokens are kept and retried forever**
   - **Where:** `Notifo.Domain.Integrations/Firebase/FirebaseIntegration.MobilePush.cs:49-52`
   - **Bug:** Only `Unregistered` is mapped to `MobilePushTokenExpiredException`, which is the only case that removes a token.
   - **Example:** After a Firebase project change, tokens rejected with `SenderIdMismatch` burn all retries of every future notification and are never removed.

6. **Open: skipped deliveries stay at `Attempt`**
   - **Where:** `Notifo.Domain/Channels/MobilePush/MobilePushChannel.cs:191` (the same `> DeliveryStatus.Attempt` check is used in the other channels)
   - **Bug:** `Skipped` is 1 and `Attempt` is 2, so a skipped result is never written.
   - **Example:** A silent push to a device type with silent push disabled stays pending forever.

7. **Open: web push loses the status code of an error**
   - **Where:** `Notifo.Domain/Channels/WebPush/WebPushChannel.cs:168-171`
   - **Bug:** Every `WebPushException` other than 404 and 410 becomes a `DomainException`, so a 429 or 503 is a permanent failure and is never retried.
   - **Also:** The early return for updates at line 70 makes the `IsUpdate` branches at lines 94-101 dead code, so web push never delivers updates.

8. **Open: the app notification list hides everything with a correlation ID**
   - **Where:** `Notifo.Domain/UserNotifications/MongoDb/MongoDbUserNotificationRepository.cs:416-423`
   - **Bug:** The else branch adds `CorrelationId >= null`, which only matches null or missing values.
   - **Example:** Notifications from events published with a correlation ID are missing from the app view, although they were delivered.

9. **Open: repeated seen calls trigger the channels again**
   - **Where:** `Notifo.Domain/UserNotifications/MongoDb/TrackingBatch.cs:261-264`
   - **Bug:** `ShouldUpdate` is correct for `Updated` but inverted for the `First*` fields, so an already seen notification still reports a change.
   - **Example:** Every seen ping from a client runs `HandleSeenAsync` again, which schedules another iOS wakeup.

10. **Open: log entries are lost and a failed log write aborts the delivery**
    - **Where:** `Notifo.Domain/Log/Internal/LogCollector.cs:56-59, 89-98`
    - **Bug:** `StopAsync` does not flush, the queue is cleared before the write, and the exception is not caught, unlike in `StatisticsCollector`.
    - **Example:** A short database problem during a flush makes `LogStore.LogAsync` throw inside the send path and aborts the user event.

### Also noted

- **`MobilePushChannel.cs:126-139`:** iOS wakeup jobs are tracked like real notifications, which roughly doubles the mobile push counters for iOS users.
- **`Users/AddUserMobileToken.cs:36-39`:** Re-registering a known token never corrects its device type, so a token registered as `Unknown` never gets silent pushes.
- **Batch endpoints** (`EventsController.PostEvents`, `UsersController.PostUsers`, `TopicsController.PostTopics`): one invalid item fails the whole request after the earlier items have been applied, so a retry duplicates them.
