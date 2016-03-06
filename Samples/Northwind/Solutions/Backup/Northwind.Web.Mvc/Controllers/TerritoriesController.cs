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
            // As alternative to this verbose-ness, use the SharpArch.WcfClient.Castle.WcfSessionFacility
            // for automatically closing the WCF service.
            try
            {
                territories = this.territoriesWcfService.GetTerritories();
                this.territoriesWcfService.Close();
            }
            catch (CommunicationException)
            {
                this.territoriesWcfService.Abort();
            }
            catch (TimeoutException)
            {
                this.territoriesWcfService.Abort();
            }
            catch (Exception)
            {
                this.territoriesWcfService.Abort();
                throw;
            }

            return View(territories);
        }
    }
}