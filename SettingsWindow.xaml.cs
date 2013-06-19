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
using BLToolkit.EditableObjects;
using System.ComponentModel;

namespace BoginyaJournal
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.DataContext = new SettingsViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }

    public class SettingsViewModel  : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            this.Server = Utils.Config.AppSettings.Settings["Server"].Value;
        }

        public string Server { get; set; }


        ICommand saveSettingsCommand = null;
        public ICommand SaveSettingsCommand
        {
            get
            {
                if (null == saveSettingsCommand)
                    saveSettingsCommand = new RelayCommand(param => {
                        try
                        {
                            Utils.Config.AppSettings.Settings["Server"].Value = Server;
                            Utils.Config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });
                return saveSettingsCommand;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
