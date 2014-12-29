using SharpArch.Domain.Commands;

namespace Tests.SharpArch.Domain.Commands
{
    public class TestCommandHandlerWithResult<TCommand> : ICommandHandler<TCommand, CommandResult<TCommand>> where TCommand : ICommand
    {
        public CommandResult<TCommand> Handle(TCommand command)
        {
            return new CommandResult<TCommand> { Command = command, Success = true };
        }
    }
}