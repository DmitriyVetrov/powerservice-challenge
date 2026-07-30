using System.ComponentModel.DataAnnotations;

namespace PowerPosition.Web.Configuration;

/// <summary>
/// Binds the "Extract" configuration section from appsettings.json, the command line
/// (e.g. --Extract:OutputPath ./reports) or the environment.
/// </summary>
/// <remarks>
/// Deliberately a separate type from the worker's <c>ExtractOptions</c>, even though both bind
/// the same section name: sharing it would mean referencing PowerPosition.Worker — and with it
/// the Axpo PowerService assembly — from the web app, for one string property. The web app only
/// ever reads what the worker produced, so it only needs the folder.
/// </remarks>
public sealed class ReportOptions
{
    public const string SectionName = "Extract";

    /// <summary>Folder the worker's CSV files are read from. Relative paths resolve from the current directory.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Extract:OutputPath must be a non-empty folder path.")]
    public string OutputPath { get; set; } = string.Empty;
}
