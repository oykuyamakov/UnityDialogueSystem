using System;
using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public abstract class ResultExecutor : MonoBehaviour
    {
        public abstract Type HandledResultType { get; }
        public abstract void Execute(Result data);
        public virtual void Revert() { }
    }

    public abstract class ResultExecutor<TData> : ResultExecutor where TData : Result
    {
        public sealed override Type HandledResultType => typeof(TData);

        public sealed override void Execute(Result data)
        {
            Execute((TData)data);
        }

        protected abstract void Execute(TData data);
    }
}
