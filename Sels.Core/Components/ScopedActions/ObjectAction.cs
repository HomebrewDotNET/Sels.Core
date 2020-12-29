using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.ScopedActions
{
    public class ObjectAction<TObject> : IDisposable where TObject : class
    {
        // Fields
        private TObject _storedObject;
        private Action<TObject> _endAction;

        public ObjectAction(TObject value, Action<TObject> startAction, Action<TObject> endAction)
        {
            value.ValidateVariable(nameof(value));
            startAction.ValidateVariable(nameof(startAction));
            endAction.ValidateVariable(nameof(endAction));

            _storedObject = value;
            _endAction = endAction;

            startAction(_storedObject);
        }

        public void Dispose()
        {
            _endAction(_storedObject);
        }
    }

    public class ObjectAction<TObject, TContext> : IDisposable where TObject : class
    {
        // Fields
        private TObject _storedObject;
        private TContext _storedContext;
        private Action<TObject, TContext> _endAction;

        public ObjectAction(TObject value, Func<TObject, TContext> startAction, Action<TObject, TContext> endAction)
        {
            value.ValidateVariable(nameof(value));
            startAction.ValidateVariable(nameof(startAction));
            endAction.ValidateVariable(nameof(endAction));

            _storedObject = value;
            _endAction = endAction;

            _storedContext = startAction(_storedObject);
        }

        public void Dispose()
        {
            _endAction(_storedObject, _storedContext);
        }
    }
}
