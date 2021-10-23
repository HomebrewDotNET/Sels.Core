using Sels.Core.Extensions;
using Sels.Core.Wpf.Components.Command.DelegateCommand;
using Sels.Core.Wpf.Components.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sels.Core.Wpf.Templates.Crud
{
    public abstract class CrudList<TObject> : BaseViewModel
    {
        // Properties
        public bool ReadOnly {
            get
            {
                return GetValue<bool>(nameof(ReadOnly));
            }
            set
            {
                SetValue(nameof(ReadOnly), value, affectedProperties: nameof(CanChange));
            }
        }
        public bool CanChange => !ReadOnly;
        public TObject SelectedObject {
            get
            {
                return GetValue<TObject>(nameof(SelectedObject));
            }
            set
            {
                SetValue(nameof(SelectedObject), value, () => RaiseSelectedObjectChanged());               
            }
        }

        public ObservableCollection<TObject> Objects
        {
            get
            {
                return GetValue<ObservableCollection<TObject>>(nameof(Objects));
            }
            set
            {
                SetValue(nameof(Objects), value);
            }
        }

        // Commands
        public ICommand DeleteObjectCommand { get; set; }

        public CrudList()
        {
            ReadOnly = true;
            DeleteObjectCommand = CreateAsyncCommand<TObject>(DeleteObjectFromList, x => x.HasValue(), nameof(SelectedObject), nameof(Objects));
        }

        private Task DeleteObjectFromList(TObject objectToDelete)
        {
            try
            {
                if(objectToDelete != null)
                {
                    RaiseObjectDeleted(objectToDelete);
                    Objects.Remove(objectToDelete);
                }              
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);          
            }
            return Task.CompletedTask;
        }

        // Events
        /// <summary>
        /// Events that gets raised when the SelectedObject changes
        /// </summary>
        public event Action<TObject> SelectedObjectChanged = delegate{};
        private void RaiseSelectedObjectChanged()
        {
            SelectedObjectChanged.Invoke(SelectedObject);
        }
        /// <summary>
        /// Raised when an object gets deleted from the list
        /// </summary>
        public event Action<TObject> ObjectDeleted = delegate { };
        private void RaiseObjectDeleted(TObject objectDeleted)
        {
            ObjectDeleted.Invoke(objectDeleted);
        }
    }
}
