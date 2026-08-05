### Compatibility

Extension versions are strongly tied to the Pixeval application version. Pixeval cannot load incompatible extensions. Please ensure both versions match.

### Priority

Pixeval extensions have a priority that can be adjusted on the extension management page — the higher an extension is in the list, the higher its priority.

The purpose of priority is that, in certain special cases (such as translation extensions), only one extension can be used for an operation, and due to UI limitations it is not convenient for the user to freely choose which extension to use. In such cases, the extension with the highest priority will be automatically selected.

### Uninstallation

Although Pixeval provides an uninstall function, due to [.NET limitations](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/libraries), .NET extensions cannot be fully unloaded while the program is running. Therefore, after closing Pixeval, you need to manually go to the extensions folder and delete the specified extension.Therefore, after closing Pixeval, you need to manually go to the extensions folder and delete the specified extension.
