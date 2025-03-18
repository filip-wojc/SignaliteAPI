using FluentValidation;
using SignaliteWebAPI.Domain.Interfaces.Repositories;
using SignaliteWebAPI.Infrastructure.Repositories.Users;

namespace SignaliteWebAPI.Application.Features.Users.SendFriendRequest;

public class SendFriendRequestValidator : AbstractValidator<SendFriendRequestCommand>
{
    private readonly IUserRepository _userRepository;
    public SendFriendRequestValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(c => c.SendFriendRequestDTO.RecipientId)
            .NotEmpty()
            .NotNull()
            .MustAsync(UserExists).WithMessage("Recipient with given id does not exist");
        RuleFor(c => c.SendFriendRequestDTO.SenderId)
            .NotEmpty()
            .NotNull()
            .MustAsync(UserExists).WithMessage("Sender with given id does not exist");

        RuleFor(c => c).MustAsync(FriendRequestNotExist)
            .WithMessage("Friend request between these users already exist");
    }

    private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserById(userId);
        return user != null;
    }
    
    private async Task<bool> FriendRequestNotExist(SendFriendRequestCommand command, CancellationToken cancellationToken)
    {
        bool exists = await _userRepository.IsFriendRequestExist(
            command.SendFriendRequestDTO.SenderId, 
            command.SendFriendRequestDTO.RecipientId
        );
        
        return !exists;
    } 
}