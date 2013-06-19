using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Net.Mail;
using System.Net;
using BoginyaJournal.Entities;
using System.Configuration;
using System.Collections.Specialized;

namespace BoginyaJournal
{
    #region Behaviours
    public class WindowClosingBehavior
    {
        public static ICommand GetClosed(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ClosedProperty);
        }

        public static void SetClosed(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ClosedProperty, value);
        }

        public static readonly DependencyProperty ClosedProperty
            = DependencyProperty.RegisterAttached(
            "Closed", typeof(ICommand), typeof(WindowClosingBehavior),
            new UIPropertyMetadata(new PropertyChangedCallback(ClosedChanged)));

        private static void ClosedChanged(
          DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Window window = target as Window;

            if (window != null)
            {
                if (e.NewValue != null)
                {
                    window.Closed += Window_Closed;
                }
                else
                {
                    window.Closed -= Window_Closed;
                }
            }
        }

        public static ICommand GetClosing(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ClosingProperty);
        }

        public static void SetClosing(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ClosingProperty, value);
        }

        public static readonly DependencyProperty ClosingProperty
            = DependencyProperty.RegisterAttached(
            "Closing", typeof(ICommand), typeof(WindowClosingBehavior),
            new UIPropertyMetadata(new PropertyChangedCallback(ClosingChanged)));

        private static void ClosingChanged(
          DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Window window = target as Window;

            if (window != null)
            {
                if (e.NewValue != null)
                {
                    window.Closing += Window_Closing;
                }
                else
                {
                    window.Closing -= Window_Closing;
                }
            }
        }

        public static ICommand GetCancelClosing(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CancelClosingProperty);
        }

        public static void SetCancelClosing(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CancelClosingProperty, value);
        }

        public static readonly DependencyProperty CancelClosingProperty
            = DependencyProperty.RegisterAttached(
            "CancelClosing", typeof(ICommand), typeof(WindowClosingBehavior));

        static void Window_Closed(object sender, EventArgs e)
        {
            ICommand closed = GetClosed(sender as Window);
            if (closed != null)
            {
                closed.Execute(null);
            }
        }

        static void Window_Closing(object sender, CancelEventArgs e)
        {
            ICommand closing = GetClosing(sender as Window);
            if (closing != null)
            {
                if (closing.CanExecute(null))
                {
                    closing.Execute(null);
                }
                else
                {
                    ICommand cancelClosing = GetCancelClosing(sender as Window);
                    if (cancelClosing != null)
                    {
                        cancelClosing.Execute(null);
                    }

                    e.Cancel = true;
                }
            }
        }
    }
    #endregion

    public class Utils
    {
        //get datetime from Internet
        public static DateTime GetNISTDate(bool convertToLocalTime)
        {
            return DateTime.Now.Date;
            //Random ran = new Random(DateTime.Now.Millisecond);
            //DateTime date = DateTime.MinValue;
            //string serverResponse = string.Empty;

            //// Represents the list of NIST servers
            //string[] servers = new string[] {
            //             "64.90.182.55",
            //             "206.246.118.250",
            //             "207.200.81.113",
            //             "128.138.188.172",
            //             "64.113.32.5",
            //             "64.147.116.229",
            //             "64.125.78.85",
            //             "128.138.188.172"
            //              };

            //// Try each server in random order to avoid blocked requests due to too frequent request
            //for (int i = 0; i < 5; i++)
            //{
            //    try
            //    {
            //        // Open a StreamReader to a random time server
            //        StreamReader reader = new StreamReader(new System.Net.Sockets.TcpClient(servers[ran.Next(0, servers.Length)], 13).GetStream());
            //        serverResponse = reader.ReadToEnd();
            //        reader.Close();

            //        // Check to see that the signiture is there
            //        if (serverResponse.Length > 47 && serverResponse.Substring(38, 9).Equals("UTC(NIST)"))
            //        {
            //            // Parse the date
            //            int jd = int.Parse(serverResponse.Substring(1, 5));
            //            int yr = int.Parse(serverResponse.Substring(7, 2));
            //            int mo = int.Parse(serverResponse.Substring(10, 2));
            //            int dy = int.Parse(serverResponse.Substring(13, 2));
            //            int hr = int.Parse(serverResponse.Substring(16, 2));
            //            int mm = int.Parse(serverResponse.Substring(19, 2));
            //            int sc = int.Parse(serverResponse.Substring(22, 2));

            //            if (jd > 51544)
            //                yr += 2000;
            //            else
            //                yr += 1999;

            //            date = new DateTime(yr, mo, dy, hr, mm, sc);

            //            // Convert it to the current timezone if desired
            //            if (convertToLocalTime)
            //                date = date.ToLocalTime();

            //            // Exit the loop
            //            break;
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        /* Do Nothing...try the next server */
            //    }
            //}

            //return (date == DateTime.MinValue)?DateTime.Now:date;
        }

        public static void SendMail(DateTime date)
        {

            var fromAddress = new MailAddress("info@boginya-salon.com", "Boginya-Journal");
            var toAddress = new MailAddress("victor.hytyk@gmail.com", "Vic");
            const string fromPassword = "manavhat";
            string subject = "Продажі за " + date.ToString("dd.MM.yyyy");
            string body = string.Format(@"за {0} продалося:<br /><br />
                                    <table border=1>
                                        <tr>
                                            <td>Код(ід)</td>
                                            <td>Назва</td>
                                            <td>К-сть</td>
                                            <td>Ціна продаж.</td>
                                            <td>Ціна</td>
                                            <td>Продавець</td>
                                        </tr>
                                        {1}
                                    </table>",
                date.ToString("dd.MM.yyyy"),GetInfoPerDate(date));

            var smtp = new SmtpClient
                       {
                           Host = "smtp.gmail.com",
                           Port = 587,
                           EnableSsl = true,
                           DeliveryMethod = SmtpDeliveryMethod.Network,
                           UseDefaultCredentials = false,
                           Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                       };
            using (var message = new MailMessage(fromAddress, toAddress)
                                 {
                                     IsBodyHtml = true,
                                     Subject = subject,
                                     Body = body
                                 })
            {
                smtp.Send(message);
            }
        }
        public static string GetInfoPerDate(DateTime date)
        {
            string result = string.Empty;
            JournalViewModel viewModel = new JournalViewModel();
            viewModel.CurrentDate = date.Date;
            List<Journal> list = viewModel.JournalList;
            list.ForEach(item => {
                result += string.Format("<tr><td>{0}({1})</td> <td>{2}</td> <td>{3}</td> <td>{4}</td> <td>{5}</td><td>{6}</td></tr>", 
                    item.Tovar.Kod,item.Tovar.ID_Tovar, item.Tovar.NameTovar, item.Amount, item.Tovar.CinaProdazh, item.Price,item.User.Login);
            });
            result += string.Format("<tr><td colspan=6 align=right>Всього: {0}</tr>",list.Sum(j=>j.Amount*j.Price));
            return result;
        }
        static Configuration config = null;
        public static Configuration Config
        {
            get
            {
                if (null == config)
                {
                    string configFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BoginyaJournal\\boginyajournal.exe.config";

                    if (File.Exists(configFileName))
                    {
                        ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                        map.ExeConfigFilename = configFileName;
                        config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


                    }
                    else
                    {
                        string dir = Path.GetDirectoryName(configFileName);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.Copy("boginyajournal.exe.config", configFileName);
                        config = ConfigurationManager.OpenExeConfiguration("boginyajournal.exe");
                    }
                }
                return config;
            }
        }
    }


    public static class BJExtensions
    {
        public static DateTime GetFirstDayOfMonth(this DateTime value)
        {
            DateTime result = new DateTime(value.Year, value.Month, 1);

            return result;
        }
        public static DateTime GetLastDayOfMonth(this DateTime value)
        {
            DateTime result = value.AddMonths(1).GetFirstDayOfMonth().AddDays(-1);

            return result;
        }
    }



    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameters)
        {
            return _canExecute == null ? true : _canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            _execute(parameters);
        }

        #endregion // ICommand Members
    }
}
