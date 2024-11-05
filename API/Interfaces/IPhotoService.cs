using System;
using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAync(IFormFile file);
    Task<DeletionResult> DeletePhotoAync(string publicId);

}
