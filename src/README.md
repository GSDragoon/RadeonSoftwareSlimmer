# Source

The source is split into 3 projects. This is mainly to improve testability on non-Windows platforms, despite the application only for running on Windows.

## Radeon.Software.Slimmer

The main WPF project that ties everything together and produces the executable users run. It references the Core and Windwos projects. All the UI code is in here.

## Radeon.Software.Slimmer.Core

The Core project contains the bulk of the back-end logic. It has abstractions around all the Windows only code to allow local development on other platforms. Tests can verify business logic without needing Windows.

## RadeonSoftwareSlimmer.Windows

This project implements all the abstractions in the Core with the Windows specific logic. Windows Registy, Scheduled Tasks, NT Services, etc. The tests against this verify the integration points with the local Windows OS.
