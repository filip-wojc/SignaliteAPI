using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.Models;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.CreateGroup;

public class CreateGroupHandler(IGroupRepository groupRepository, IMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<CreateGroupCommand>
{
    public async Task Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = mapper.Map<CreateGroupCommand, Group>(request);
        group.IsPrivate = false;
        
        // create group before transactions scope to get groupId
        await groupRepository.CreateGroup(group);
        await unitOfWork.SaveChangesAsync();
        
        try
        {
            await unitOfWork.BeginTransactionAsync();
            var userGroup = new UserGroup
            {
                GroupId = group.Id,
                UserId = request.OwnerId
            };
            await groupRepository.AddUserToGroup(userGroup);
            await unitOfWork.CommitTransactionAsync();
        }
        catch (Exception e)
        {
           await unitOfWork.RollbackTransactionAsync();

           // delete group if transaction failed
           groupRepository.DeleteGroup(group);
           await unitOfWork.SaveChangesAsync();
        }
    }
}