using ApplicationAuth.Common.Extensions;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Telegram;
using ApplicationAuth.Models.RequestModels;
using ApplicationAuth.Models.ResponseModels;
using ApplicationAuth.Models.ResponseModels.Session;
using ApplicationAuth.Models.ResponseModels.Telegram;
using Profile = ApplicationAuth.Domain.Entities.Identity.Profile;

namespace ApplicationAuth.Services.StartApp
{
    public class AutoMapperProfileConfiguration : AutoMapper.Profile
    {
        public AutoMapperProfileConfiguration()
        : this("MyProfile")
        {
        }

        protected AutoMapperProfileConfiguration(string profileName)
        : base(profileName)
        {

            CreateMap<TelegramSticker, TelegramStickerResponseModel>()
                .ForMember(t => t.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(t => t.CountOfUsage, opt => opt.MapFrom(src => src.CountOfUsage))
                .ForMember(t => t.StickerId, opt => opt.MapFrom(src => src.StickerId))
                .ForMember(t => t.FileUniqueId, opt => opt.MapFrom(src => src.FileUniqueId))
                .ForMember(t => t.DateOfFirstUsage, opt => opt.MapFrom(src => src.DateTime.ToISO()));

            CreateMap<TelegramMessage, TelegramMessageResponseModel>()
                .ForMember(t => t.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(t => t.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(t => t.SenderId, opt => opt.MapFrom(src => src.UserToken))
                .ForMember(t => t.SentAt, opt => opt.MapFrom(src => src.DateTime.ToISO()));

            CreateMap<UserDevice, UserDeviceResponseModel>()
                .ForMember(t => t.AddedAt, opt => opt.MapFrom(src => src.AddedAt.ToISO()));

            #region User model

            CreateMap<UserProfileRequestModel, Profile>()
                .ForMember(t => t.Id, opt => opt.Ignore())
                .ForMember(t => t.User, opt => opt.Ignore());

            CreateMap<Profile, UserResponseModel>()
                .ForMember(t => t.Email, opt => opt.MapFrom(x => x.User != null ? x.User.Email : ""))
                .ForMember(t => t.PhoneNumber, opt => opt.MapFrom(x => x.User != null ? x.User.PhoneNumber : ""))
                .ForMember(t => t.IsBlocked, opt => opt.MapFrom(x => x.User != null ? !x.User.IsActive : false));

            CreateMap<ApplicationUser, UserBaseResponseModel>()
               .IncludeAllDerived();

            CreateMap<ApplicationUser, UserResponseModel>()
                .ForMember(x => x.FirstName, opt => opt.MapFrom(x => x.Profile.FirstName))
                .ForMember(x => x.LastName, opt => opt.MapFrom(x => x.Profile.LastName))
                .ForMember(x => x.IsBlocked, opt => opt.MapFrom(x => !x.IsActive))
                .IncludeAllDerived();

            CreateMap<ApplicationUser, UserRoleResponseModel>();

            #endregion

        }
    }
}
