using System.ComponentModel.DataAnnotations;

namespace PowerPosition.Worker.Configuration;

/// <summary>
/// Binds the "Extract" configuration section from appsettings.json or the command line
/// (e.g. --Extract:IntervalMinutes 5).
/// </summary>
public sealed class ExtractOptions
{
    public const string SectionName = "Extract";

    /// <summary>Minutes between scheduled extracts.</summary>
    [Range(1, 1440, ErrorMessage = "Extract:IntervalMinutes must be between 1 and 1440.")]
    public int IntervalMinutes { get; set; }

    /// <summary>Folder the CSV files are written to.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Extract:OutputPath must be a non-empty folder path.")]
    public string OutputPath { get; set; } = string.Empty;
}
