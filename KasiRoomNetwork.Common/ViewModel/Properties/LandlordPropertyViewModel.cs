namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class LandlordPropertyViewModel
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Suburb { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
    }
}
