using System;
namespace ready.Interfaces
{
	public interface ICoffeeMachine
	{
        DateTime UtcNow { get; }

        int GetCallCount();
    }
}

