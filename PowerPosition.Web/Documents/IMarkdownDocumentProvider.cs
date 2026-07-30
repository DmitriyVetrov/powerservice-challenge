using Microsoft.AspNetCore.Components;

namespace PowerPosition.Web.Documents;

/// <summary>
/// Supplies the authored Markdown documents in <c>/docs</c> as HTML ready to render.
/// </summary>
public interface IMarkdownDocumentProvider
{
    /// <summary>Reads a document by its logical name and renders it to HTML.</summary>
    /// <param name="name">One of the names on <see cref="MarkdownDocument"/>.</param>
    /// <exception cref="InvalidOperationException">No document ships under that name.</exception>
    MarkupString Render(string name);
}

/// <summary>The documents the pages ask for, so a page never spells a name out itself.</summary>
public static class MarkdownDocument
{
    public const string Requirements = "requirements";
    public const string Cv = "cv";
}
