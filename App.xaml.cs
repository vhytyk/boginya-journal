using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using BoginyaJournal.Entities;
using BLToolkit.DataAccess;
using System.Threading;
using System.Windows.Controls;
using BLToolkit.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Windows.Input;

namespace BoginyaJournal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.GotFocusEvent,
                new RoutedEventHandler(TextBox_GotFocus));
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.GotMouseCaptureEvent,
                new RoutedEventHandler(TextBox_GotMouseCapture));

            base.OnStartup(e);
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                App.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    new SettingsWindow().ShowDialog();
                    
                }
                string connectionString = string.Format(@"DataSource={0};Database=d:\ProfStor\SKLAD.GDB;User=SYSDBA;Password=masterkey;Dialect=3;Charset=win1251", Utils.Config.AppSettings.Settings["Server"].Value);
                if(!ApplicationDeployment.IsNetworkDeployed)
                    connectionString = string.Format(@"DataBase=D:\Projects\Journal\BoginyaJournal_MAIN\BoginyaJournal\BoginyaJournal\bin\Debug\SKLAD.GDB;User=SYSDBA;Password=masterkey;Dialect=3;Charset=win1251");
                BLToolkit.Data.DbManager.AddConnectionString("Fdp", "Sklad", connectionString);
                BLToolkit.Data.DbManager.AddDataProvider(new BLToolkit.Data.DataProvider.FdpDataProvider());
                //checkin connection
                using (DbManager db = new DbManager())
                {
                    db.SetCommand("select * from users");
                    db.ExecuteNonQuery();
                }
                DbMigration.DBMigration.Migrate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            

            //sending mail
            new Thread(new ThreadStart(()=> 
                {
                    for (var i = 0; i < 3; i++)
                    {
                        try
                        {
                            //Utils.SendMail(Utils.GetNISTDate(true).AddDays(-1));
                            break;
                        }
                        catch{
                            Thread.Sleep(10000);
                        }
                    }
                }
                )).Start();
            Login login = new Login();
            login.ShowDialog();
            if (login.DialogResult == null || login.DialogResult == false)
            {
                Application.Current.Shutdown();
            }
            

        }
        void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
        void TextBox_GotMouseCapture(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
