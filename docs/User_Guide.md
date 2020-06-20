# User Guide

This is a basic guide on how to use Radeon Software Slimmer. The `About` tab shows the version you are running and links to the GitHub repository to access documentation, downloads and source code. At the right side of the title bar are options to switch to `Light` or `Dark` theme. It defaults to your system settings for applications.

Radeon Software Slimmer can be broken down into two main categories. `Pre Install` and `Post Install`.

## Pre Install

Pre Install modifies the installer in order to remove or prevent certain components from installing. To start, you will need to download the drivers installer. This software ultimately reads from an extracted installer folder. It can extract the files for you, if needed.

Once you have the installer files extracted, reading from that location will load various components from the installer. Packages can be drivers or software to install. Scheduled Tasks are Windows Scheduled Tasks that can run at startup, on a schedule or from a trigger.

Unchecking `Keep` for packages will remove them from the respective JSON file. This will prevent the installer from installing these. Unchecking `Enabled` for Scheduled Tasks will still install them, but with a disabled status so that they do not run.

When you are doing making changes, click the `Modify Installer Files' button to save the changes.

Lastly, click the button to start the installer.

## Post Install

Post Install modifies the system after Radeon Software has already been installed. To start, click the load/refresh button to load the components that are installed on your system.

Radeon Software Host Services is a set of processes that run in the background and provide various functionality. The parent process is AMDRSServ.exe and can spawn multiple child processes. What this all does is not completely known, but does provide things like overlays, hotkeys, instant replay, recording and more. Unticking Enabled will rename an executable that is used to load these processes, preventing them from starting up. Re-enabling it will undo the rename, allowing it to run again.

Windows Scheduled Tasks can be Enabled or Disabled.

System Services are system drivers or NT Services that get installed with Radeon Software. You can change the start mode to disable these. Be _very careful_ with what you change here as an improper startmode could cause your OS to fail to boot.

Installed Items shows Windows Installer entries, even those that are hidden from Programs and Features in the control panel. Check `Uninstall` to uninstall them.

Click the `Apply Post Install Changes` button to apply the changes. Some changes need a system reboot to take affect, so a restart after this is recommended.

## Troubleshooting

Logging is available on the `Logging` tab. If you are trying to troubleshoot an issue, learn more about what it's doing or reports an issue, then take a look here. It reports in real time. Logging can be manually cleared or saved to file.