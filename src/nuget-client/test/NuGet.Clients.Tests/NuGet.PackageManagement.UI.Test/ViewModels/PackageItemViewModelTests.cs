// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Moq;
using NuGet.PackageManagement.UI.Models.Package;
using NuGet.PackageManagement.UI.Test.Models.Package;
using NuGet.PackageManagement.UI.ViewModels;
using NuGet.PackageManagement.VisualStudio;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Test.Utility;
using NuGet.Versioning;
using NuGet.VisualStudio.Internal.Contracts;
using NuGet.VisualStudio.Telemetry;
using Test.Utility.Threading;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.PackageManagement.UI.Test.ViewModels
{
    [Collection(MockedVS.Collection)]
    public class PackageItemViewModelTests : IClassFixture<LocalPackageSearchMetadataFixture>, IClassFixture<DispatcherThreadFixture>
    {
        private readonly LocalPackageSearchMetadataFixture _testData;
        private readonly PackageItemViewModel _testInstance;
        private readonly ITestOutputHelper _output;
        private readonly INuGetPackageFileService _packageFileService;
        private Mock<IServiceBroker> _serviceBroker = new Mock<IServiceBroker>();
        private Mock<INuGetTelemetryProvider> _telemetryProvider = new Mock<INuGetTelemetryProvider>(MockBehavior.Strict);
        private Mock<INuGetSearchService> _searchService = new Mock<INuGetSearchService>();

        public PackageItemViewModelTests(
            GlobalServiceProvider globalServiceProvider,
            ITestOutputHelper output,
            LocalPackageSearchMetadataFixture testData)
        {
            globalServiceProvider.Reset();
            _serviceBroker.Setup(
#pragma warning disable ISB001 // Dispose of proxies
                x => x.GetProxyAsync<INuGetPackageFileService>(
                It.Is<ServiceJsonRpcDescriptor>(d => d.Moniker == NuGetServices.PackageFileService.Moniker),
                It.IsAny<ServiceActivationOptions>(),
                It.IsAny<CancellationToken>()))
#pragma warning restore ISB001 // Dispose of proxies
            .Returns(new ValueTask<INuGetPackageFileService>(new NuGetPackageFileService(_serviceBroker.Object, _telemetryProvider.Object)));

            _packageFileService = new NuGetPackageFileService(_serviceBroker.Object, _telemetryProvider.Object);
            _testData = testData;
            var identity = new PackageIdentity("package", new NuGetVersion("1.0.0"));
            var packagePath = _testData.TestData.PackagePath;
            var embeddedCapability = new EmbeddedResourcesCapability(_packageFileService, identity, null);

            var packageModel = PackageModelCreationTestHelper.CreateLocalPackageModel(identity, packagePath, embeddedCapability);

            _testInstance = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);
            _output = output;
        }

        [Fact]
        public void LocalSources_PackagePath_NotNull()
        {
            Assert.NotNull(_testInstance.PackagePath);
        }

        [Fact]
        public async Task IconUrl_WithMalformedUrlScheme_ReturnsDefaultInitiallyAndFinally()
        {
            // Arrange
            var iconUrl = new Uri("httphttphttp://fake.test/image.png");
            var packageModel = CreateLocalPackageModel(iconUrl: iconUrl);
            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

            // Act
            var packageIdentity = new PackageIdentity(packageItemViewModel.Id, packageItemViewModel.Version);
            NuGetPackageFileService.AddIconToCache(packageIdentity, packageItemViewModel.IconUrl);

            // Assert
            // initial result should be fetching and defaultpackageicon
            BitmapSource initialResult = packageItemViewModel.IconBitmap;
            Assert.Same(initialResult, Images.DefaultPackageIcon);

            BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel, addIconToCache: false);
            VerifyImageResult(result, packageItemViewModel.BitmapStatus);
            Assert.Equal(IconBitmapStatus.DefaultIconDueToNullStream, packageItemViewModel.BitmapStatus);
        }

        [Fact]
        public async Task IconUrl_WhenFileNotFound_ReturnsDefault()
        {
            // Arrange
            var iconUrl = new Uri(@"C:\path\to\image.png");
            var packageModel = CreateLocalPackageModel(iconUrl: iconUrl);

            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

            // Act
            BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

            // Assert
            VerifyImageResult(result, packageItemViewModel.BitmapStatus);
            Assert.Equal(IconBitmapStatus.DefaultIconDueToNullStream, packageItemViewModel.BitmapStatus);
        }

        [Fact]
        public async Task IconUrl_RelativeUri_ReturnsDefault()
        {
            // Arrange
            // relative URIs are not supported in viewmodel.
            var iconUrl = new Uri("resources/testpackageicon.png", UriKind.Relative);
            var packageModel = CreateLocalPackageModel(iconUrl: iconUrl);

            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

            // Act
            BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

            // Assert
            VerifyImageResult(result, packageItemViewModel.BitmapStatus);
            Assert.Equal(IconBitmapStatus.DefaultIconDueToRelativeUri, packageItemViewModel.BitmapStatus);
        }

        [Fact]
        public async Task IconUrl_WithLocalPathAndColorProfile_LoadsImage()
        {
            await ReadIconFromEmbeddedResourceAsync("grayicc.png");
        }

        [Fact]
        public async Task IconUrl_JpegFullReading_LoadsImage()
        {
            await ReadIconFromEmbeddedResourceAsync("customMetadata.jpeg");
        }

        [Fact]
        public async Task IconUrl_WithValidImageUrl_FailsDownloadsImage_ReturnsDefault()
        {
            // Arrange
            var iconUrl = new Uri("http://fake.test/image.png");

            var packageModel = CreateLocalPackageModel(iconUrl: iconUrl);
            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

            // Act
            BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

            // Assert
            VerifyImageResult(result, packageItemViewModel.BitmapStatus);
            Assert.Equal(IconBitmapStatus.DefaultIconDueToNullStream, packageItemViewModel.BitmapStatus);
        }

        [LocalOnlyTheory]
        [InlineData("icon.png", "icon.png", "icon.png", "")]
        [InlineData("folder/icon.png", "folder\\icon.png", "folder/icon.png", "folder")]
        [InlineData("folder\\icon.png", "folder\\icon.png", "folder\\icon.png", "folder")]
        public async Task IconUrl_EmbeddedIcon_HappyPath_LoadsImage(
            string iconElement,
            string iconFileLocation,
            string fileSourceElement,
            string fileTargetElement)
        {
            using (var testDir = TestDirectory.Create())
            {
                // Arrange
                // Create decoy nuget package
                var zipPath = Path.Combine(testDir.Path, "file.nupkg");
                CreateDummyPackage(
                    zipPath: zipPath,
                    iconName: iconElement,
                    iconFile: iconFileLocation,
                    iconFileSourceElement: fileSourceElement,
                    iconFileTargetElement: fileTargetElement,
                    isRealImage: true);

                // prepare test
                var builder = new UriBuilder(new Uri(zipPath, UriKind.Absolute))
                {
                    Fragment = iconElement
                };

                var packageModel = CreateLocalPackageModel(iconUrl: builder.Uri);
                var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

                _output.WriteLine($"ZipPath {zipPath}");
                _output.WriteLine($"File Exists {File.Exists(zipPath)}");
                _output.WriteLine($"Url {builder.Uri}");

                // Act
                BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

                // Assert
                _output.WriteLine($"result {result}");
                Assert.Equal(IconBitmapStatus.FetchedIcon, packageItemViewModel.BitmapStatus);
                VerifyImageResult(result, packageItemViewModel.BitmapStatus);
            }
        }

        [Fact]
        public async Task IconUrl_FileUri_LoadsImage()
        {
            using (var testDir = TestDirectory.Create())
            {
                // Arrange
                var imagePath = Path.Combine(testDir, "image.png");
                CreateNoisePngImage(path: imagePath);

                var packageModel = CreateLocalPackageModel(iconUrl: new Uri(imagePath, UriKind.Absolute));
                var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

                // Act
                BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

                // Assert
                VerifyImageResult(result, packageItemViewModel.BitmapStatus);
                Assert.Equal(IconBitmapStatus.FetchedIcon, packageItemViewModel.BitmapStatus);
            }
        }

        [Theory]
        [InlineData("/")]
        [InlineData(@"\")]
        public async Task IconUrl_EmbeddedIcon_RelativeParentPath_ReturnsDefault(string separator)
        {
            using (var testDir = TestDirectory.Create())
            {
                // Arrange
                // Create decoy nuget package
                var zipPath = Path.Combine(testDir.Path, "file.nupkg");
                CreateDummyPackage(zipPath);

                // prepare test
                var builder = new UriBuilder(new Uri(zipPath, UriKind.Absolute))
                {
                    Fragment = $"..{separator}icon.png"
                };

                var packageModel = CreateLocalPackageModel(iconUrl: builder.Uri);
                var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

                // Act
                BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

                // Assert
                VerifyImageResult(result, packageItemViewModel.BitmapStatus);
                Assert.Equal(IconBitmapStatus.DefaultIconDueToNullStream, packageItemViewModel.BitmapStatus);
            }
        }

        [Theory]
        [MemberData(nameof(EmbeddedTestData))]
        public void IsEmbeddedIconUri_Tests(Uri testUri, bool expectedResult)
        {
            var result = NuGetPackageFileService.IsEmbeddedUri(testUri);
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> EmbeddedTestData()
        {
            var baseUri = new Uri(@"C:\path\to\package");
            var builder1 = new UriBuilder(baseUri)
            {
                Fragment = "    " // UriBuilder trims the string
            };
            var builder2 = new UriBuilder(baseUri)
            {
                Fragment = "icon.png"
            };
            var builder3 = new UriBuilder(baseUri)
            {
                Fragment = @"..\icon.png"
            };
            var builder4 = new UriBuilder(baseUri)
            {
                Fragment = string.Empty // implies that there's a Tag, but no value
            };
            var builder5 = new UriBuilder(baseUri)
            {
                Query = "aParam"
            };
            return new List<object[]>
            {
                new object[]{ builder1.Uri, false },
                new object[]{ builder2.Uri, true },
                new object[]{ builder3.Uri, true },
                new object[]{ builder4.Uri, false },
                new object[]{ builder5.Uri, false },
                new object[]{ new Uri("https://sample.test/"), false },
                new object[]{ baseUri, false },
                new object[]{ new Uri("https://another.test/#"), false },
                new object[]{ new Uri("https://complimentary.test/#anchor"), false },
                new object[]{ new Uri("https://complimentary.test/?param"), false },
                new object[]{ new Uri("relative/path", UriKind.Relative), false },
            };
        }

        [Fact]
        public async Task UpdateTransitivePackageStatus_WhenGivenInstalledVersion_SetsLatestVersionEqualToInstalledVersionAsync()
        {
            await _testInstance.UpdateTransitivePackageStatusAsync();
            Assert.Equal(_testInstance.LatestVersion, _testInstance.InstalledVersion);
        }

        /// <summary>
        /// Tests the final bitmap returned by the view model, by waiting for the BitmapStatus to be "complete".
        /// </summary>
        private static async Task<BitmapSource> GetFinalIconBitmapAsync(PackageItemViewModel packageItemViewModel, bool addIconToCache = true)
        {
            if (addIconToCache)
            {
                var packageIdentity = new PackageIdentity(packageItemViewModel.Id, packageItemViewModel.Version);
                NuGetPackageFileService.AddIconToCache(packageIdentity, packageItemViewModel.IconUrl);
            }

            BitmapSource result = packageItemViewModel.IconBitmap;
            var millisecondsToWait = 200000;
            while (!IconBitmapStatusUtility.GetIsCompleted(packageItemViewModel.BitmapStatus) && millisecondsToWait >= 0)
            {
                await Task.Delay(250);
                millisecondsToWait -= 250;
            }

            result = packageItemViewModel.IconBitmap;
            return result;
        }

        [Theory]
        [InlineData("icon.jpg", "icon.jpg", "icon.jpg", "")]
        [InlineData("icon2.jpg", "icon2.jpg", "icon2.jpg", "")]
        public async Task IconUrl_EmbeddedIcon_NotAnIcon_ReturnsDefault(
            string iconElement,
            string iconFileLocation,
            string fileSourceElement,
            string fileTargetElement)
        {
            using (var testDir = TestDirectory.Create())
            {
                // Arrange
                // Create decoy nuget package
                var zipPath = Path.Combine(testDir.Path, "file.nupkg");
                CreateDummyPackage(
                    zipPath: zipPath,
                    iconName: iconElement,
                    iconFile: iconFileLocation,
                    iconFileSourceElement: fileSourceElement,
                    iconFileTargetElement: fileTargetElement,
                    isRealImage: false);

                // prepare test
                var builder = new UriBuilder(new Uri(zipPath, UriKind.Absolute))
                {
                    Fragment = iconElement
                };

                var packageModel = CreateLocalPackageModel(packagePath: zipPath, iconUrl: builder.Uri);
                var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

                _output.WriteLine($"ZipPath {zipPath}");
                _output.WriteLine($"File Exists {File.Exists(zipPath)}");
                _output.WriteLine($"Url {builder.Uri}");

                // Act
                BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

                VerifyImageResult(result, packageItemViewModel.BitmapStatus);

                _output.WriteLine($"result {result}");
                var resultFormat = result != null ? result.Format.ToString() : "";
                _output.WriteLine($"Pixel format: {resultFormat}");

                // Assert
                Assert.Equal(IconBitmapStatus.DefaultIconDueToDecodingError, packageItemViewModel.BitmapStatus);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ByOwnerOrAuthor_WhenKnownOwnerViewModelsIsNull_AndAuthorIsNullOrWhitespace_IsNull(string emptyAuthor)
        {
            // Arrange
            var packageModel = CreateLocalPackageModel(authors: emptyAuthor, owners: ["owner1, owner2"]);

            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

            // Act
            string byOwnerOrAuthor = packageItemViewModel.ByOwnerOrAuthor;

            // Assert
            byOwnerOrAuthor.Should().BeNull();
        }

        [Fact]
        public void ByOwnerOrAuthor_WhenKnownOwnerViewModelsIsNull_OnlyContainsAuthor()
        {
            // Arrange
            var packageModel = CreateLocalPackageModel(authors: "author1", owners: ["owner1, owner2"]);
            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

            // Act
            string byOwnerOrAuthor = packageItemViewModel.ByOwnerOrAuthor;

            // Assert
            byOwnerOrAuthor.Should()
                .NotBeNull()
                .And.Contain("author1")
                .And.NotContain("owner1")
                .And.NotContain("owner2");
        }

        [Fact]
        public void ByOwnerOrAuthor_WhenKnownOwnerViewModelsIsEmpty_IsEmpty()
        {
            // Arrange
            var packageModel = CreateLocalPackageModel(authors: "author1", owners: ["owner1, owner2"]);

            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel)
            {
                KnownOwnerViewModels = ImmutableList<KnownOwnerViewModel>.Empty,
            };

            // Act
            string byOwnerOrAuthor = packageItemViewModel.ByOwnerOrAuthor;

            // Assert
            byOwnerOrAuthor.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void ByOwnerOrAuthor_WhenOwnerIsEmpty_IsEmpty()
        {
            // Arrange
            var packageModel = CreateLocalPackageModel(authors: "author1", owners: [string.Empty]);

            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel)
            {
                KnownOwnerViewModels = new List<KnownOwnerViewModel>()
                {
                    new KnownOwnerViewModel(new KnownOwner("owner1", new Uri("https://nuget.test/profiles/owner1?_src=template"))),
                    new KnownOwnerViewModel(new KnownOwner("owner2", new Uri("https://nuget.test/profiles/owner2?_src=template"))),
                }
                .ToImmutableList()
            };

            // Act
            string byOwnerOrAuthor = packageItemViewModel.ByOwnerOrAuthor;

            // Assert
            byOwnerOrAuthor.Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void ByOwnerOrAuthor_WhenKnownOwnerViewModelsExist_OnlyContainsOwner()
        {
            // Arrange
            var packageModel = CreateLocalPackageModel(authors: "author1", owners: ["owner1", "owner2"]);
            var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel)
            {
                KnownOwnerViewModels = new List<KnownOwnerViewModel>()
                {
                    new KnownOwnerViewModel(new KnownOwner("owner1", new Uri("https://nuget.test/profiles/owner1?_src=template"))),
                    new KnownOwnerViewModel(new KnownOwner("owner2", new Uri("https://nuget.test/profiles/owner2?_src=template"))),
                }
                .ToImmutableList()
            };

            // Act
            string byOwnerOrAuthor = packageItemViewModel.ByOwnerOrAuthor;

            // Assert
            byOwnerOrAuthor.Should()
                .NotBeNull()
                .And.NotContain("author1")
                .And.Contain("owner1")
                .And.Contain("owner2");
        }

        private LocalPackageModel CreateLocalPackageModel(string packagePath = null, string authors = null, IReadOnlyList<string> owners = null, Uri iconUrl = null)
        {
            var identity = new PackageIdentity("package", new NuGetVersion("1.0.0"));
            var embeddedCapability = new EmbeddedResourcesCapability(_packageFileService, identity, null);

            return PackageModelCreationTestHelper.CreateLocalPackageModel(identity, _testData.TestData.PackagePath, embeddedCapability, authors: authors, ownersList: owners, iconUrl: iconUrl);
        }

        private static void VerifyImageResult(object result, IconBitmapStatus bitmapStatus)
        {
            Assert.NotNull(result);
            Assert.True(result is BitmapImage || result is CachedBitmap);
            var image = result as BitmapSource;
            Assert.NotNull(image);
            Assert.Equal(PackageItemViewModel.DecodePixelWidth, image.PixelWidth);
            Assert.Equal(PackageItemViewModel.DecodePixelWidth, image.PixelHeight);
        }

        /// <summary>
        /// Creates a NuGet package with .nupkg extension and with a PNG image named "icon.png"
        /// </summary>
        /// <param name="path">Path to NuGet package</param>
        /// <param name="iconName">Icon filename with .png extension</param>
        private static void CreateDummyPackage(
            string zipPath,
            string iconName = "icon.png",
            string iconFile = "icon.png",
            string iconFileSourceElement = "icon.png",
            string iconFileTargetElement = "",
            bool isRealImage = true)
        {
            var dir = Path.GetDirectoryName(zipPath);
            var holdDir = "pkg";
            var folderPath = Path.Combine(dir, holdDir);

            // base dir
            Directory.CreateDirectory(folderPath);

            // create nuspec
            var nuspec = NuspecBuilder.Create()
                .WithIcon(iconName)
                .WithFile(iconFileSourceElement, iconFileTargetElement);

            // create png image
            var iconPath = Path.Combine(folderPath, iconFile);
            var iconDir = Path.GetDirectoryName(iconPath);
            Directory.CreateDirectory(iconDir);

            if (isRealImage)
            {
                CreateNoisePngImage(iconPath);
            }
            else
            {
                File.WriteAllText(iconPath, "I am an image");
            }

            // Create nuget package
            using (var nuspecStream = new MemoryStream())
            using (FileStream nupkgStream = File.Create(zipPath))
            using (var writer = new StreamWriter(nuspecStream))
            {
                nuspec.Write(writer);
                writer.Flush();
                nuspecStream.Position = 0;
                var pkgBuilder = new PackageBuilder(stream: nuspecStream, basePath: folderPath);
                pkgBuilder.Save(nupkgStream);
            }
        }

        /// <summary>
        /// Creates a PNG image with random pixels
        /// </summary>
        /// <param name="path">Filename in which the image is created</param>
        /// <param name="w">Image width in pixels</param>
        /// <param name="h">Image height in pixels</param>
        /// <param name="dpiX">Horizontal Dots (pixels) Per Inch in the image</param>
        /// <param name="dpiY">Vertical Dots (pixels) Per Inch in the image</param>
        private static void CreateNoisePngImage(string path, int w = 128, int h = 128, int dpiX = 96, int dpiY = 96)
        {
            // Create PNG image with noise
            var fmt = PixelFormats.Bgr32;

            // a row of pixels
            var stride = w * fmt.BitsPerPixel;
            var data = new byte[stride * h];

            // Random pixels
            var rnd = new Random();
            rnd.NextBytes(data);

            var bitmap = BitmapSource.Create(w, h,
                dpiX, dpiY,
                fmt,
                null, data, stride);

            BitmapEncoder encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var fs = File.OpenWrite(path))
            {
                encoder.Save(fs);
            }
        }

        /// <summary>
        /// Reads an Icon from test assembly resource
        /// </summary>
        /// <param name="imageFile">Relative path to the resource in test assembly</param>
        /// <returns>An awitable Task</returns>
        private async Task ReadIconFromEmbeddedResourceAsync(string imageFile)
        {
            // Prepare
            using (var testDir = TestDirectory.Create())
            {
                byte[] bytes;
                Assembly testAssembly = typeof(PackageItemViewModelTests).Assembly;

                using (Stream sourceStream = testAssembly.GetManifestResourceStream($"NuGet.PackageManagement.UI.Test.Resources.{imageFile}"))
                using (var memoryStream = new MemoryStream())
                {
                    sourceStream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }

                var imageFilePath = Path.Combine(testDir, imageFile);
                File.WriteAllBytes(imageFilePath, bytes);

                var packageModel = CreateLocalPackageModel(iconUrl: new Uri(imageFilePath, UriKind.Absolute));

                var packageItemViewModel = new PackageItemViewModel(_searchService.Object, packageModel: packageModel);

                // Act
                BitmapSource result = await GetFinalIconBitmapAsync(packageItemViewModel);

                // Assert
                VerifyImageResult(result, packageItemViewModel.BitmapStatus);
                Assert.Equal(IconBitmapStatus.FetchedIcon, packageItemViewModel.BitmapStatus);
            }
        }
    }
}
