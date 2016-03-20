using NUnit.Framework;
using SharpArch.Testing.NUnit;

namespace Tests.Northwind.Wcf.Dtos
{
    using SharpArch.Testing.NUnit.Helpers;

    using global::Northwind.Domain;
    using global::Northwind.Domain.Organization;
    using global::Northwind.WcfServices.Dtos;

    [TestFixture]
    public class TerritoryDtoTests
    {
        [Test]
        public void CanCreateDtoWithEntity() {
            Territory territory = CreateTerritory();

            TerritoryDto territoryDto = TerritoryDto.Create(territory);

            territoryDto.Id.ShouldEqual("08837");
            territoryDto.Description.ShouldEqual("Edison");

            territoryDto.RegionBelongingTo.ShouldNotBeNull();
            territoryDto.RegionBelongingTo.Id.ShouldEqual(1); ;

            territoryDto.Employees.Count.ShouldEqual(2);
            territoryDto.Employees[0].Id.ShouldEqual(5);
            territoryDto.Employees[1].Id.ShouldEqual(10);
        }

        private Territory CreateTerritory() {
            Territory territory = new Territory();
            EntityIdSetter.SetIdOf<string>(territory, "08837");
            territory.Description = "Edison";

            territory.RegionBelongingTo = new Region("Eastern");
            territory.RegionBelongingTo.SetAssignedIdTo(1);

            Employee employee1 = new Employee();
            EntityIdSetter.SetIdOf<int>(employee1, 5);
            territory.Employees.Add(employee1);

            Employee employee2 = new Employee();
            EntityIdSetter.SetIdOf<int>(employee2, 10);
            territory.Employees.Add(employee2);

            return territory;
        }
    }
}
