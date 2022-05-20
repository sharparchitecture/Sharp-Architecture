namespace Suteki.TardisBank.Infrastructure.Configuration;

using Api.Announcements;
using AutoMapper;
using Domain;


public class AnnouncementMappingProfile : Profile
{
    /// <inheritdoc />
    public AnnouncementMappingProfile()
    {
        CreateMap<Announcement, AnnouncementSummary>();
        CreateMap<Announcement, AnnouncementModel>();
        CreateMap<NewAnnouncement, Announcement>()
            .ForMember(d => d.LastModifiedUtc, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Id, opt => opt.Ignore())
            ;
    }
}
