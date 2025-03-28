using FluentValidation;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Messages.GetMessageThread;

public class GetMessageThreadValidator : AbstractValidator<GetMessageThreadQuery>
{
    private readonly IGroupRepository _groupRepository;
    public GetMessageThreadValidator(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
        
        RuleFor(q => q.GroupId).MustAsync(GroupExists).WithMessage("Group not found");
        RuleFor(q => q).MustAsync(IsUserInGroup).WithMessage("You are not a member of this group");
    }

    private async Task<bool> GroupExists(int groupId, CancellationToken cancellationToken)
    {
        return await _groupRepository.GroupExists(groupId);
    }
    
    private async Task<bool> IsUserInGroup(GetMessageThreadQuery query, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupWithUsers(query.GroupId);
        return group.Users.FirstOrDefault(ug => ug.UserId == query.UserId) != null;
    }
}