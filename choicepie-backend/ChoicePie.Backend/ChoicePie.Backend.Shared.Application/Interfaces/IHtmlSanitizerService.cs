namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
}
