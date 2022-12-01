# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2022-11-30

### Added

* **API**: New endpoints to fetch notifications across users.
* **API**: New endpoint to fetch unseen notifications.
* **API**: More API tests.
* **General**: Internal Notifo APP.

### Changed

* **Email**: Updated MJML version.
* **Runtime**: Migration to Mediatr.
* **Runtime**: Moving to scoped namespaces.
* **Runtime**: Moved to .NET 7.
* **Tracking**: New tracking system.

### Fixed

* **API**: Fixed to OpenAPI
* **Assets**: Fixes to asset proxy.

## [1.2.2] - 2022-08-10

### Fixed

* **Setup**: Fixed validation message.

## [1.2.1] - 2022-08-10

### Fixed

* **Setup**: Text fixed that was causing the crash.

## [1.2.0] - 2022-08-10

### Added

* **Added**: Diagnostics endpoints to generate dumps and gcdumps.
* **Added**: Fullscreen button for template editing.
* **Emails**: Unsubscribe screen for emails.
* **Integrations**: WhatsApp support for messaging channel.
* **Notifications**: Template variants: Choose a template by probability.
* **Testing**: Added verify to the API tests.
* **Testing**: Automated API testing with Google PubSub.
* **UI**: Better detail view for notifications.
* **UI**: Show last notification per user.

### Changed

* **Backend**: Hotfix for a memory leak introduced by XML comments in OpenAPI.
* **Messaging**: Better implementation for messaing solutions with test coverage.
* **Notifications**: Reworked the notification rpeferences.
* **UI**: Better UI for template management.

## [1.1.0] - 2022-04-23

### Added

* **Backend**: Started with full API tests, including Email, SMS, Webhooks.
* **Emails**: Custom MJML engine, based on .NET with 30x performance.
* **Emails**: Support for fluid templates.
* **Events** New filter to get events by channel.
* **Integrations**: Javascript conditions to decide when an integration should be used.
* **Notifications**: Better control when to send notifications through which channel.
* **Notifications**: New filter to get notifications by channel.
* **Notifications**: New tracking tokens with more information.
* **Topics**: New topic management.
* **UI**: Added a few hints to forms.

### Changed

* **Backend**: Improved enum serialization.
* **Backend**: Updated all dependencies.
* **Notifications**: Better tracking of notification channels.
* **UI**: Frontend topics and improved SDK.
* **UI**: New login screen.

### Fixed

* **Mobile Push**: Fixed removing of mobile push tokens.
* **UI**: Fix layout of apps list.
* **UI**: Fixed constant reloading of the user in the user dialog.

### Security

* **Apps**: Better check whether you have access to an app to prevent 500er http requests and return 403 properly in all cases.

## [1.0.2] - 2022-01-19

### Fixed

* **Identity**: Fixed a problem with roles.
* **Logging**: Turn off formatted logging.

## [1.0.1] - 2022-01-18

### Added

* **Identity**: Profile page to change email address and password.
* **Identity**: Basic UI for system user management.

### Changed

* **Identity**: Change default role to `Admin`.

### Fixed

* **UI**: Fixed top nav dropdown to close properly.

## [1.0.x] - 2022-01-XX

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security
