using FluentAssertions;
using Kasi_Room_Network___KRN.Controllers;
using Kasi_Room_Network___KRN.Services;
using KasiRoomNetwork.Common.Models;
using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KasiRoomNetwork.Test.Controllers
{
    public class PropertyControllerTests
    {
        [Fact]
        public async Task CreateProperty_Should_Redirect_To_Profile_When_Profile_Is_Incomplete()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            profileRepoMock
                .Setup(x => x.IsComplete("user-123"))
                .ReturnsAsync(false);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.CreateProperty();

            // Assert

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("MyProfile");
            redirect.ControllerName.Should().Be("Profile");
        }
        [Fact]
        public async Task CreateProperty_Should_Return_View_When_Profile_Is_Complete()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            profileRepoMock
                .Setup(x => x.IsComplete("user-123"))
                .ReturnsAsync(true);

            amenityRepoMock
                .Setup(x => x.GetAllAmenities())
                .ReturnsAsync(new List<AmenityModel>());

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.CreateProperty();

            // Assert

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;

            viewResult.Model.Should().BeOfType<CreatePropertyViewModel>();
        }
        [Fact]
        public async Task CreateProperty_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();
            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            // Act

            var result = await controller.CreateProperty();

            // Assert

            result.Should().BeOfType<ChallengeResult>();
        }
        [Fact]
        public async Task CreateProperty_Post_Should_Return_View_When_ModelState_Is_Invalid()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();
            amenityRepoMock
                .Setup(x => x.GetAllAmenities())
                .ReturnsAsync(new List<AmenityModel>());

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            controller.ModelState.AddModelError(
                "PropertyType",
                "Required");

            var model = new CreatePropertyViewModel();

            // Act

            var result = await controller.CreateProperty(model);

            // Assert

            result.Should().BeOfType<ViewResult>();

            propertyRepoMock.Verify(
                x => x.CreateProperty(
                    It.IsAny<CreatePropertyViewModel>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task CreateProperty_Post_Should_Redirect_To_Profile_When_Profile_Is_Incomplete()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();
            profileRepoMock
                .Setup(x => x.IsComplete("user-123"))
                .ReturnsAsync(false);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            var model = new CreatePropertyViewModel
            {
                PropertyType = "House",
                Province = "North West",
                City = "Rustenburg",
                Street = "123 Main Street"
            };

            // Act

            var result = await controller.CreateProperty(model);

            // Assert

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("MyProfile");
            redirect.ControllerName.Should().Be("Profile");

            propertyRepoMock.Verify(
                x => x.CreateProperty(
                    It.IsAny<CreatePropertyViewModel>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task CreateProperty_Post_Should_Create_Property_And_Redirect_To_AddPropertyPhotos()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();
            profileRepoMock
                .Setup(x => x.IsComplete("user-123"))
                .ReturnsAsync(true);

            propertyRepoMock
                .Setup(x => x.CreateProperty(
                    It.IsAny<CreatePropertyViewModel>(),
                    "user-123"))
                .ReturnsAsync(100);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            var model = new CreatePropertyViewModel
            {
                PropertyType = "House",
                PropertyName = "Test Property",
                Province = "North West",
                City = "Rustenburg",
                Street = "123 Main Street",
                SelectedAmenityIds = new List<int> { 1, 2 }
            };

            // Act

            var result = await controller.CreateProperty(model);

            // Assert

            propertyRepoMock.Verify(
                x => x.CreateProperty(model, "user-123"),
                Times.Once);

            amenityRepoMock.Verify(
                x => x.AddPropertyAmenity(100, 1),
                Times.Once);

            amenityRepoMock.Verify(
                x => x.AddPropertyAmenity(100, 2),
                Times.Once);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("AddPropertyPhotos");

            redirect.RouteValues!["propertyId"]
                .Should()
                .Be(100);
        }
        [Fact]
        public async Task DeleteProperty_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            // Act

            var result = await controller.DeleteProperty(10);

            // Assert

            result.Should().BeOfType<ChallengeResult>();

            propertyRepoMock.Verify(
                x => x.DeletePropertyAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task DeleteProperty_Should_Delete_Property_And_Redirect()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.DeleteProperty(10);

            // Assert

            propertyRepoMock.Verify(
                x => x.DeletePropertyAsync(
                    10,
                    "user-123"),
                Times.Once);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("MyProperties");
        }
        [Fact]
        public async Task EditProperty_Get_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            // Act

            var result = await controller.EditProperty(1);

            // Assert

            result.Should().BeOfType<ChallengeResult>();
        }
        [Fact]
        public async Task EditProperty_Get_Should_Return_NotFound_When_Property_Does_Not_Exist()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyForEditAsync(1, "user-123"))
                .ReturnsAsync((EditPropertyViewModel?)null);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.EditProperty(1);

            // Assert

            result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public async Task EditProperty_Get_Should_Return_View_When_Property_Exists()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var property = new EditPropertyViewModel
            {
                PropertyId = 1,
                PropertyName = "Test Property"
            };

            propertyRepoMock
                .Setup(x => x.GetPropertyForEditAsync(1, "user-123"))
                .ReturnsAsync(property);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.EditProperty(1);

            // Assert

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;

            viewResult.Model.Should().Be(property);
        }
        [Fact]
        public async Task EditProperty_Post_Should_Return_View_When_ModelState_Is_Invalid()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            controller.ModelState.AddModelError("PropertyName", "Required");

            var model = new EditPropertyViewModel();

            // Act

            var result = await controller.EditProperty(model);

            // Assert

            result.Should().BeOfType<ViewResult>();

            propertyRepoMock.Verify(
                x => x.UpdatePropertyAsync(
                    It.IsAny<EditPropertyViewModel>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task EditProperty_Post_Should_Update_Property_And_Redirect()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            var model = new EditPropertyViewModel
            {
                PropertyId = 1,
                PropertyName = "Updated Property"
            };

            // Act

            var result = await controller.EditProperty(model);

            // Assert

            propertyRepoMock.Verify(
                x => x.UpdatePropertyAsync(model, "user-123"),
                Times.Once);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("PropertyDetails");
            redirect.RouteValues!["propertyId"].Should().Be(1);
        }
        [Fact]
        public async Task AddPropertyPhotos_Get_Should_Return_View()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyPhotoCount(1))
                .ReturnsAsync(3);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.AddPropertyPhotos(1);

            // Assert

            result.Should().BeOfType<ViewResult>();

            propertyRepoMock.Verify(
                x => x.GetPropertyPhotoCount(1),
                Times.Once);
        }
        [Fact]
        public async Task AddPropertyPhotos_Post_Should_Return_View_When_Photo_Is_Null()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyPhotoCount(1))
                .ReturnsAsync(0);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.AddPropertyPhotos(
                1,
                null!,
                false);

            // Assert

            result.Should().BeOfType<ViewResult>();

            propertyRepoMock.Verify(
                x => x.AddPropertyPhoto(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task AddPropertyPhotos_Post_Should_Return_View_When_File_Extension_Is_Invalid()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyPhotoCount(1))
                .ReturnsAsync(0);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "photo",
                "document.pdf");

            // Act

            var result = await controller.AddPropertyPhotos(
                1,
                file,
                false);

            // Assert

            result.Should().BeOfType<ViewResult>();

            propertyRepoMock.Verify(
                x => x.AddPropertyPhoto(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task AddPropertyPhotos_Post_Should_Return_View_When_File_Is_Too_Large()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyPhotoCount(1))
                .ReturnsAsync(0);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            var stream = new MemoryStream(new byte[3 * 1024 * 1024]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "photo",
                "test.jpg");

            // Act

            var result = await controller.AddPropertyPhotos(
                1,
                file,
                false);

            // Assert

            result.Should().BeOfType<ViewResult>();

            propertyRepoMock.Verify(
                x => x.AddPropertyPhoto(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task AddPropertyPhotos_Post_Should_Save_Photo_And_Redirect()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            var stream = new MemoryStream(new byte[100]);

            IFormFile file = new FormFile(
                stream,
                0,
                stream.Length,
                "photo",
                "test.jpg");

            // Act

            var result = await controller.AddPropertyPhotos(
                1,
                file,
                false);

            // Assert

            propertyRepoMock.Verify(
                x => x.AddPropertyPhoto(
                    1,
                    It.IsAny<string>(),
                    false,
                    It.IsAny<string>()),
                Times.Once);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("AddPropertyPhotos");
            redirect.RouteValues!["propertyId"].Should().Be(1);
        }
        [Fact]
        public async Task DeletePropertyPhoto_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            // Act

            var result = await controller.DeletePropertyPhoto(1, 10);

            // Assert

            result.Should().BeOfType<ChallengeResult>();
        }
       
        [Fact]
        public async Task DeletePropertyPhoto_Should_Redirect_When_User_Does_Not_Own_Property()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyById(1))
                .ReturnsAsync(new PropertyDetailsViewModel
                {
                    PropertyId = 1,
                    LandlordUserId = "different-user"
                });

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.DeletePropertyPhoto(1, 10);

            // Assert

            result.Should().BeOfType<RedirectToActionResult>();

            propertyRepoMock.Verify(
                x => x.DeletePropertyPhoto(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }
        [Fact]
        public async Task DeletePropertyPhoto_Should_Handle_Repository_Exception()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyById(1))
                .ReturnsAsync(new PropertyDetailsViewModel
                {
                    PropertyId = 1,
                    LandlordUserId = "user-123"
                });

            propertyRepoMock
                .Setup(x => x.DeletePropertyPhoto(10, 1))
                .ThrowsAsync(new Exception("DB Error"));

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.DeletePropertyPhoto(1, 10);

            // Assert

            result.Should().BeOfType<RedirectToActionResult>();
        }
        [Fact]
        public async Task SetPrimaryPropertyPhoto_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            // Act

            var result = await controller.SetPrimaryPropertyPhoto(1, 10);

            // Assert

            result.Should().BeOfType<ChallengeResult>();
        }
        [Fact]
        public async Task SetPrimaryPropertyPhoto_Should_Redirect_When_User_Does_Not_Own_Property()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyById(1))
                .ReturnsAsync(new PropertyDetailsViewModel
                {
                    PropertyId = 1,
                    LandlordUserId = "different-user"
                });

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.SetPrimaryPropertyPhoto(1, 10);

            // Assert

            result.Should().BeOfType<RedirectToActionResult>();

            propertyRepoMock.Verify(
                x => x.SetPrimaryPropertyPhoto(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }
        [Fact]
        public async Task SetPrimaryPropertyPhoto_Should_Update_Primary_Photo_And_Redirect()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyById(1))
                .ReturnsAsync(new PropertyDetailsViewModel
                {
                    PropertyId = 1,
                    LandlordUserId = "user-123"
                });

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.SetPrimaryPropertyPhoto(1, 10);

            // Assert

            propertyRepoMock.Verify(
                x => x.SetPrimaryPropertyPhoto(1, 10),
                Times.Once);

            result.Should().BeOfType<RedirectToActionResult>();
        }
        [Fact]
        public async Task SetPrimaryPropertyPhoto_Should_Handle_Repository_Exception()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            propertyRepoMock
                .Setup(x => x.GetPropertyById(1))
                .ReturnsAsync(new PropertyDetailsViewModel
                {
                    PropertyId = 1,
                    LandlordUserId = "user-123"
                });

            propertyRepoMock
                .Setup(x => x.SetPrimaryPropertyPhoto(1, 10))
                .ThrowsAsync(new Exception("DB Error"));

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.SetPrimaryPropertyPhoto(1, 10);

            // Assert

            result.Should().BeOfType<RedirectToActionResult>();
        }
        [Fact]
        public async Task EditPropertyAmenities_Get_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            // Act

            var result = await controller.EditPropertyAmenities(1);

            // Assert

            result.Should().BeOfType<ChallengeResult>();
        }

        [Fact]
        public async Task EditPropertyAmenities_Get_Should_Return_NotFound_When_Property_Is_Not_Found()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            amenityRepoMock
                .Setup(x => x.GetPropertyAmenitiesForEditAsync(1, "user-123"))
                .ReturnsAsync((EditPropertyAmenitiesViewModel?)null);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.EditPropertyAmenities(1);

            // Assert

            result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public async Task EditPropertyAmenities_Get_Should_Return_View_When_Property_Exists()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var model = new EditPropertyAmenitiesViewModel
            {
                PropertyId = 1
            };

            amenityRepoMock
                .Setup(x => x.GetPropertyAmenitiesForEditAsync(1, "user-123"))
                .ReturnsAsync(model);

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.EditPropertyAmenities(1);

            // Assert

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;

            viewResult.Model.Should().Be(model);
        }
        [Fact]
        public async Task EditPropertyAmenities_Post_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            var model = new EditPropertyAmenitiesViewModel
            {
                PropertyId = 1,
                Amenities = new List<AmenitySelectionViewModel>()
            };

            // Act

            var result = await controller.EditPropertyAmenities(model);

            // Assert

            result.Should().BeOfType<ChallengeResult>();
        }
        [Fact]
        public async Task EditPropertyAmenities_Post_Should_Update_Amenities_And_Redirect()
        {
            // Arrange

            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var landlordRepoMock = new Mock<ILandlordRepository>();
            var amenityRepoMock = new Mock<IAmenityRepository>();
            var photoStorageMock = new Mock<IPhotoStorageService>();

            var controller = new PropertyController(
                propertyRepoMock.Object,
                profileRepoMock.Object,
                landlordRepoMock.Object,
                amenityRepoMock.Object,
                photoStorageMock.Object);

            ControllerTestHelper.SetupController(controller);

            var model = new EditPropertyAmenitiesViewModel
            {
                PropertyId = 1,
                Amenities = new List<AmenitySelectionViewModel>
                {
                    new()
                    {
                        AmenityId = 1,
                        IsSelected = true
                    },
                    new()
                    {
                        AmenityId = 2,
                        IsSelected = false
                    },
                    new()
                    {
                        AmenityId = 3,
                        IsSelected = true
                    }
                }
            };

            // Act

            var result = await controller.EditPropertyAmenities(model);

            // Assert

            amenityRepoMock.Verify(
                x => x.UpdatePropertyAmenitiesAsync(
                    1,
                    It.Is<List<int>>(ids =>
                        ids.Count == 2 &&
                        ids.Contains(1) &&
                        ids.Contains(3)),
                    "user-123"),
                Times.Once);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("PropertyDetails");
            redirect.RouteValues!["propertyId"].Should().Be(1);
        }
    }
}
