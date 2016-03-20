namespace Tests.Northwind.Wcf.Dtos
{
    using global::Northwind.Domain;
    using global::Northwind.WcfServices.Dtos;

    using NUnit.Framework;

    using SharpArch.Testing.NUnit;

    [TestFixture]
    public class RegionDtoTests
    {
        [Test]
        public void CanCreateDtoWithEntity() 
        {
            Region region = new Region("Eastern");
            region.SetAssignedIdTo(1);

            RegionDto regionDto = RegionDto.Create(region);

            regionDto.Id.ShouldEqual(1);
            regionDto.Description.ShouldEqual("Eastern");
        }
    }
}
