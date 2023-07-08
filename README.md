# Dynamic assembly loading and unloading example
This is an example of a project utilizing dynamic loading and unloading of .NET assemblies. Published assemblies should contain their dependencies, then they can be loaded through the ExtensionLoadContext.

As long as the ServiceProvider that gets build by the extension gets disposed and everything is cleaned up, the load context should be unloadable / collectible.

# References
- https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
- https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability