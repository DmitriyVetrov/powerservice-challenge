namespace PowerPosition.Web.Models;

/// <summary>
/// One CSV report found in the extract folder, as shown in the report list.
/// </summary>
/// <param name="Name">File name only — never a path, so it cannot be used to escape the folder.</param>
/// <param name="ModifiedUtc">Last write time, used to order the list newest-first.</param>
public sealed record ReportFile(string Name, DateTimeOffset ModifiedUtc);
