namespace Tests.Northwind.Wcf
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using Rhino.Mocks;

    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.Testing.NUnit;
    using SharpArch.Testing.NUnit.Helpers;

    using global::Northwind.Domain;
    using global::Northwind.WcfServices;
    using global::Northwind.WcfServices.Dtos;

    [TestFixture]
    public class TerritoriesWcfServiceTests
    {
        /// <summary>
        /// This is testing the behavior of the logic within the WCF service class; it's not actually 
        /// invoking a WCF service or testing integration but focusing on the logic within the service
        /// itself.
        /// </summary>
        [Test]
        public void CanGetTerritories() {
            ITerritoriesWcfService territoriesWcfService = 
                new TerritoriesWcfService(CreateMockTerritoryRepository());

            IList<TerritoryDto> territoryDtos = territoriesWcfService.GetTerritories();

            territoryDtos.Count.ShouldEqual(2);
        }

        private IRepository<Territory> CreateMockTerritoryRepository() {

            IRepository<Territory> repository = MockRepository.GenerateMock<IRepository<Territory>>();
            repository.Expect(r => r.GetAll()).Return(CreateTerritories());

            IDbContext dbContext = MockRepository.GenerateStub<IDbContext>();
            repository.Stub(r => r.DbContext).Return(dbContext);

            return repository;
        }

        /// <summary>
        /// This creates "shallow" territories; there's no need to build out the remainder of the 
        /// model as there are other tests which confirm the DTO creation process.
        /// </summary>
        private List<Territory> CreateTerritories() {
            List<Territory> territories = new List<Territory>();

            Territory territory1 = new Territory();
            EntityIdSetter.SetIdOf<string>(territory1, "08837");
            territory1.Description = "Edison";
            territories.Add(territory1);

            Territory territory2 = new Territory();
            EntityIdSetter.SetIdOf<string>(territory2, "00042");
            territory2.Description = "Galactic";
            territories.Add(territory2);

            return territories;
        }
    }
}
