using Dermatologiya.Server.AllDTOs;
using Dermatologiya.Server.Models;
using Dermatologiya.Server.RepositoriesAll.ImageRep;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Dermatologiya.Server.Services
{
    public class ImageService : IImageService
    {
        //private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly IImageRepository _imageRepository;
        //public ImageService(IWebHostEnvironment webHostEnvironment, IImageRepository imageRepository)
        //{
        //    _webHostEnvironment = webHostEnvironment;
        //    _imageRepository = imageRepository;
        //}

        private readonly IMinioClient _minioClient;
        private readonly IImageRepository _imageRepository;
        private readonly string _bucketName = "dermatology";
        public ImageService(IImageRepository imageRepository, IMinioClient minioClient)
        {
            _imageRepository = imageRepository;
            _minioClient = minioClient;
        }


        public async Task<ImageResponseDTO> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Fayl bo'sh bo'lishi mumkin emas.");
            }

            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            try
            {
                // Bucket mavjudligini tekshirish
                var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
                if (!bucketExists)
                {
                    // Agar bucket mavjud bo'lmasa, uni yaratish
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
                }

                // Faylni MinIO'ga yuklash
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(uniqueFileName)
                    .WithStreamData(memoryStream)
                    .WithObjectSize(memoryStream.Length)
                    .WithContentType(file.ContentType);
                await _minioClient.PutObjectAsync(putObjectArgs);

                var getPresignedUrl = await _minioClient.PresignedGetObjectAsync(
                                          new PresignedGetObjectArgs()
                                            .WithBucket(_bucketName)
                                            .WithObject(uniqueFileName)
                                            .WithExpiry(3600));
                var image = new ImageHospital
                {
                    ImageName = uniqueFileName,
                    ImagePath = getPresignedUrl,
                    CreateTime = DateTime.UtcNow
                };
                var img2 = await _imageRepository.AddImageHospitalAsync(image);
                return new ImageResponseDTO
                {
                    Id = img2.Id,
                    ImageName = img2.ImageName,
                    ImagePath = getPresignedUrl,
                    CreateTime = img2.CreateTime
                };

            }
            catch (MinioException e)
            {
                throw new InvalidOperationException("Faylni yuklashda xatolik yuz berdi.", e);
            }
            //try
            //{
            //    if (file.Length > 0)
            //    {

            //        string path = _webHostEnvironment.WebRootPath + "\\Img\\";
            //        if (!Directory.Exists(path))
            //        {
            //            Directory.CreateDirectory(path);
            //        }

            //        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            //        string fullPath = Path.Combine(path, uniqueFileName);

            //        var image = new ImageHospital
            //        {
            //            ImageName = file.FileName,
            //            ImagePath = fullPath,
            //            CreateTime = DateTime.UtcNow
            //        };

            //        var img2 = await _imageRepository.AddImageHospitalAsync(image);
            //        using (FileStream fileStream = System.IO.File.Create(path + file.FileName))
            //        {
            //            file.CopyTo(fileStream);
            //            fileStream.Flush();

            //            return new ImageResponseDTO { Id = img2.Id, ImageName = img2.ImageName, ImagePath = img2.ImagePath, CreateTime = img2.CreateTime };

            //        }

            //    }
            //    else
            //    {
            //        return new ImageResponseDTO();
            //    }

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}

        }

        public async Task<IEnumerable<ImageResponseDTO>> GetAll()
        {
            var imageHospitals = await _imageRepository.GetAllImageHospitalAsync();
            var imageResponseDTOs = new List<ImageResponseDTO>();

            foreach (var item in imageHospitals)
            {
                var presignedUrl = await _minioClient.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(item.ImageName)
                        .WithExpiry(3600));

                imageResponseDTOs.Add(new ImageResponseDTO
                {
                    Id = item.Id,
                    ImageName = item.ImageName,
                    ImagePath = presignedUrl,
                    CreateTime = item.CreateTime
                });
            }

            return imageResponseDTOs;
        }

        public async Task<string?> GetImageById(int Id)
        {
            var image = await _imageRepository.GetImageHospitalByIdAsync(Id);

            if (image == null)
            {
                return "Rasmlar mavjud emas!!!";
            }

            try
            {
                var url = await _minioClient.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(image.ImageName)
                        .WithExpiry(3600));

                return url;
            }
            catch (MinioException e)
            {
                return null;
            }
        }

        public async Task<bool> DeleteById(int Id)
        {
            var image = await _imageRepository.GetImageHospitalByIdAsync(Id);
            if (image == null)
            {
                return false;
            }

            try
            {
                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(image.ImageName));
            }
            catch (MinioException e)
            {
                return false;
            }

            var result = await _imageRepository.DeleteImageHospitalAsync(Id);
            return result;
        }
    }
}

