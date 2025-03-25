using FluentValidation;
using SignaliteWebAPI.Application.Features.Users.AddProfilePhoto;

namespace SignaliteWebAPI.Application.Features.Users.UpdateProfilePhoto;

public class UpdateProfilePhotoValidator : AbstractValidator<UpdateProfilePhotoCommand>
{
    public UpdateProfilePhotoValidator()
    {
        RuleFor(c => c.PhotoFile).NotEmpty().NotNull();
    }
}