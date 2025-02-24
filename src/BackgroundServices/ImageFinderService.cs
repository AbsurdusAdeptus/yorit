using Microsoft.EntityFrameworkCore;
using System.IO;
using Yorit.Api.Data;
using Yorit.Api.Data.Models;
using Yorit.Api.Services;

namespace Yorit.Api.BackgroundServices
{
    public class ImageFinderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ImageFinderService> _logger;
        private readonly IFileSystemService _fileSystemService;
        private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(10); // Scan every 10 minutes

        public ImageFinderService(IServiceProvider serviceProvider, ILogger<ImageFinderService> logger, IFileSystemService fileSystemService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _fileSystemService = fileSystemService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ScanForNewImages();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scanning for images.");
                }

                await Task.Delay(_scanInterval, stoppingToken);
            }
        }

        private async Task ScanForNewImages()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var imageSources = await dbContext.ImageSources.ToListAsync();
            foreach (var source in imageSources)
            {
                var fullPath = Path.GetFullPath(source.Path);
                if (!_fileSystemService.DirectoryExists(fullPath))
                {
                    _logger.LogWarning("Skipping missing directory: {Path}", fullPath);
                    continue;
                }

                var files = _fileSystemService.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories)
                                     .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                 f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                 f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                                     .ToList();

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var newImage = new Image
                    {
                        Id = Guid.NewGuid(),
                        FilePath = Path.GetRelativePath(fullPath, file),
                        FileSize = fileInfo.Length,
                        CreatedDate = fileInfo.CreationTimeUtc
                    };

                    dbContext.Images.Add(newImage);
                }

                await dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Image scanning completed.");
        }
    }
}
