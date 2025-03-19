using MediatR;
using SignaliteWebAPI.Domain.DTOs.Auth;

namespace SignaliteWebAPI.Application.Features.Auth.Register;

public class RegisterCommand : IRequest
{
    public RegisterDTO RegisterDto { get; set; }
}