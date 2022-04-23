# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
