#pragma warning disable 1573
namespace Suteki.TardisBank.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Api.Announcements;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Domain;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using NHibernate.Linq;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.Web.AspNetCore.Transaction;


    [ApiController]
    [Route("[controller]")]
    [Transaction(IsolationLevel.ReadCommitted)]
    public class AnnouncementsController : ControllerBase
    {
        readonly ILinqRepository<Announcement, int> _announcementRepository;
#if NET3UP
        readonly LinkGenerator _linkGenerator;
#endif
        readonly IMapper _mapper;

        /// <summary>
        ///     Creates AnnouncementController.
        /// </summary>
        /// <param name="announcementRepository">Announcements repository.</param>
        /// <param name="mapper">Mapper</param>
        public AnnouncementsController(ILinqRepository<Announcement, int> announcementRepository,
#if NET3UP
            LinkGenerator linkGenerator,
#endif
            IMapper mapper)
        {
            _announcementRepository = announcementRepository ?? throw new ArgumentNullException(nameof(announcementRepository));
#if NET3UP
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
#endif
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     Retrieves  announcements.
        /// </summary>
        /// <returns>Announcement sorted by date (desc).</returns>
        public async Task<ActionResult<IEnumerable<AnnouncementSummary>>> Get()
        {
            var res = await _announcementRepository.FindAll().WithOptions(x => { x.SetCacheable(true); })
                .OrderByDescending(a => a.Date)
                .ProjectTo<AnnouncementSummary>(_mapper.ConfigurationProvider)
                .ToListAsync(HttpContext.RequestAborted).ConfigureAwait(false);
            return res;
        }

        /// <summary>
        ///     Retrieves announcement by Id.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>
        ///     Announcement data.
        /// </returns>
        [HttpGet]
        [Route("{id}", Name = "GetAnnouncement")]
        public async Task<ActionResult<AnnouncementModel>> Get(int id)
        {
            var announcement = await _announcementRepository.GetAsync(id).ConfigureAwait(false);
            if (announcement != null) return _mapper.Map<AnnouncementModel>(announcement);
            return NotFound(new {id});
        }

        [HttpPost]
        public async Task<ActionResult> Post(NewAnnouncement model)
        {
            var announcement = _mapper.Map<Announcement>(model);
            await _announcementRepository.SaveAsync(announcement, HttpContext.RequestAborted).ConfigureAwait(false);
#if NET3UP
            var location = _linkGenerator.GetPathByName("GetAnnouncement", new {id = announcement.Id});
#else
            var location = Url.RouteUrl("GetAnnouncement", new {id = announcement.Id});
#endif
            return Created(location!, announcement);
        }

        /// <summary>
        ///     Removes announcement.
        /// </summary>
        /// <param name="id">Announcement Id.</param>
        /// <returns>OK</returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _announcementRepository.DeleteAsync(id, HttpContext.RequestAborted).ConfigureAwait(false);
            return NoContent();
        }

        /// <summary>
        ///     Retrieves last 10 announcements.
        /// </summary>
        /// <returns>Announcement, newest first.</returns>
        [HttpGet]
        [Route("latest")]
        public async Task<ActionResult<IEnumerable<AnnouncementSummary>>> Latest()
        {
            var res = await _announcementRepository.FindAll()
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ProjectTo<AnnouncementSummary>(_mapper.ConfigurationProvider).ToListAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false);
            return res;
        }
    }
}
