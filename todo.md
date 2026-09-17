# Backend: Open Delivery Issues

Paths are relative to `backend/src/`.

The top 10 list has been handled. #1 (no language fallback) and #10 (MessageBird phone number trimming) are by design. The others are fixed.

## Still open

- **`MessagingChannel.cs:192`:** `lastResult` is never set on success, so the status stays `Attempt`.
- **`UserNotificationService.cs:64`:** The arguments to `Scheduling.Merged` are swapped, so user scheduling overrides the event's scheduling (the opposite of what the comment says).
- **`MongoDbSubscription.cs`:** Subscription `Scheduling` is never saved.
- **`MongoDbSchedulerStore.cs:156-165`:** Grouped cancel can race with enqueue and delete a job that was just added.
- **`MobilePush/MobilePushJob.cs:27`:** The iOS wakeup job and the real job share a schedule key.
- **`Scheduling.cs:60, 76`:** `AtStrictly` throws on DST gaps and overlaps, so the user event is dropped.
- **SMS and Messaging schedulers:** `ExecutionRetries = []` (in `SmsServiceExtensions.cs` and `MessagingServiceExtensions.cs`). Temporary provider errors are now thrown instead of wrapped, but these queues still don't retry them.
