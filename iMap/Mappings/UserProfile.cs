using AutoMapper;
using iMap.Models;
using iMap.ViewModels;

namespace iMap.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<AppUser, UserViewModel>()
                .ForMember(dst => dst.UserName, opt => opt.MapFrom(x => x.UserName));

            CreateMap<UserViewModel, AppUser>();
        }
    }
}
