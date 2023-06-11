using AutoMapper;
using iMap.ViewModels;
using iMap.Models;
using iMap.Helpers;

namespace iMap.Mappings
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageViewModel>()
                .ForMember(dst => dst.FromUserName, opt => opt.MapFrom(x => x.FromUser.UserName))
                
                .ForMember(dst => dst.Room, opt => opt.MapFrom(x => x.ToRoom.Name))
                
                .ForMember(dst => dst.Content, opt => opt.MapFrom(x => BasicEmojis.ParseEmojis(x.Content)));

            CreateMap<MessageViewModel, Message>();
        }
    }
}
