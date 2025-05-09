using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using SignaliteWebAPI.Application.Features.Groups.CreateGroup;
using SignaliteWebAPI.Domain.DTOs.Groups;
using SignaliteWebAPI.Domain.Models;
using SQLitePCL;

namespace SignaliteWebAPI.Application.MappingProfiles;

public class GroupsAutoMapper : Profile
{
    public GroupsAutoMapper()
    {
        CreateMap<CreateGroupCommand, Group>();
        CreateMap<Group, GroupMembersDTO>()
            .ForMember(g => g.Owner, o => o.MapFrom(g => g.Owner))
            .ForMember(g => g.Members,
                o => o.MapFrom(g => g.Users.Where(u => g.OwnerId != u.UserId).Select(u => u.User)));
        CreateMap<Group, GroupBasicInfoDTO>()
            // map the name (other user username if private group, otherwise the group name)
            .ForMember(g => g.Name, o =>
                o.MapFrom(
                    (group, groupDto, destMember,
                        context) => // <- nie ruszac nieużywanych parametrów bo nie działa inaczej 💀
                    {
                        if (group.IsPrivate && context.Items.TryGetValue("UserId", out var id))
                        {
                            var userId = (int)id;
                            var otherUser = group.Users.FirstOrDefault(u => u.UserId != userId)?.User;

                            if (otherUser != null)
                                return $"{otherUser.Username}";
                        }

                        return group.Name;
                    }))
            // map the photo (if private group set the photo as the other user profile photo, if not use the group photo)
            .ForMember(g => g.PhotoUrl, o =>
                o.MapFrom(
                    (group, groupDto, destMember,
                        context) => // <- nie ruszac nieużywanych parametrów bo nie działa inaczej 💀
                    {
                        if (group.IsPrivate && context.Items.TryGetValue("UserId", out var id))
                        {
                            var userId = (int)id;
                            var otherUser = group.Users.FirstOrDefault(u => u.User.Id != userId)?.User;

                            if (otherUser?.ProfilePhoto != null)
                                return otherUser.ProfilePhoto.Url;
                        }

                        return group.Photo?.Url;
                    }))
            // format the last message with username and depending on attachment present
            .ForMember(g => g.LastMessage, o => o.MapFrom((group, context) =>
            {
                var lastMessage = group.Messages.OrderByDescending(m => m.DateSent).FirstOrDefault();

                if (lastMessage == null)
                    return null;

                string senderUsername = lastMessage.Sender.Username;

                if (lastMessage.Attachment != null)
                    return $"{senderUsername}: sent an attachment";

                return $"{senderUsername}: {lastMessage.Content}";
            }))
            .ForMember(g => g.LastMessageDate, o => o.MapFrom(g => 
                g.Messages.Any() 
                    ? g.Messages.OrderByDescending(m => m.DateSent).First().DateSent 
                    : DateTime.MinValue));
    }
}