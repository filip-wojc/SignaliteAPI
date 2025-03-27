using FluentValidation;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Messages.SendMessage;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    private readonly IGroupRepository _groupRepository;
    public SendMessageValidator(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
        
        RuleFor(x => x.SendMessageDto.GroupId).MustAsync(GroupExists).WithMessage("Group does not exist");
        RuleFor(x => x).MustAsync(IsUserInGroup).WithMessage("User is not in group");
    }

    private async Task<bool> GroupExists(int groupId, CancellationToken cancellationToken)
    {
        return await _groupRepository.GroupExists(groupId);
    }

    private async Task<bool> IsUserInGroup(SendMessageCommand command, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetGroupWithUsers(command.SendMessageDto.GroupId);
        return group.Users.FirstOrDefault(ug => ug.UserId == command.SenderId) != null;
    }
}