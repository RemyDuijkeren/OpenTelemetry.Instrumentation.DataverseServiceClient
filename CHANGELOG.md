# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Add .NET 8.0 support

### Changed

- Improved SQL statements for Create, Update, Delete and Retrieve operations
- Update activity creation to include SQL statements directly, improving traceability of operations.
- Upgrade dependency: OpenTelemetry 1.10.0 (but support for 1.4 upwards)
- Upgrade dependency: Microsoft.PowerPlatform.Dataverse.Client 1.2.2 (but support for 1.1.16 upwards)

### Deprecated

- .NET 6.0 target will be removed in future releases

### Removed

### Fixed

### Security

## [1.2.0] - 2024-08-07

### Added

- Enable assembly signing so that it is a strong-named assembly

### Changed

- Upgrade dependency: OpenTelemetry 1.9.0 (but support for 1.4 upwards)
- Upgrade dependency: Microsoft.PowerPlatform.Dataverse.Client 1.1.32 (but support for 1.1.16 upwards)

## [1.1.0] - 2024-05-26

### Added

- Add SQL statement for Create, Update, Delete and Retrieve operations

## [1.0.0] - 2024-03-09

### Added

- Initial release.
