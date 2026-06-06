using FluentAssertions;
using Kasi_Room_Network___KRN.Controllers;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Test.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KasiRoomNetwork.Test.Controllers
{
    public class ListingControllerTests
    {
        [Fact]
        public async Task CreateListing_Get_Should_Return_Challenge_When_User_Is_Missing()
        {
            // Arrange

            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            var controller = new ListingController(
                listingRepoMock.Object,
                profileRepoMock.Object,
                propertyRepoMock.Object);

            ControllerTestHelper.SetupAnonymousController(controller);

            // Act

            var result = await controller.CreateListing(1);

            // Assert

            result.Should().BeOfType<ChallengeResult>();
        }
        [Fact]
        public async Task CreateListing_Get_Should_Redirect_When_Profile_Is_Incomplete()
        {
            // Arrange

            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            profileRepoMock
                .Setup(x => x.IsComplete("user-123"))
                .ReturnsAsync(false);

            var controller = new ListingController(
                listingRepoMock.Object,
                profileRepoMock.Object,
                propertyRepoMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.CreateListing(1);

            // Assert

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("MyProfile");
            redirect.ControllerName.Should().Be("Profile");
        }
        [Fact]
        public async Task CreateListing_Get_Should_Return_View_When_Profile_Is_Complete()
        {
            // Arrange

            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            profileRepoMock
                .Setup(x => x.IsComplete("user-123"))
                .ReturnsAsync(true);

            propertyRepoMock
                .Setup(x => x.GetPropertyById(1))
                .ReturnsAsync(new PropertyDetailsViewModel
                {
                    PropertyId = 1,
                    PropertyName = "Test Property",
                    Suburb = "Tlhabane",
                    City = "Rustenburg"
                });

            var controller = new ListingController(
                listingRepoMock.Object,
                profileRepoMock.Object,
                propertyRepoMock.Object);

            ControllerTestHelper.SetupController(controller);

            // Act

            var result = await controller.CreateListing(1);

            // Assert

            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;

            var model = viewResult.Model as CreateListingViewModel;

            model.Should().NotBeNull();
            model!.PropertyId.Should().Be(1);
            model.PropertyName.Should().Be("Test Property");
        }
        [Fact]
        public async Task CreateListing_Post_Should_Return_View_When_ModelState_Is_Invalid()
        {
            // Arrange

            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            propertyRepoMock
                .Setup(x => x.GetPropertyById(1))
                .ReturnsAsync(new PropertyDetailsViewModel
                {
                    PropertyId = 1,
                    PropertyName = "Test Property",
                    Suburb = "Tlhabane",
                    City = "Rustenburg"
                });

            var controller = new ListingController(
                listingRepoMock.Object,
                profileRepoMock.Object,
                propertyRepoMock.Object);

            ControllerTestHelper.SetupController(controller);

            controller.ModelState.AddModelError("Title", "Required");

            var model = new CreateListingViewModel
            {
                PropertyId = 1
            };

            // Act

            var result = await controller.CreateListing(model);

            // Assert

            result.Should().BeOfType<ViewResult>();

            listingRepoMock.Verify(
                x => x.CreateListing(
                    It.IsAny<CreateListingViewModel>(),
                    It.IsAny<string>()),
                Times.Never);
        }
        [Fact]
        public async Task CreateListing_Post_Should_Create_Listing_And_Redirect()
        {
            // Arrange

            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            profileRepoMock
                .Setup(x => x.IsComplete("user-123"))
                .ReturnsAsync(true);

            listingRepoMock
                .Setup(x => x.CreateListing(
                    It.IsAny<CreateListingViewModel>(),
                    "user-123"))
                .ReturnsAsync(50);

            var controller = new ListingController(
                listingRepoMock.Object,
                profileRepoMock.Object,
                propertyRepoMock.Object);

            ControllerTestHelper.SetupController(controller);

            var model = new CreateListingViewModel
            {
                PropertyId = 1,
                Title = "Room Available"
            };

            // Act

            var result = await controller.CreateListing(model);

            // Assert

            listingRepoMock.Verify(
                x => x.CreateListing(model, "user-123"),
                Times.Once);

            result.Should().BeOfType<RedirectToActionResult>();

            var redirect = (RedirectToActionResult)result;

            redirect.ActionName.Should().Be("AddListingPhotos");
            redirect.RouteValues!["listingId"].Should().Be(50);
        }

        private static ListingDetailsViewModel Listing(int listingId = 10, string landlordUserId = "user-123")
        {
            return new ListingDetailsViewModel
            {
                ListingId = listingId,
                Title = "Room Available",
                Description = "A clean room",
                Price = 2500,
                IsAvailable = true,
                IsVerified = true,
                PropertyVerified = true,
                LandlordUserId = landlordUserId,
                FullName = "Test Landlord",
                PhoneNumber = "123",
                Province = "North West",
                City = "Rustenburg",
                Suburb = "Tlhabane"
            };
        }

        private static IFormFile TestPhoto()
        {
            var bytes = Encoding.UTF8.GetBytes("fake image bytes");
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "photo", "room.jpg");
        }

        [Fact]
        public async Task EditListing_Get_Should_Return_View_For_Owner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock
                .Setup(x => x.GetListingForEdit(10, "user-123"))
                .ReturnsAsync(new EditListingViewModel { ListingId = 10, Title = "Room", Description = "Desc", Price = 1000 });

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.EditListing(10);

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task EditListing_Get_Should_Return_NotFound_When_Listing_Missing()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock
                .Setup(x => x.GetListingForEdit(10, "user-123"))
                .ReturnsAsync((EditListingViewModel?)null);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.EditListing(10);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task EditListing_Post_Should_Save_Changes_For_Owner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var model = new EditListingViewModel { ListingId = 10, PropertyId = 3, Title = "Updated", Description = "Updated description", Price = 1200, IsAvailable = true };

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing());
            listingRepoMock.Setup(x => x.UpdateListing(model, "user-123")).ReturnsAsync(true);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.EditListing(model);

            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be("ListingDetails");
            controller.TempData["SuccessMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task EditListing_Post_Should_Return_Forbid_For_NonOwner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var model = new EditListingViewModel { ListingId = 10, PropertyId = 3, Title = "Updated", Description = "Updated description", Price = 1200 };

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing(10, "other-user"));

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.EditListing(model);

            result.Should().BeOfType<ForbidResult>();
            listingRepoMock.Verify(x => x.UpdateListing(It.IsAny<EditListingViewModel>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditListing_Post_Should_Return_NotFound_When_Listing_Missing()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var model = new EditListingViewModel { ListingId = 10, PropertyId = 3, Title = "Updated", Description = "Updated description", Price = 1200 };

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync((ListingDetailsViewModel?)null);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.EditListing(model);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task EditListing_Post_Should_Show_Error_When_Update_Fails()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var model = new EditListingViewModel { ListingId = 10, PropertyId = 3, Title = "Updated", Description = "Updated description", Price = 1200 };

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing());
            listingRepoMock.Setup(x => x.UpdateListing(model, "user-123")).ReturnsAsync(false);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.EditListing(model);

            result.Should().BeOfType<ViewResult>();
            controller.TempData["ErrorMessage"].Should().NotBeNull();
            controller.TempData["SuccessMessage"].Should().BeNull();
        }

        [Fact]
        public async Task ListingDetails_Should_Return_NotFound_For_Public_Unverified_Listing()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var hidden = Listing();
            hidden.IsVerified = false;

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(hidden);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupAnonymousController(controller);

            var result = await controller.ListingDetails(10);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ListingDetails_Should_Return_View_For_Owner_Unverified_Listing()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();
            var hidden = Listing();
            hidden.IsVerified = false;

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(hidden);
            listingRepoMock.Setup(x => x.GetListingPhotos(10)).ReturnsAsync([]);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.ListingDetails(10);

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task UploadListingPhoto_Should_Succeed_For_Owner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing());
            listingRepoMock.Setup(x => x.AddListingPhoto(10, It.IsAny<string>(), true, "user-123")).ReturnsAsync(true);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.UploadListingPhoto(10, TestPhoto(), true);

            result.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["SuccessMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task UploadListingPhoto_Should_Return_Forbid_For_NonOwner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing(10, "other-user"));

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.UploadListingPhoto(10, TestPhoto(), true);

            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteListingPhoto_Should_Succeed_For_Owner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing());
            listingRepoMock.Setup(x => x.DeleteListingPhoto(7, 10, "user-123")).ReturnsAsync(true);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.DeleteListingPhoto(10, 7);

            result.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["SuccessMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteListingPhoto_Should_Return_Forbid_For_NonOwner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing(10, "other-user"));

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.DeleteListingPhoto(10, 7);

            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task SetPrimaryListingPhoto_Should_Succeed_For_Owner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing());
            listingRepoMock.Setup(x => x.SetPrimaryListingPhoto(10, 7, "user-123")).ReturnsAsync(true);

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.SetPrimaryListingPhoto(10, 7);

            result.Should().BeOfType<RedirectToActionResult>();
            controller.TempData["SuccessMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task SetPrimaryListingPhoto_Should_Return_Forbid_For_NonOwner()
        {
            var listingRepoMock = new Mock<IListingRepository>();
            var propertyRepoMock = new Mock<IPropertyRepository>();
            var profileRepoMock = new Mock<IProfileRepository>();

            listingRepoMock.Setup(x => x.GetListingById(10)).ReturnsAsync(Listing(10, "other-user"));

            var controller = new ListingController(listingRepoMock.Object, profileRepoMock.Object, propertyRepoMock.Object);
            ControllerTestHelper.SetupController(controller);

            var result = await controller.SetPrimaryListingPhoto(10, 7);

            result.Should().BeOfType<ForbidResult>();
        }

    }
}
