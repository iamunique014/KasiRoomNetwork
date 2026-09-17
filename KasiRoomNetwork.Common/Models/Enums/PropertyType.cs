using System.ComponentModel.DataAnnotations;

namespace KasiRoomNetwork.Common.Models.Enums
{
    public enum PropertyType
    {
        [Display(Name = "Back Rooms")]
        BackRooms,

        [Display(Name = "Rental Units")]
        RentalUnits,

        [Display(Name = "Inside Rooms")]
        InsideRooms
    }
}
