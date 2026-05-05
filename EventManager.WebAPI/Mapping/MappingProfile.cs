using AutoMapper;
using EventManager.WebAPI.Dtos;
using EventManager.WebAPI.Models;

namespace EventManager.WebAPI.Mapping
{
    /// <summary>
    /// Defines AutoMapper rules between EF models and DTOs.
    /// This lets controllers convert objects with less manual mapping code.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Event entity -> EventDto
            // Used when returning event data to the client.
            CreateMap<Event, EventDto>();

            // EventDto -> Event entity
            // Used when creating or updating Event objects from DTO input.
            // Some members are ignored because they are controlled by the server
            // or represent navigation properties that should not be mapped directly.
            CreateMap<EventDto, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.EventPerformers, opt => opt.Ignore())
                .ForMember(dest => dest.EventType, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.Registrations, opt => opt.Ignore());

            // Performer entity -> PerformerDto
            // Used when sending performer data to the client.
            CreateMap<Performer, PerformerDto>();

            // PerformerDto -> Performer entity
            // Used when creating or updating performer records.
            CreateMap<PerformerDto, Performer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Registration entity -> RegistrationDetailsDto
            // Includes data from related entities:
            // Event.Name -> EventName
            // User.Username -> Username
            CreateMap<Registration, RegistrationDetailsDto>()
                .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.Event.Name))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username));
        }
    }
}