namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Http;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using NHibernate.Linq;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.Web.Http;
    using Suteki.TardisBank.Domain;
    using Suteki.TardisBank.Web.Mvc.Models;
    using Suteki.TardisBank.Web.Mvc.WebApi;

    /// <summary>
    /// Provides WebApi access to announcements.
    /// </summary>
    [Transaction]
    [RoutePrefix("api/announcements")]
    public class AnnouncementsController : ApiController
    {
        private readonly ILinqRepository<Announcement> announcementRepository;

        /// <summary>
        /// Creates AnnouncementController.
        /// </summary>
        /// <param name="announcementRepository">Announcements repository.</param>
        public AnnouncementsController(ILinqRepository<Announcement> announcementRepository)
        {
            this.announcementRepository = announcementRepository;
            Debug.WriteLine("Created Announcements controller {0:x}", GetHashCode());
        }

        /// <summary>
        /// Retrieves  announcements.
        /// </summary>
        /// <returns>Announcement sorted by date (desc).</returns>
        public IEnumerable<AnnouncementSummary> Get()
        {
            return announcementRepository.FindAll().Cacheable()
                .OrderByDescending(a => a.Date)
                .ProjectTo<AnnouncementSummary>();
        }

        /// <summary>
        /// Retrieves announcement by Id.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>
        /// Announcement data.
        /// </returns>
        public AnnouncementModel Get(int id)
        {
            var announcement =  announcementRepository.Get(id);
            return announcement != null ? Mapper.Map<AnnouncementModel>(announcement) : null;
        }

        [Authorize(Roles = UserRoles.Parent)]
        [ValidateModel]
        public IHttpActionResult PostAnnouncement(AnnouncementModel model)
        {
            var announcement = Mapper.Map<Announcement>(model);
            announcementRepository.Save(announcement);
            var location = Url.Link("DefaultApi", new { controller = "Announcements", id = announcement.Id });
            return Created(location, announcement);
        }

        /// <summary>
        /// Removes announcement.
        /// </summary>
        /// <param name="id">Announcement Id.</param>
        /// <returns>OK</returns>
        [Authorize(Roles = UserRoles.Parent)]
        public IHttpActionResult Delete(int id)
        {
            announcementRepository.Delete(id);
            return Ok(new { result = "OK" });
        }

        /// <summary>
        /// Retrieves last 10 announcements.
        /// </summary>
        /// <returns>Announcement, newest first.</returns>
        [Route("latest")]
        [HttpGet]
        public IEnumerable<AnnouncementSummary> Latest()
        {
            return announcementRepository.FindAll().Cacheable()
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ProjectTo<AnnouncementSummary>();
        }

        protected override void Dispose(bool disposing)
        {
            Debug.WriteLine("Disposing AnnouncementController {0:x}", GetHashCode());
            base.Dispose(disposing);
        }
    }
}