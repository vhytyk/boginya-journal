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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BLToolkit.Data;
using BoginyaJournal.Entities;
using BLToolkit.DataAccess;
using FirebirdSql.Data.FirebirdClient;
using System.ComponentModel;

namespace BoginyaJournal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        public FindWindow()
        {
            InitializeComponent();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            txtFilter.Focus();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as FindViewModel).ApplyFilter((sender as TextBox).Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }

    public class FindViewModel : INotifyPropertyChanged
    {
        Tovar selectedTovar = null;
        public Tovar SelectedTovar
        {
            get
            {
                return selectedTovar;
            }
            set
            {
                selectedTovar = value;
                if(null != PropertyChanged)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedTovar"));
            }
        }

        public void ApplyFilter(string Filter)
        {
            if (null != Filter && Filter.Trim() != string.Empty)
            {
                string[] words = Filter.Trim().ToUpper().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                Predicate<Tovar> findPredicate =
                    (Tovar t) =>
                    {
                        Ean13Barcode2005.Ean13 ean = new Ean13Barcode2005.Ean13("0", t.ID_Tovar.ToString());
                        string searchSource = t.Kod.ToString() + "|" + t.NameTovar.ToUpper() + "|" + t.Group.Name.ToUpper() + "|" +
                            t.CinaProdazh.ToString() + "|" + t.Partner.NamePartner.ToUpper() + "|" + t.ID_Tovar.ToString().PadLeft(12, '0') + ean.ChecksumDigit;
                        foreach (string w in words)
                            if (!searchSource.Contains(w))
                                return false;
                        return true;
                    };
                currentTovarList = allTovarList.FindAll(findPredicate);

            }
            else
                currentTovarList = new List<Tovar>(allTovarList);
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs("TovarList"));
            if (currentTovarList != null && currentTovarList.Count > 0)
                SelectedTovar = currentTovarList[0];

        }


        List<Tovar> allTovarList = null;
        List<Tovar> currentTovarList = null;
        public List<Tovar> TovarList
        {
            get
            {
                if (allTovarList == null)
                {
                    try
                    {
                        using (DbManager db = new DbManager())
                        {
                            var query = from t in db.GetTable<Tovar>()
                                        select
                                            new Tovar
                                            {
                                                ID_Tovar = t.ID_Tovar,
                                                ID_Partner = t.ID_Partner,
                                                ID_Group = t.ID_Group,
                                                Partner = t.Partner,
                                                Group = t.Group,
                                                BarCodeCount = t.BarCodeCount,
                                                CinaProdazh = t.CinaProdazh,
                                                CinaZakup = t.CinaZakup,
                                                Kod = t.Kod,
                                                Ostatok = t.Ostatok,
                                                NameTovarInternal = t.NameTovarInternal
                                            };
                            allTovarList = query.ToList();
                        }
                        currentTovarList = new List<Tovar>(allTovarList);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return currentTovarList;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}
