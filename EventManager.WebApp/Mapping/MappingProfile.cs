using AutoMapper;
using EventManager.DAL.Models;
using EventManager.WebApp.ViewModels;

namespace EventManager.WebApp.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Maps performer entity to performer display view model.
            CreateMap<Performer, PerformerDisplayVM>();

            // Maps Event entity to EventVM, including related display fields.
            CreateMap<Event, EventVM>()
                .ForMember(dest => dest.EventTypeName, opt => opt.MapFrom(src => src.EventType != null ? src.EventType.Name : "-"))
                .ForMember(dest => dest.CreatedByEmail, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Email : "-"))
                .ForMember(dest => dest.ImageFileName, opt => opt.MapFrom(src => src.Image != null ? src.Image.FileName : "-"))
                // Maps performers assigned to an event into display ViewModels.
                .ForMember(dest => dest.Performers,
                    opt => opt.MapFrom(src => src.EventPerformers
                        .Where(ep => ep.Performer != null)
                        .Select(ep => ep.Performer)));

            CreateMap<Event, EventEditVM>();

            CreateMap<EventCreateVM, Event>();

            CreateMap<EventSearchVM, Event>();


            CreateMap<EventEditVM, Event>();

            CreateMap<EventType, EventTypeVM>();

            CreateMap<EventTypeVM, EventType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore());

            CreateMap<Performer, PerformerVM>();

            CreateMap<PerformerVM, Performer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EventPerformers, opt => opt.Ignore());
        }
    }
}
