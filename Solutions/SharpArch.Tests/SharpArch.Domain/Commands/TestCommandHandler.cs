namespace Tests.SharpArch.Domain.Commands
{
    using global::SharpArch.Domain.Commands;

    public class TestCommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
    {
        public TestCommandHandler(OnHandle onHandle)
        {
            OnHandleEvent += onHandle;
        }

        public delegate void OnHandle(ICommandHandler<TCommand> handler, TCommand command);

        public event OnHandle OnHandleEvent;

        public void Handle(TCommand command)
        {
            var onHandleEvent = OnHandleEvent;

            if (onHandleEvent != null)
            {
              onHandleEvent(this, command);
            }
        }
    }
}