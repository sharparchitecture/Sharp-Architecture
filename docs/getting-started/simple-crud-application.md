# Building a simple CRUD feature

This walks through a real CRUD slice — entity, mapping, and controller — taken directly
from the [`TardisBank`](../../Src/Samples/TardisBank) sample's `Announcement` feature.
[Install](installation.md) S#arp Architecture before starting.

## 1. Define the entity

Domain entities inherit from `Entity<TId>`. All properties that NHibernate needs to
persist must be `virtual` (NHibernate proxies override them for lazy loading):

```csharp
// Suteki.TardisBank.Domain/Announcement.cs
public class Announcement : Entity<int>
{
    public virtual DateTime Date { get; set; }

    [MaxLength(120)]
    public virtual string Title { get; set; } = null!;

    [MaxLength(2000)]
    public virtual string? Content { get; set; }

    public virtual DateTime LastModifiedUtc { get; set; }
}
```

`Id` (inherited from `Entity<int>`) is already there, with a `protected` setter — you
don't declare it yourself, and application code never sets it directly.

## 2. Map it

With FluentNHibernate's auto-persistence model doing the heavy lifting, you only need
an override for anything the conventions can't infer:

```csharp
// Suteki.TardisBank.Infrastructure/NHibernateMaps/AnnouncementMap.cs
public class AnnouncementMap : IAutoMappingOverride<Announcement>
{
    public void Override(AutoMapping<Announcement> mapping)
    {
        mapping.Map(x => x.Date).CustomSqlType("date");
        mapping.Map(x => x.LastModifiedUtc).CustomType(typeof(UtcDateTimeType));
    }
}
```

## 3. Register the repository

`ILinqRepository<Announcement, int>` gives you LINQ queries (`FindAll()`) plus the base
CRUD operations (`GetAsync`, `SaveAsync`, `DeleteAsync`, ...). Register it as shown in
[Installation](installation.md#wire-up-nhibernate-and-aspnet-core).

## 4. Write the controller

```csharp
// Suteki.TardisBank.WebApi/Controllers/AnnouncementsController.cs
[ApiController]
[Route("[controller]")]
[Transaction(IsolationLevel.ReadCommitted)]
public class AnnouncementsController : ControllerBase
{
    readonly ILinqRepository<Announcement, int> _announcementRepository;
    readonly IMapper _mapper;

    public AnnouncementsController(
        ILinqRepository<Announcement, int> announcementRepository, IMapper mapper)
    {
        _announcementRepository = announcementRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnnouncementSummary>>> Get()
    {
        var result = await _announcementRepository.FindAll()
            .OrderByDescending(a => a.Date)
            .ProjectTo<AnnouncementSummary>(_mapper.ConfigurationProvider)
            .ToListAsync(HttpContext.RequestAborted);
        return result;
    }

    [HttpGet]
    [Route("{id}", Name = "GetAnnouncement")]
    public async Task<ActionResult<AnnouncementModel>> Get(int id)
    {
        var announcement = await _announcementRepository.GetAsync(id);
        return announcement is null
            ? NotFound(new { id })
            : _mapper.Map<AnnouncementModel>(announcement)!;
    }

    [HttpPost]
    public async Task<ActionResult> Post(NewAnnouncement model)
    {
        var announcement = _mapper.Map<Announcement>(model)!;
        await _announcementRepository.SaveAsync(announcement, HttpContext.RequestAborted);
        return Created($"/announcements/{announcement.Id}", announcement);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _announcementRepository.DeleteAsync(id, HttpContext.RequestAborted);
        return NoContent();
    }
}
```

`[Transaction(IsolationLevel.ReadCommitted)]` on the controller means every action on
it runs inside a transaction opened by `AutoTransactionHandler`, committed on success
and rolled back on an unhandled exception. See [Unit of Work](../general/unit-of-work.md)
for the full behavior.

That's the whole slice — no hand-written SQL, no manual session handling, and no
separate repository class to write since `LinqRepository<TEntity,TId>` already
implements everything `ILinqRepository` needs.

For the full working project — `Program.cs`, DI container setup, view/DTO mapping
profiles, and the test suite — see
[`Src/Samples/TardisBank`](../../Src/Samples/TardisBank) directly.
