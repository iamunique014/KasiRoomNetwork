using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KasiRoomNetwork.Test.Repositories
{
    public class PropertyRepositoryTests
    {
        [Fact]
        public async Task DeletePropertyAsync_Should_Call_sp_Property_Delete()
        {
            // Arrange

            var dbMock = new Mock<ISqlDataAccess>();

            var repository = new PropertyRepository(dbMock.Object);

            int propertyId = 10;
            string landlordId = "landlord-123";

            // Act

            await repository.DeletePropertyAsync(propertyId, landlordId);

            // Assert

            dbMock.Verify(
                x => x.SaveData(
                     "sp_Property_Delete",
                     It.Is<object>(p =>
                         p.ToString()!.Contains("PropertyId") &&
                         p.ToString()!.Contains("LandlordId")),
                     "conn"),
                Times.Once);
        }
    }
}
