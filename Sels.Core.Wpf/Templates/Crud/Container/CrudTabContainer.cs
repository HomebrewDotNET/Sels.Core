using Sels.Core.Wpf.Components.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Wpf.Templates.Crud
{
    public abstract class CrudTabContainer<TObject, TValidationError, TListViewModel, TDetailViewModel, TCreateOrUpdateViewModel> : CrudContainer<TObject, TValidationError, TListViewModel, TDetailViewModel, TCreateOrUpdateViewModel> where TListViewModel : CrudList<TObject> where TDetailViewModel : CrudDetail<TObject> where TCreateOrUpdateViewModel : CrudCreateOrEdit<TObject, TValidationError> where TObject : new()
    {
        // Properties
        public bool TabsVisible => IsCreateView || IsTabView;
        public bool IsCreateView {
            get
            {
                return GetValue<bool>(nameof(IsCreateView));
            }
            set
            {
                SetValue(nameof(IsCreateView), value, affectedProperties: nameof(TabsVisible));
            }
        }
        public bool IsTabView {
            get
            {
                return GetValue<bool>(nameof(IsTabView));
            }
            set
            {
                SetValue(nameof(IsTabView), value, affectedProperties: nameof(TabsVisible));
            }
        }
        public int CurrentTabIndex {
            get
            {
                return GetValue<int>(nameof(CurrentTabIndex));
            }
            set
            {
                SetValue(nameof(CurrentTabIndex), value, (x,y) => TabIndexChanged(x, CurrentTabIndex));
            }
        }

        public CrudTabContainer(TListViewModel listViewModel, TDetailViewModel detailViewModel, TCreateOrUpdateViewModel createOrUpdateViewModel) : base(listViewModel, detailViewModel, createOrUpdateViewModel)
        {
            SubscribeToPropertyChanged<BaseViewModel>(nameof(CurrentControl), CurrentControlChanged);
        }

        // Virtuals
        protected virtual void TabIndexChanged(bool wasDifferent, int currentIndex)
        {
            try
            {
                if (wasDifferent && currentIndex == UpdateTabIndex)
                {
                    // Manually set update tab content
                    DetailViewModel.EditObjectCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }          
        }

        // Overrides
        protected void CurrentControlChanged(bool wasChanged, BaseViewModel viewModel)
        {
            try
            {
                if (viewModel == null)
                {
                    // Show nothing
                    IsCreateView = false;
                    IsTabView = false;
                }
                else if (viewModel is TCreateOrUpdateViewModel managerViewModel)
                {
                    if (managerViewModel.IsEdit == false)
                    {
                        // Show Create Tab
                        IsCreateView = true;
                        IsTabView = false;
                        CurrentTabIndex = CreateTabIndex;
                    }
                    else
                    {
                        // Show Update Tab
                        IsCreateView = false;
                        IsTabView = true;
                        CurrentTabIndex = UpdateTabIndex;
                    }
                }
                else
                {
                    // Show detail tab
                    IsCreateView = false;
                    IsTabView = true;
                    CurrentTabIndex = DetailTabIndex;
                }

                RaisePropertyChanged(nameof(TabsVisible));
            }
            catch (Exception ex)
            {
                RaiseExceptionOccured(ex);
            }            
        }

        // Abstractions
        protected abstract int DetailTabIndex { get; }
        protected abstract int CreateTabIndex { get; }
        protected abstract int UpdateTabIndex { get; }
    }
}
