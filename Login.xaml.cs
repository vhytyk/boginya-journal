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
using System.ComponentModel;
using System.Reflection;
using System.Deployment.Application;
using System.IO;

namespace BoginyaJournal
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            string s = System.Configuration.ConfigurationManager.AppSettings["Server"];

            if (this.DataContext == null)
                this.DataContext = new LoginViewModel();
        }

    }

    public class LoginViewModel : INotifyPropertyChanged
    {
        public LoginViewModel()
        {
            if (Utils.Config.AppSettings.Settings["Login"] != null)
            {
                BoginyaJournal.Entities.User user = UserList.Find(param => param.Login == Utils.Config.AppSettings.Settings["Login"].Value);
                if (null != user)
                {
                    Selected = user;
                    Password = Utils.Config.AppSettings.Settings["Password"].Value;
                    LogIn(null);
                }
            }
        }
        bool reLogin = false;
        public LoginViewModel(bool _relogin = false)
        {
            this.reLogin = _relogin;
        }
        public string Title
        {
            get
            {
                string version = string.Empty;
                if (ApplicationDeployment.IsNetworkDeployed)
                    version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                return "Журнал v." + version + " - Авторизація ";
            }
        }
        List<BoginyaJournal.Entities.User> userList = null;
        public List<BoginyaJournal.Entities.User> UserList
        {
            get
            {
                if (null == userList)
                    userList = new BLToolkit.DataAccess.SqlQuery<BoginyaJournal.Entities.User>().SelectAll();
                return userList;
            }
        }
        public Entities.User Selected { get; set; }

        public string Password { get; set; }

        bool? dialogResult = null;
        public bool? DialogResult
        {
            get
            {
                return dialogResult;
            }
            set
            {
                dialogResult = value;
                PropertyChange("DialogResult");
            }
        }

        #region LogIn command
        ICommand logIn = null;
        public ICommand LogInCommand
        {
            get
            {
                if (null == logIn)
                {
                    logIn = new RelayCommand(param => this.LogIn(param));
                }
                return logIn;
            }
        }

        private void LogIn(object param)
        {
            if (null != Selected && Selected.Pass == Password.Trim())
            {
                Entities.User.CurrentUser = Selected;
                if (!reLogin)
                {
                    JournalView mainWindow = new JournalView();
                    mainWindow.Show();
                }
                else
                    reLogin = false;

                DialogResult = true;
            }
            else
                MessageBox.Show("Невірні дані авторизації, виберіть користувача і введіть відповідний пароль", "авторизація");
        }
        #endregion

        #region Closing command
        ICommand closingCommand = null;
        public ICommand ClosingCommand
        {
            get
            { 
                if(closingCommand == null)
                    closingCommand = new RelayCommand(param => { }, param => {
                        if (reLogin)
                        {
                            if (MessageBox.Show("Вийти з програми?", "Вихід", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Application.Current.Shutdown();
                                return true;
                            }
                            else
                                return false;
                        }
                        else
                            return true;
                    });
                return closingCommand;
            }
        }
      
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void PropertyChange(string property)
        {
            if (null != PropertyChanged)
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
namespace FunctionalFun.UI
{
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssistant), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached(
            "BindPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false));

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox box = d as PasswordBox;

            // only handle this event when the property is attached to a PasswordBox
            // and when the BindPassword attached property has been set to true
            if (d == null || !GetBindPassword(d))
            {
                return;
            }

            // avoid recursive updating by ignoring the box's changed event
            box.PasswordChanged -= HandlePasswordChanged;

            string newPassword = (string)e.NewValue;

            if (!GetUpdatingPassword(box))
            {
                box.Password = newPassword;
            }

            box.PasswordChanged += HandlePasswordChanged;
        }

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            // when the BindPassword attached property is set on a PasswordBox,
            // start listening to its PasswordChanged event

            PasswordBox box = dp as PasswordBox;

            if (box == null)
            {
                return;
            }

            bool wasBound = (bool)(e.OldValue);
            bool needToBind = (bool)(e.NewValue);

            if (wasBound)
            {
                box.PasswordChanged -= HandlePasswordChanged;
            }

            if (needToBind)
            {
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox box = sender as PasswordBox;

            // set a flag to indicate that we're updating the password
            SetUpdatingPassword(box, true);
            // push the new password into the BoundPassword property
            SetBoundPassword(box, box.Password);
            SetUpdatingPassword(box, false);
        }

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPassword, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPassword);
        }

        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPassword);
        }

        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPassword, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPassword);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPassword, value);
        }
    }
}
namespace ExCastle.Wpf
{
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
                window.DialogResult = e.NewValue as bool?;
        }
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}

