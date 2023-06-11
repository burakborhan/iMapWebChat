namespace iMap.Helper;

public interface IUploadHelper
{
    Task<string> UploadImage(IFormFile file);
}