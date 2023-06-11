using Microsoft.AspNetCore.Http;

namespace iMap.Helpers
{
    public interface IFileValidator
    {
        bool IsValid(IFormFile file);
    }
}
