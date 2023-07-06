using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace iMap.Helper;

public class UploadHelper : IUploadHelper
{
    private IHostingEnvironment _hostEnvironment;

    public UploadHelper(IHostingEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task<string> UploadImage(IFormFile file)
    {
        string uniqueFileName = null;
        if (file != null)
        {
            string uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/uploaded-images");
            uniqueFileName = Guid.NewGuid().ToString() + "-" + file.FileName;
            string filePath = Path.Combine(uploadFolder, uniqueFileName);
            // Boyut sýnýrý kontrolleri
            using (var image = Image.Load(file.OpenReadStream()))
            {
                if (image.Width > 50 || image.Height > 50)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(50, 50),
                        Mode = ResizeMode.Max
                    }));
                    await image.SaveAsync(filePath);
                }
                else
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
            return "images/uploaded-images/" + uniqueFileName;
        }
        return "";
    }
}