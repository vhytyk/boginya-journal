using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BoginyaJournal.Entities;
using System.Collections.ObjectModel;
using BLToolkit.Data;
using BLToolkit.Data.Sql;
using BLToolkit.DataAccess;
using BLToolkit.EditableObjects;
using System.ComponentModel;
using BoginyaJournal.JournalViews;
using System.Reflection;
using System.Deployment.Application;

namespace BoginyaJournal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class JournalView : Window
    {
        public JournalView()
        {
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            //this.DataContext = new JournalViewModel();
            JournalViewModel vm = new JournalViewModel();
            
            this.DataContext = vm;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }


    }

    public class JournalViewModel : INotifyPropertyChanged
    {
        public JournalViewModel()
        {
            CurrentDate = Utils.GetNISTDate(true);
        }
        public string Title
        {
            get
            {
                string version = string.Empty;
                if (ApplicationDeployment.IsNetworkDeployed)
                    version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                return "Журнал v." + version;
            }
        }
        public User CurrentUser
        {
            get
            {
                return User.CurrentUser;
            }
        }
        DateTime currentDate = DateTime.MinValue;
        public DateTime CurrentDate
        {
            get {
                return currentDate;
            }
            set
            {
                currentDate = value;
                NotifyAllPropertiesChanged();
            }
        }
        Journal selectedItem = null;
        public Journal SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                PropertyChange("CanRemoveItem");
            }
        }
        public bool CanRemoveItem
        {
            get
            {
                return User.CurrentUser.Rights == Rights.Admin && SelectedItem != null;
            }
        }
        public bool CanAddItem
        {
            get
            {
                return currentDate == Utils.GetNISTDate(true) || User.CurrentUser.Rights == Rights.Admin;
            }
        }
        public decimal DaySum
        {
            get
            {
                return JournalList.Sum(journal => (journal.Tovar != null)?journal.Amount * journal.Price:0);
            }
        }
        public decimal InKasaSum
        {
            get
            {
                using(DbManager db = new DbManager())
                {
                    db.SetCommand("select sum(Amount*Price) from Journal where ID_Tovar>0");
                    decimal sumTovar = db.ExecuteScalar<decimal>();
                    db.SetCommand("select sum(Amount*Price) from Journal where ID_Partner>0");
                    decimal sumReduce = db.ExecuteScalar<decimal>();
                    return sumTovar - sumReduce;
                }
            }
        }
        public decimal InKasaSumDay
        {
            get
            {
                using (DbManager db = new DbManager())
                {
                    try
                    {
                        db.SetCommand(string.Format("select sum(Amount*Price) from Journal where ID_Tovar>0 and SaleDate<'{0}'", CurrentDate.Date.AddDays(1).ToString("dd.MM.yyyy")));
                        decimal sumTovar = db.ExecuteScalar<decimal>();
                        db.SetCommand(string.Format("select sum(Amount*Price) from Journal where ID_Partner>0 and SaleDate<'{0}'", CurrentDate.Date.AddDays(1).ToString("dd.MM.yyyy")));
                        decimal sumReduce = db.ExecuteScalar<decimal>();
                        return sumTovar - sumReduce;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }
        }
        public List<Journal> JournalList
        {
            get
            {
                try
                {
                    using(DbManager db = new DbManager())
                    {
                        var query = from j in db.GetTable<Journal>()
                                    where j.SaleDate.Date == currentDate.Date
                                    select
                                        new Journal
                                        {
                                            ID = j.ID,
                                            ID_Tovar = j.ID_Tovar,
                                            ID_User = j.ID_User,
                                            ID_Partner = j.ID_Partner,
                                            Tovar = j.Tovar,
                                            Partner = j.Partner,
                                            User = j.User,
                                            SaleDate = j.SaleDate,
                                            Amount = j.Amount,
                                            Price = j.Price,
                                            Discount = j.Discount,
                                            Comment = j.Comment
                                        };
                        var list = query.ToList();
                        return list;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public bool CanAddReduce
        {
            get
            {
                return SelectedReduce != null;
            }
        }
        Partner selectedReduce = null;
        public Partner SelectedReduce
        {
            get
            {
                return selectedReduce;
            }
            set
            {
                selectedReduce = value;
                PropertyChange("CanAddReduce");
            }
        }
        public List<Partner> ReduceItems
        {
            get
            {

                using (DbManager db = new DbManager())
                {
                    var query = from p in db.GetTable<Partner>()
                                where p.PartnerType == PartnerType.Reduce
                                orderby p.NamePartner
                                select p;
                    return query.ToList();
                }
            }
        }
        ICommand addReduceCommand = null;
        public ICommand AddReduceCommand
        {
            get
            {
                if (null == addReduceCommand)
                {
                    addReduceCommand = new RelayCommand(param => this.AddReduceItem());
                }
                return addReduceCommand;
            }
        }

        ICommand addJournalItemCommand = null;
        public ICommand AddJournalItemCommand
        {
            get
            {
                if (null == addJournalItemCommand)
                {
                    addJournalItemCommand = new RelayCommand(param => this.AddJournalItem());
                }
                return addJournalItemCommand;
            }
        }
        ICommand removeJournalItemCommand = null;
        public ICommand RemoveJournalItemCommand
        {
            get
            {
                if (null == removeJournalItemCommand)
                {
                    removeJournalItemCommand = new RelayCommand(param => this.RemoveJournalItem());
                }
                return removeJournalItemCommand;
            }
        }
        ICommand changeUserCommand = null;
        public ICommand ChangeUserCommand
        {
            get
            {
                if (null == changeUserCommand)
                {
                    changeUserCommand = new RelayCommand(param => this.ChangeUser());
                }
                return changeUserCommand;
            }
        }
        ICommand showMonthSumCommand = null;
        public ICommand ShowMonthSumCommand
        {
            get
            {
                if (null == showMonthSumCommand && User.CurrentUser.IsAdmin)
                {
                    showMonthSumCommand = new RelayCommand(param => {
                        using (DbManager db = new DbManager())
                        {

                            var sumOfMonth = from journal in db.GetTable<Journal>()
                                             where journal.SaleDate >= CurrentDate.GetFirstDayOfMonth() && journal.SaleDate < CurrentDate.GetLastDayOfMonth().AddDays(1) && journal.ID_Partner == 0
                                             select journal;
                            decimal sum = sumOfMonth.Sum(journal => journal.Price * journal.Amount);
                            MessageBox.Show(sum.ToString());
                        }
                    });
                }
                return showMonthSumCommand;
            }
        }

        ICommand showMonthRevenueCommand = null;
        public ICommand ShowMonthRevenueCommand
        {
            get
            {
                if (null == showMonthRevenueCommand && User.CurrentUser.IsAdmin)
                {
                    showMonthRevenueCommand = new RelayCommand(param =>
                    {
                        using (DbManager db = new DbManager())
                        {

                            var RevenueOfMonth = from journal in db.GetTable<Journal>()
                                             where journal.SaleDate >= CurrentDate.GetFirstDayOfMonth() && journal.SaleDate < CurrentDate.AddMonths(1).GetFirstDayOfMonth() && journal.ID_Partner == 0
                                             select (journal.Price-journal.Tovar.CinaZakup)*journal.Amount;
                            decimal sum = RevenueOfMonth.Sum();
                            MessageBox.Show(sum.ToString());
                        }
                    });
                }
                return showMonthRevenueCommand;
            }
        }

        ICommand refreshAllCommand = null;
        public ICommand RefreshAllCommand
        {
            get
            {
                if (null == refreshAllCommand)
                {
                    refreshAllCommand = new RelayCommand(param => NotifyAllPropertiesChanged());
                }
                return refreshAllCommand;
            }
        }

        ICommand editItemCommand = null;
        public ICommand EditItemCommand
        {
            get
            {
                if (null == editItemCommand)
                {
                    editItemCommand = new RelayCommand(param =>
                    {
                        if (null != selectedItem && User.CurrentUser.IsAdmin)
                        {
                            JournalItemView view = new JournalItemView();
                            view.DataContext = new JournalItemViewModel() { Item = selectedItem };
                            if (view.ShowDialog() == true)
                            {
                                using (DbManager db = new DbManager())
                                {
                                    new SqlQuery<Journal>(db).Update(selectedItem);
                                }
                                PropertyChange("DaySum");
                                PropertyChange("InKasaSum");
                            }
                        }
                    });
                }
                return editItemCommand;
            }
        }



        private void ChangeUser()
        {
            Login newLogin = new Login();
            newLogin.DataContext = new LoginViewModel(true);
            newLogin.ShowDialog();
            NotifyAllPropertiesChanged();
        }

        public void RemoveJournalItem()
        {
            if (null != selectedItem && 
                MessageBox.Show(string.Format("Ви впевнені що хочете вилучити позицію \"{0}\"?",selectedItem.Name), "вилучення", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (DbManager db = new DbManager())
                {
                    db.BeginTransaction();
                    try
                    {
                        if (selectedItem.Tovar != null)
                        {
                            //add ostatok in tovar
                            selectedItem.Tovar.Ostatok += (int)selectedItem.Amount;
                            SqlQuery<Tovar> query2 = new SqlQuery<Tovar>(db);
                            query2.Update(selectedItem.Tovar);
                        }
                        //insert new journal item
                        SqlQuery<Journal> query = new SqlQuery<Journal>(db);
                        query.Delete(selectedItem);
                        db.CommitTransaction();
                    }
                    catch
                    {
                        db.RollbackTransaction();
                        throw;
                    }


                }
                PropertyChange("JournalList");
                PropertyChange("DaySum");
                PropertyChange("InKasaSum");
            }
        }
        public void AddJournalItem()
        {
            if (!CanAddItem)
                return;
            FindWindow findWindow = new FindWindow();
            FindViewModel viewModel = new FindViewModel();
            //viewModel.SelectedTovar = viewModel.TovarList[0];
            findWindow.DataContext = viewModel;
            if (findWindow.ShowDialog() == true)
            {
                if (viewModel.SelectedTovar != null)
                {
                    Journal newJournal = new Journal()
                    {
                        Amount = 1,
                        Discount = 0,
                        ID_Tovar = viewModel.SelectedTovar.ID_Tovar,
                        Tovar = viewModel.SelectedTovar,
                        User = User.CurrentUser,
                        ID_User = User.CurrentUser.ID_User,
                        Price = viewModel.SelectedTovar.CinaProdazh,
                        SaleDate = CurrentDate
                    };
                    JournalItemView view = new JournalItemView();
                    view.DataContext = new JournalItemViewModel() { Item = newJournal };
                    if (view.ShowDialog() == true)
                    {
                        using (DbManager db = new DbManager())
                        {
                            db.BeginTransaction();
                            try
                            {
                                //inser new journal item
                                SqlQuery<Journal> query = new SqlQuery<Journal>(db);
                                query.Insert(newJournal);

                                //reduce ostatok in tovar
                                newJournal.Tovar.Ostatok -= (int)newJournal.Amount;
                                SqlQuery<Tovar> query2 = new SqlQuery<Tovar>(db);
                                query2.Update(newJournal.Tovar);
                                db.CommitTransaction();
                            }
                            catch {
                                db.RollbackTransaction();
                                throw;
                            }
                            

                        }
                        PropertyChange("JournalList");
                        PropertyChange("DaySum");
                        PropertyChange("InKasaSum");


                    }
                }
                    
            }

        }

        public void AddReduceItem()
        {
            if (selectedReduce != null)
            {
                Journal newJournal = new Journal()
                {
                    Amount = 1,
                    Discount = 0,
                    ID_Partner = selectedReduce.ID_Partner,
                    Partner = selectedReduce,
                    User = User.CurrentUser,
                    ID_User = User.CurrentUser.ID_User,
                    Price = 0,
                    SaleDate = CurrentDate
                };

                JournalItemView view = new JournalItemView();
                view.DataContext = new JournalItemViewModel() { Item = newJournal };
                if (view.ShowDialog() == true)
                {
                    using (DbManager db = new DbManager())
                    {
                        db.BeginTransaction();
                        try
                        {
                            //inser new journal item
                            SqlQuery<Journal> query = new SqlQuery<Journal>(db);
                            query.Insert(newJournal);
                            db.CommitTransaction();
                        }
                        catch
                        {
                            db.RollbackTransaction();
                            throw;
                        }


                    }
                    PropertyChange("JournalList");
                    PropertyChange("DaySum");
                    PropertyChange("InKasaSum");

                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void PropertyChange(string property)
        { 
            if(null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        protected void NotifyAllPropertiesChanged()
        {
            Type t = this.GetType();
            PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo info in properties)
            {
                PropertyChange(info.Name);
            }
        }
        #endregion
    }
    
}
