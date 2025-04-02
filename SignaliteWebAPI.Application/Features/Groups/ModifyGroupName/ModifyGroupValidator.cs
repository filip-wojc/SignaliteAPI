using FluentValidation;

namespace SignaliteWebAPI.Application.Features.Groups.ModifyGroupName;

public class ModifyGroupValidator : AbstractValidator<ModifyGroupCommand>
{
    public ModifyGroupValidator()
    {
        RuleFor(c => c.GroupName).NotEmpty().NotNull().MaximumLength(32);
    }
}