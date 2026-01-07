namespace Fermetta.Services
{
    public interface IProductAssistantService
    {
        Task<string> AskAsync(int productId, string question);
    }
}
