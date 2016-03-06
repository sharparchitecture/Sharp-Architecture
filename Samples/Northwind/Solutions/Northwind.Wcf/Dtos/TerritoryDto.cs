namespace Northwind.WcfServices.Dtos
{
    using System.Collections.Generic;

    using Northwind.Domain;

    public class TerritoryDto
    {
        private TerritoryDto()
        {
            this.Employees = new List<EmployeeDto>();
        }

        public string Description { get; set; }

        /// <summary>
        ///   I'd prefer to have a protected setter, but since we need it to be XML-serializable, the setter must be public
        /// </summary>
        public List<EmployeeDto> Employees { get; set; }

        public string Id { get; set; }

        public RegionDto RegionBelongingTo { get; set; }

        /// <summary>
        ///   Transfers the territory entity's property values to the DTO.
        ///   Strongly consider Jimmy Bogard's AutoMapper (http://automapper.codeplex.com/) 
        ///   for doing this kind of work in a more automated fashion.
        /// </summary>
        public static TerritoryDto Create(Territory territory)
        {
            if (territory == null)
            {
                return null;
            }

            var territoryDto = new TerritoryDto();
            territoryDto.Id = territory.Id;
            territoryDto.RegionBelongingTo = RegionDto.Create(territory.RegionBelongingTo);
            territoryDto.Description = territory.Description;

            foreach (var employee in territory.Employees)
            {
                territoryDto.Employees.Add(EmployeeDto.Create(employee));
            }

            return territoryDto;
        }
    }
}