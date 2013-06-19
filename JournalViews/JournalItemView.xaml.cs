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
using BLToolkit.EditableObjects;
using BoginyaJournal.Entities;

namespace BoginyaJournal.JournalViews
{
    /// <summary>
    /// Interaction logic for JournalItemView.xaml
    /// </summary>
    public partial class JournalItemView : Window
    {
        public JournalItemView()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Validation.GetHasError(txtPrice))
                MessageBox.Show("ціна задана невірно!");
            else if (Validation.GetHasError(txtAmount))
                MessageBox.Show("кількість задана невірно!");
            else
                DialogResult = true;
        }
    }

    public class JournalItemViewModel : EditableObject<JournalItemViewModel>
    {
        public Journal Item { get; set; }
        public bool CanEditAmount
        {
            get
            {
                return Item.Tovar != null;
            }
        }
        public bool CanEditIsRent
        {
            get { return Item.ID <= 0; }
        }
    }

}
