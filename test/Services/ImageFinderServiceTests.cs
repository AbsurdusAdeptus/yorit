using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yorit.Api.BackgroundServices;
using Yorit.Api.Data;
using Yorit.Api.Data.Models;
using Yorit.Api.Services;

namespace Yorit.Api.Test.Services
{
    public class ImageFinderServiceTests
    {
        private readonly Mock<IFileSystemService> _fileSystemMock;
        private readonly ServiceProvider _serviceProvider;
        private readonly AppDbContext _dbContext;

        public ImageFinderServiceTests()
        {
            _fileSystemMock = new Mock<IFileSystemService>();

            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));
            services.AddLogging();
            services.AddSingleton<IFileSystemService>(_fileSystemMock.Object);

            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        }

        [Fact]
        public async Task ScanForNewImages_AddsNewImagesToDatabase()
        {
            // Arrange
            var testDirectory = "/test/images";
            var testFiles = new List<string> { "/test/images/img1.png", "/test/images/img2.jpg" };

            // Convert it to an absolute path (just like ImageFinderService does)
            var absoluteTestDirectory = Path.GetFullPath(testDirectory);
            var absoluteTestFiles = testFiles.Select(f => Path.GetFullPath(f)).ToList();
            var fileContents = new Dictionary<string, string>
            {
                { absoluteTestFiles[0], "mock file data 1" },
                { absoluteTestFiles[1], "mock file data 2" }
            };

            _fileSystemMock.Setup(fs => fs.DirectoryExists(absoluteTestDirectory)).Returns(true);
            _fileSystemMock.Setup(fs => fs.EnumerateFiles(absoluteTestDirectory, "*.*", SearchOption.AllDirectories))
                           .Returns(absoluteTestFiles);
            foreach (var file in fileContents)
            {
                _fileSystemMock.Setup(fs => fs.OpenRead(file.Key))
                               .Returns(new MemoryStream(Encoding.UTF8.GetBytes(file.Value)));
            }

            var imageSource = new ImageSource { Id = Guid.NewGuid(), Path = testDirectory, DisplayName = "Test Source" };
            await _dbContext.ImageSources.AddAsync(imageSource);
            await _dbContext.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<ImageFinderService>>();
            var service = new ImageFinderService(_serviceProvider, loggerMock.Object, _fileSystemMock.Object);

            // Act
            await service.InvokePrivateMethodAsync("ScanForNewImages");

            // Assert
            var images = await _dbContext.Images.ToListAsync();
            Assert.Equal(2, images.Count);
        }
    }

    public static class TestHelper
    {
        public static async Task InvokePrivateMethodAsync<T>(this T instance, string methodName, params object[] parameters)
        {
            var method = typeof(T).GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null)
                throw new ArgumentException($"Method '{methodName}' not found in {typeof(T).Name}");

            var result = method.Invoke(instance, parameters);
            if (result is Task task)
                await task;
        }
    }
}
