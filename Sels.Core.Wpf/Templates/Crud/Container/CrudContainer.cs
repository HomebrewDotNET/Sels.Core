using Sels.Core.Wpf.Components.Command.DelegateCommand;
using Sels.Core.Wpf.Components.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Sels.Core.Extensions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Sels.Core.Wpf.Templates.Crud
{
    public abstract class CrudContainer<TObject, TValidationError, TListViewModel, TDetailViewModel, TCreateOrUpdateViewModel> : BaseViewModel where TListViewModel : CrudList<TObject> where TDetailViewModel : CrudDetail<TObject> where TCreateOrUpdateViewModel : CrudCreateOrEdit<TObject, TValidationError> where TObject : new()
    {
        // Properties
        public BaseViewModel CurrentControl {
            get
            {
                return GetValue<BaseViewModel>(nameof(CurrentControl));
            }
            protected set
            {
                SetValue(nameof(CurrentControl), value);
            }
        }
        public TListViewModel ListViewModel {
            get
            {
                return GetValue<TListViewModel>(nameof(ListViewModel));
            }
            private set
            {
                SetValue(nameof(ListViewModel), value);
            }
        }
        public TDetailViewModel DetailViewModel {
            get
            {
                return GetValue<TDetailViewModel>(nameof(DetailViewModel));
            }
            private set
            {
                SetValue(nameof(DetailViewModel), value);
            }
        }
        public TCreateOrUpdateViewModel CreateOrUpdateViewModel {
            get
            {
                return GetValue<TCreateOrUpdateViewModel>(nameof(CreateOrUpdateViewModel));
            }
            private set
            {
                SetValue(nameof(CreateOrUpdateViewModel), value);
            }
        }

        // Commands
        /// <summary>
        /// Command to trigger request that user wants to create an object
        /// </summary>
        public ICommand CreateObjectRequestCommand { get; }
        /// <summary>
        /// Used to refresh the current view model
        /// </summary>
        public ICommand RefreshCommand { get; }

        public CrudContainer(TListViewModel listViewModel, TDetailViewModel detailViewModel, TCreateOrUpdateViewModel createOrUpdateViewModel)
        {
            listViewModel.ValidateVariable(nameof(listViewModel));
            detailViewModel.ValidateVariable(nameof(detailViewModel));
            createOrUpdateViewModel.ValidateVariable(nameof(createOrUpdateViewModel));

            ListViewModel = listViewModel;
            DetailViewModel = detailViewModel;
            CreateOrUpdateViewModel = createOrUpdateViewModel;

            CreateObjectRequestCommand = new AsyncDelegateCommand(CreateObjectRequestHandler, exceptionHandler: RaiseExceptionOccured);
            RefreshCommand = new AsyncDelegateCommand(RefreshPage, exceptionHandler: RaiseExceptionOccured);
        }

        protected async override Task InitializeControl()
        {
            var initTask = Initialize();

            ListViewModel.SelectedObjectChanged += SelectedObjectChangedHandler;
            DetailViewModel.EditObjectRequest += EditObjectRequestHandler;
            DetailViewModel.DeleteObjectRequest += DeleteObjectRequestHandler;
            CreateOrUpdateViewModel.DeleteObjectRequest += DeleteObjectRequestHandler;
            CreateOrUpdateViewModel.ObjectPersistedEvent += ObjectPersistedHandler;
            CreateOrUpdateViewModel.CancelEditRequest += CancelEditRequestHandler;          

            await initTask;
            await RefreshPage();
        }

        private async Task RefreshPage()
        {
            var objects = await LoadObjects();
            ListViewModel.Objects = objects.HasValue() ? new ObservableCollection<TObject>(objects) : new ObservableCollection<TObject>();

            ResetSelection();
        }

        private void ResetSelection()
        {
            ListViewModel.SelectedObject = default;
            DetailViewModel.DetailObject = default;
            CreateOrUpdateViewModel.Object = default;
            CurrentControl = null;
        }

        // Events Handlers
        private Task CreateObjectRequestHandler()
        {
            try
            {
                var task = CreateOrUpdateViewModel.SetupForCreate();
                CurrentControl = CreateOrUpdateViewModel;
                return task;
            }
            catch(Exception ex)
            {
                RaiseExceptionOccured(ex);
            }
            return Task.CompletedTask;
        }
        protected void SelectedObjectChangedHandler(TObject objectChanged)
        {
            try
            {
                if(objectChanged != null)
                {
                    DetailViewModel.DetailObject = objectChanged;
                    CurrentControl = DetailViewModel;
                }             
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }
        }
        protected async void EditObjectRequestHandler(TObject objectToEdit)
        {
            try
            {
                objectToEdit.ValidateVariable(nameof(objectToEdit));

                await CreateOrUpdateViewModel.SetupForUpdate(objectToEdit);
                CurrentControl = CreateOrUpdateViewModel;
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }
        }
        protected void ObjectPersistedHandler(bool wasCreate, TObject oldObjectPersisted, TObject newObject)
        {
            try
            {
                oldObjectPersisted.ValidateVariable(nameof(oldObjectPersisted));
                newObject.ValidateVariable(nameof(newObject));

                if (!wasCreate)
                {
                    ListViewModel.Objects.Remove(oldObjectPersisted);
                }

                ListViewModel.Objects.Add(newObject);

                SelectedObjectChangedHandler(newObject);
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }
        }
        protected async void CancelEditRequestHandler(bool wasEdit, TObject objectEditCanceled)
        {
            try
            {
                ResetSelection();
                if (wasEdit)
                    await CancelEdit(objectEditCanceled);                
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }          
       }
        protected async void DeleteObjectRequestHandler(TObject objectEditCanceled)
        {
            try
            {              
                var deleted = await DeleteObject(objectEditCanceled);

                if (deleted)
                {
                    CurrentControl = null;
                }
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }
        }

        // Virtuals
        /// <summary>
        /// Used to cancel the editing of the supplied object.
        /// </summary>
        /// <param name="objectEditCanceled">Object that was cancelled editing</param>
        /// <returns></returns>
        protected virtual async Task CancelEdit(TObject objectEditCanceled) {
            try
            {
                var objectTask = RefreshObject(objectEditCanceled);

                // Removed canceled edit object
                ListViewModel.Objects.Remove(objectEditCanceled);

                // Get the object so we omit any changes
                var refreshedObject = await objectTask;

                // Add back to list
                ListViewModel.Objects.Add(refreshedObject);

                // Setup detail viewmodel and change current view
                DetailViewModel.DetailObject = refreshedObject;
                CurrentControl = DetailViewModel;
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }            
        }

        // Abstractions
        /// <summary>
        /// Code that runs when control gets rendered
        /// </summary>
        /// <returns></returns>
        protected abstract Task Initialize();
        /// <summary>
        /// Method that should load the Objects when the page loads
        /// </summary>
        /// <returns></returns>
        protected abstract Task<IEnumerable<TObject>> LoadObjects();
        /// <summary>
        /// Used to refresh any changes done to object
        /// </summary>
        /// <param name="objectToRefresh">Object that needs to be refreshed</param>
        /// <returns></returns>
        protected abstract Task<TObject> RefreshObject(TObject objectToRefresh);
        /// <summary>
        /// Used to delete the supplied object
        /// </summary>
        /// <param name="objectToDelete">Object to delete</param>
        /// <returns>Was object deleted</returns>
        protected abstract Task<bool> DeleteObject(TObject objectToDelete);

        public override void Dispose()
        {
            base.Dispose();

            ListViewModel?.Dispose();
            DetailViewModel?.Dispose();
            CreateOrUpdateViewModel?.Dispose();
        }
    }
}
