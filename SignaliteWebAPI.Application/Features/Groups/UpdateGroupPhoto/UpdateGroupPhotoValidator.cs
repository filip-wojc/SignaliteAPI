using FluentValidation;

namespace SignaliteWebAPI.Application.Features.Groups.UpdateGroupPhoto;

public class UpdateGroupPhotoValidator : AbstractValidator<UpdateGroupPhotoCommand>
{
    public UpdateGroupPhotoValidator()
    {
        RuleFor(c => c.Photo).NotEmpty().NotNull();
    }
}