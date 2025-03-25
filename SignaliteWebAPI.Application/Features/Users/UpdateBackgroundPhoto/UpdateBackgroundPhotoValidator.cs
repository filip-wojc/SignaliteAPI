using FluentValidation;

namespace SignaliteWebAPI.Application.Features.Users.UpdateBackgroundPhoto;

public class UpdateBackgroundPhotoValidator : AbstractValidator<UpdateBackgroundPhotoCommand>
{
    public UpdateBackgroundPhotoValidator()
    {
        RuleFor(c => c.PhotoFile).NotEmpty().NotNull();
    }
}