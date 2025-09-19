namespace Tourist.API.Models.Dto
{
    public class TouristPlacesDto
    {
        public int Id { get; set; }
        public string Country { get; set; }
        
        public string State { get; set; }
     
        public string City { get; set; }
 
        public string PlaceName { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public string ImageUrl { get; set; }

        public decimal EntryFee { get; set; }

        public string CreatedBy { get; set; }
    }
}
