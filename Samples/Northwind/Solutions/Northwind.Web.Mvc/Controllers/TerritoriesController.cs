namespace Northwind.Web.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Web.Mvc;

    using Northwind.WcfServices;
    using Northwind.WcfServices.Dtos;

    using SharpArch.Domain;

    public class TerritoriesController : Controller
    {
        private readonly ITerritoriesWcfService territoriesWcfService;

        public TerritoriesController(ITerritoriesWcfService territoriesWcfService)
        {
            Check.Require(territoriesWcfService != null, "territoriesWcfService may not be null");

            this.territoriesWcfService = territoriesWcfService;
        }

        public ActionResult Index()
        {
            IList<TerritoryDto> territories = null;

            // WCF service closing advice taken from http://msdn.microsoft.com/en-us/library/aa355056.aspx
            
            territories = this.territoriesWcfService.GetTerritories();
            
            return View(territories);
        }
    }
}