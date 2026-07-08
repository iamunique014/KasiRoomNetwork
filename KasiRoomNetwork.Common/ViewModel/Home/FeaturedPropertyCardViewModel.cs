namespace KasiRoomNetwork.Common.ViewModel.Home
{
    public class FeaturedPropertyCardViewModel
    {
        public int PropertyId { get; set; }

        public string PropertyName { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Suburb { get; set; } = string.Empty;

        public string? PrimaryPhoto { get; set; }

        public int AvailableRooms { get; set; }

        public decimal StartingPrice { get; set; }
    }
}