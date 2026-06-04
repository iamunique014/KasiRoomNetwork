using FluentAssertions;
using Kasi_Room_Network___KRN.Controllers;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Test.Helpers;
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
    }
}
