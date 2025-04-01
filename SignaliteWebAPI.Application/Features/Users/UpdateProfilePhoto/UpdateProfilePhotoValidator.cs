using FluentValidation;

namespace SignaliteWebAPI.Application.Features.Users.UpdateProfilePhoto;

public class UpdateProfilePhotoValidator : AbstractValidator<UpdateProfilePhotoCommand>
{
    public UpdateProfilePhotoValidator()
    {
        RuleFor(c => c.PhotoFile).NotEmpty().NotNull();
    }
}