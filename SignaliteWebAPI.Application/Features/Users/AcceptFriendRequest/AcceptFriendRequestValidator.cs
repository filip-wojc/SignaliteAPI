using FluentValidation;
using SignaliteWebAPI.Domain.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;

public class AcceptFriendRequestValidator : AbstractValidator<AcceptFriendRequestCommand>
{
    public AcceptFriendRequestValidator()
    {
      
    }
    
}