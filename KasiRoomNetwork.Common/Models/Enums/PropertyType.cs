using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.Models.Enums
{
    public enum PropertyType
    {
        [Display(Name = "Backrooms")]
        Backrooms,

        [Display(Name = "Rental Units")]
        RentalUnits,

        [Display(Name = "Inside Rooms")]
        InsideRooms
    }
}
