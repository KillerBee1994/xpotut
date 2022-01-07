using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using XpoTutorial;
using System.Linq;
using WpfApplicationMvvm.Services;
using System.Windows.Input;

namespace WpfApplicationMvvm.ViewModels {
    public class CustomerListViewModel :ViewModelBase {


        protected EditCustomerViewModel DialogViewModel { get; private set; }
        public ICommand ShowMessageBoxCommand { get; private set; }
        public ICommand ShowDialogCommand { get; private set; }

        protected IDialogService DialogService { get { return GetService<IDialogService>(); } }
        protected IMessageBoxService MessageService { get { return GetService<IMessageBoxService>(); } }

        public CustomerListViewModel() {
            ShowMessageBoxCommand = new DelegateCommand(OnShowMessageBoxCommandExecute);
            ShowDialogCommand = new DelegateCommand(OnShowDialogCommandExecute);
            DialogViewModel = new EditCustomerViewModel();
        }
        void OnShowMessageBoxCommandExecute() {
            MessageService.Show("This is a message box", "DXMessageBoxService", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        void OnShowDialogCommandExecute() {
            UICommand resCommand = DialogService.ShowDialog(DialogViewModel.DialogCommands, "DialogWindow", DialogViewModel);
            if(resCommand != null) {
                MessageService.Show("Dialog Clicked Button: " + resCommand.Caption);
            }
        }
        public IListSource Customers {
            get { return GetProperty(() => Customers);  }
            private set { SetProperty(() => Customers, value); }
        }
        protected IDocumentManagerService DocumentManagerService {
            get { return GetService<IDocumentManagerService>(); }
        }
        protected IInstantFeedbackService InstantFeedbackService {
            get { return GetService<IInstantFeedbackService>(); }
        }
        protected override void OnInitializeInRuntime() {
            base.OnInitializeInRuntime();
            Reload();
        }
        protected void CreateDocument(int arg) {
            IDocument doc = DocumentManagerService.FindDocument(arg, this);
            if (doc == null) {
                doc = DocumentManagerService.CreateDocument("EditCustomerView", arg, this);
                doc.Id = string.Format("DocId_{0}", DocumentManagerService.Documents.Count());
                doc.Title = string.Format("Edit Customer {0}", arg);
            }
            doc.Show();
        }
        [Command]
        public void Reload() {
            if (InstantFeedbackService == null)
                ReloadCore();
            else {
                object focusedObjectKey = InstantFeedbackService.GetFocusedRowKey();
                ReloadCore();
                InstantFeedbackService.SetFocusedRowByKeyAsync(focusedObjectKey);
            }
        }
        [Command]
        public async void Delete() {
            using (UnitOfWork uow = new UnitOfWork()) {
                object key = InstantFeedbackService.GetFocusedRowKey();
                Customer customer = uow.GetObjectByKey<Customer>(key);
                uow.Delete(customer);
                await uow.CommitChangesAsync();
            }
            Reload();
        }
        [Command]
        public void Edit() {
            object key = InstantFeedbackService.GetFocusedRowKey();
            CreateDocument((int)key);
        }
        [Command]
        public void Add() {
            CreateDocument(-1);
        }
        private void ReloadCore() {
            IDisposable old = (IDisposable)Customers;
            Customers = new XPInstantFeedbackSource(typeof(Customer), "Oid;ContactName", null,
                e => e.Session = new Session(),
                e => e.Session.Session.Dispose());
            if (old != null) {
                old.Dispose();
            }
        }
    }
}
