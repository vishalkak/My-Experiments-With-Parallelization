using System;

namespace Finance.Server.Infrastructure.StateNode
{
    public interface IStateNode<out T>
    {
        Func<T> Result { get; }
    }

    public class StateNode<T> : IStateNode<T>
    {
        public StateNode(Func<T> result)
        {
            Result = result;
        }

        public Func<T> Result { get; }
    }

    public static class StateNode
    {
        public static IStateNode<TOutput> FromResult<TOutput>(TOutput value)
        {
            return new StateNode<TOutput>(() => value);
        }

        public static IStateNode<TOutput> FromResult<TOutput>(Func<TOutput> value)
        {
            return new StateNode<TOutput>(value);
        }
    }

    public static class StateNodeExtensions
    {
        public static IStateNode<TSecoundOutput> Then<TFirstOutput, TSecoundOutput>(this IStateNode<TFirstOutput> firstNode, Func<TFirstOutput, TSecoundOutput> nextNodeFunc)
        {
            return new StateNode<TSecoundOutput>(() =>
            {
                var firstOutput = firstNode.Result();

                var secondOutput = nextNodeFunc(firstOutput);

                return secondOutput;
            });
        }

        public static TOutput Execute<TOutput>(this IStateNode<TOutput> stateNode)
        {
            return stateNode.Result();
        }
    }
}
