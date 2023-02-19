﻿using System.Reflection;

namespace Vdcrpt.Desktop.Models;

public record ProgramInfo(
    string Name,
    string Version,
    string ItchUrl,
    string GitHubUrl
)
{
    private static string GetVersionFromAssembly()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionString = version is not null
            ? $"{version.Major:00}.{version.Minor:00}.{version.Build:00}"
            : "UNKNOWN";
#if DEBUG
        versionString += " (DEBUG)";
#endif

        return $"{versionString}";
    }

    public static readonly ProgramInfo Default = new(
        Name: "vdcrpt",
        Version: GetVersionFromAssembly(),
        ItchUrl: "https://branchpanic.itch.io/vdcrpt",
        GitHubUrl: "https://github.com/branchpanic/vdcrpt"
    );
}
