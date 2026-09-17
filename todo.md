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

| # | Status | Issue |
|---|--------|-------|
| 1 | Fixed | **A malformed tracking token returned HTTP 500 and the event was lost.** `TrackingToken.cs`: the length check allowed a read past the end. Covered by `TrackingTokenTests`. |
| 2 | Fixed | **Pending integrations were never verified, so their notifications were dropped.** `IntegrationManager` uses the status returned by `CheckStatusAsync` now. Covered by `IntegrationManagerTests`. |
| 3 | Fixed | **The "already handled" check never matched.** The status is written and compared as string, and the filter also accepts the old numeric values. Covered by `MongoDbUserNotificationRepositoryTests`. |
| 4 | Fixed | **A dot in the schedule key destroyed a job.** The scheduler store escapes the key before it is used as a field path. Covered by `MongoDbSchedulerStoreTests`. |
| 5 | Fixed | **Dead Firebase tokens were kept and retried forever.** `SenderIdMismatch` removes the token now. Not tested (needs the Firebase API). |
| 6 | Fixed | **Skipped deliveries stayed at `Attempt`.** All channels track every result except `Unknown` and `Attempt`. Covered by `MessagingChannelTests`. |
| 7 | Fixed | **Web push lost the status code of an error.** Temporary errors are thrown now, so the scheduler retries them. The dead update branch has been removed, web push still does not send updates. |
| 8 | Fixed | **The app notification list hid everything with a correlation ID.** The `CorrelationId >= null` filter is gone. Covered by `MongoDbUserNotificationRepositoryTests`. |
| 9 | Fixed | **Repeated seen calls triggered the channels again.** The first timestamps are only written once, and the updated timestamp alone no longer counts as a change. Covered by `MongoDbUserNotificationRepositoryTests`. |
| 10 | Fixed | **Log entries were lost and a failed log write aborted the delivery.** `LogCollector` flushes on stop, keeps failed entries and no longer fails the caller. Covered by `LogCollectorTests`. |
| 11 | Fixed | **iOS wakeup jobs inflated the mobile push counters.** They are no longer tracked. Covered by `MobilePushChannelTests`. |
| 12 | Fixed | **A device type could never be corrected.** Registering a known token again updates the device type and identifier. Covered by `AddUserMobileTokenTests`. |
| 13 | Fixed | **Batch endpoints applied a prefix and then failed.** Events are validated before the first one is published, and the user endpoint no longer throws on a missing list. Not tested. |

### Open

- **No unique index on the scheduler group key** (`MongoDbSchedulerStore`): two nodes enqueuing the same key at the same moment can insert duplicate batches and send a message twice. A partial unique index would make concurrent upserts fail, so this needs a decision on the error handling.
- **Batch endpoints** still apply items one by one, so a failure inside a handler (not during validation) leaves the earlier items applied.
- **Not tested:** the SMTP reconnect after a stale pooled connection, the SMS and Messaging retry settings, and the Mailchimp, Telegram, Discord and Firebase changes, which all need the provider APIs.
