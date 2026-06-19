namespace backend.Services;

public interface INumberSequenceService
{
    Task<string> GenerateNextAsync(string sequenceCode);
    Task<string> PreviewAsync(Guid numberSequenceId);
    Task ResetAsync(Guid numberSequenceId);
}
