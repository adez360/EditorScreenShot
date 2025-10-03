# Changelog

All notable changes to the EditorScreenShot package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.1] - 2025-10-03

### Fixed
- Fixed bug where `EditorScreenShotSceneSync.EnsureSceneSyncEnabled(null)` was called with null parameter, causing scene sync functionality to break when exiting play mode
- Added `GetCurrentData()` method to `EditorScreenShotWindow` to provide safe access to EditorScreenShotData instance
- Updated hotkey handling methods to properly pass persist pose callback parameter

### Changed
- Improved error handling in PlayMode state change coordination
- Enhanced API design by adding public method instead of using reflection for data access
- Moved LICENSE file to package root directory for better visibility

## [Previous Versions]

*Previous changes not documented in this changelog.*
