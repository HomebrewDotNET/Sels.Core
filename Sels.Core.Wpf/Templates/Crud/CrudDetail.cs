using Sels.Core.Wpf.Components.Command.DelegateCommand;
using Sels.Core.Wpf.Components.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Sels.Core.Wpf.Templates.Crud
{
    public abstract class CrudDetail<TObject> : BaseViewModel
    {
        // Properties
        public bool ReadOnly {
            get
            {
                return GetValue<bool>(nameof(ReadOnly));
            }
            set
            {
                SetValue(nameof(ReadOnly), value);
            }
        }
        public TObject DetailObject
        {
            get
            {
                return GetValue<TObject>(nameof(DetailObject));
            }
            set
            {
                SetValue(nameof(DetailObject), value, (x,y) => DetailObjectChanged(x, value));
            }
        }

        // Commands
        /// <summary>
        /// Command that triggers request that the user wants to edit the current DetailObject
        /// </summary>
        public ICommand EditObjectCommand { get; }
        /// <summary>
        /// Command that triggers request that the user wants to delete the current DetailObject
        /// </summary>
        public ICommand DeleteObjectCommand { get; }

        public CrudDetail()
        {
            EditObjectCommand = new DelegateCommand(RaiseEditObjectRequest, exceptionHandler: RaiseExceptionOccured);
            DeleteObjectCommand = new DelegateCommand(RaiseDeleteObjectRequest, exceptionHandler: RaiseExceptionOccured);
            ReadOnly = false;
        }

        // Events
        /// <summary>
        /// Events that gets raised when the user whats to edit the current DetailObject
        /// </summary>
        public event Action<TObject> EditObjectRequest = delegate { };
        private void RaiseEditObjectRequest()
        {
            EditObjectRequest.Invoke(DetailObject);
        }
        /// <summary>
        /// Events that gets raised when the user whats to delete the current DetailObject
        /// </summary>
        public event Action<TObject> DeleteObjectRequest = delegate { };
        private void RaiseDeleteObjectRequest()
        {
            DeleteObjectRequest.Invoke(DetailObject);
        }

        // Abstractions
        /// <summary>
        /// Triggers when the DetailObject changes. Can be used to fetch additional information
        /// </summary>
        /// <param name="detailObjectChanged">Current DetailObject</param>
        protected abstract void DetailObjectChanged(bool wasDifferent, TObject detailObjectChanged);
    }
}
