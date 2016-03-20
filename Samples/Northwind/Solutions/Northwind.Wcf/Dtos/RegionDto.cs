namespace Northwind.WcfServices.Dtos
{
    using Northwind.Domain;

    /// <summary>
    ///   DTO for a region entity.
    /// </summary>
    public class RegionDto
    {
        private RegionDto()
        {
        }

        public string Description { get; set; }

        public int Id { get; set; }

        /// <summary>
        ///   Transfers the region entity's property values to the DTO.
        ///   Strongly consider Jimmy Bogard's AutoMapper (http://automapper.codeplex.com/) 
        ///   for doing this kind of work in a more automated fashion.
        /// </summary>
        public static RegionDto Create(Region region)
        {
            if (region == null)
            {
                return null;
            }

            return new RegionDto { Id = region.Id, Description = region.Description };
        }
    }
}