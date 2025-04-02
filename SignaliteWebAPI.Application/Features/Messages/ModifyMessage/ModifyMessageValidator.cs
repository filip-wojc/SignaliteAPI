using FluentValidation;

namespace SignaliteWebAPI.Application.Features.Messages.ModifyMessage;

public class ModifyMessageValidator : AbstractValidator<ModifyMessageCommand>
{
    public ModifyMessageValidator()
    {
        RuleFor(x => x.MessageContent).NotNull().NotEmpty();
    }
}