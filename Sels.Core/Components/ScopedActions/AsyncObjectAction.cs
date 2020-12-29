using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Components.ScopedActions
{
    public class AsyncObjectAction<TObject> : IAsyncDisposable where TObject : class
    {
        // Fields
        private TObject _storedObject;
        private Func<TObject, Task> _startAction;
        private Func<TObject, Task> _endAction;

        protected AsyncObjectAction(TObject value, Func<TObject, Task> startAction, Func<TObject, Task> endAction)
        {
            value.ValidateVariable(nameof(value));
            startAction.ValidateVariable(nameof(startAction));
            endAction.ValidateVariable(nameof(endAction));

            _storedObject = value;
            _startAction = startAction;
            _endAction = endAction;           
        }

        public static Task Create(TObject value, Func<TObject, Task> startAction, Func<TObject, Task> endAction)
        {
            var objectAction = new AsyncObjectAction<TObject>(value, startAction, endAction);

            return objectAction.Start();
        }
        
        public async Task Start()
        {
            await _startAction(_storedObject);
        }

        public async ValueTask DisposeAsync()
        {
            await _endAction(_storedObject);
        }
    }

    public class AsyncObjectAction<TObject, TContext> : IAsyncDisposable where TObject : class
    {
        // Fields
        private TObject _storedObject;
        private TContext _storedContext;
        private Func<TObject, Task<TContext>> _startAction;
        private Func<TObject, TContext, Task> _endAction;

        protected AsyncObjectAction(TObject value, Func<TObject, Task<TContext>> startAction, Func<TObject, TContext, Task> endAction)
        {
            value.ValidateVariable(nameof(value));
            startAction.ValidateVariable(nameof(startAction));
            endAction.ValidateVariable(nameof(endAction));

            _storedObject = value;
            _startAction = startAction;
            _endAction = endAction;
        }

        public static Task<IAsyncDisposable> Create(TObject value, Func<TObject, Task<TContext>> startAction, Func<TObject, TContext, Task> endAction)
        {
            var objectAction = new AsyncObjectAction<TObject, TContext>(value, startAction, endAction);

            return objectAction.Start();
        }

        public async Task<IAsyncDisposable> Start()
        {
            _storedContext = await _startAction(_storedObject);

            return this;
        }

        public async ValueTask DisposeAsync()
        {
            await _endAction(_storedObject, _storedContext);
        }
    }
}
