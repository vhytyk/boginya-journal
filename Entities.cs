using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLToolkit.Mapping;
using BLToolkit.EditableObjects;
using BLToolkit.Data;
using BLToolkit.DataAccess;

namespace BoginyaJournal.Entities
{
    public static class EditableObjectExtensions
    {
        public static List<T> SelectAll<T>(this EditableObject<T> eObject) where T:EditableObject<T>
        {
            return new SqlQuery<T>().SelectAll();
        }
    }

    public class DbVersion
    {
        public int CurrentValue { get; set; }
    }
    public class Tovar
    {
        [PrimaryKey, NonUpdatable, Identity]
        public int ID_Tovar { get; set; }
        public int ID_Partner { get; set; }
        public int ID_Group { get; set; }
        [Association(ThisKey = "ID_Partner", OtherKey = "ID_Partner", CanBeNull = false)]
        public Partner Partner { get; set; }
        [Association(ThisKey = "ID_Group", OtherKey = "ID_Group", CanBeNull = false)]
        public Group Group { get; set; }
        [MapField("NameTovar")]
        public string NameTovarInternal { get; set; }

        string nameTovar = string.Empty;
        [MapIgnore]
        public string NameTovar
        {
            get
            {
                if(nameTovar == string.Empty)
                    nameTovar = NameTovarInternal.Remove(NameTovarInternal.Length-1,1);
                return nameTovar;
            }
        }
        public decimal CinaZakup { get; set; }
        public decimal CinaProdazh { get; set; }
        public int Ostatok { get; set; }
        public int Kod { get; set; }
        public int BarCodeCount { get; set; }

        public override string ToString()
        {
            Ean13Barcode2005.Ean13 ean = new Ean13Barcode2005.Ean13("0", ID_Tovar.ToString());
            return string.Format("Код: {0}\r\nШтрихкод: {3}\r\nНазва: {1}\r\nЦіна: {2:0.00}", Kod, NameTovar,CinaProdazh,ID_Tovar.ToString().PadLeft(12,'0')+ean.ChecksumDigit);
            
        }
    }
    [TableName("Groups")]
    public class Group
    {
        [PrimaryKey, NonUpdatable, Identity]
        public int ID_Group { get; set; }
        public string Name { get; set; }

    }
    public class Journal
    {
        [PrimaryKey, NonUpdatable, Identity]
        public int ID { get; set; }
        public int ID_Tovar { get; set; }
        public int ID_Partner { get; set; }
        public int ID_User { get; set; }
        [Association(ThisKey = "ID_User", OtherKey = "ID_User", CanBeNull = false)]
        public User User { get; set; }
        [Association(ThisKey = "ID_Tovar", OtherKey = "ID_Tovar", CanBeNull = true)]
        public Tovar Tovar { get; set; }
        [Association(ThisKey = "ID_Partner", OtherKey = "ID_Partner", CanBeNull = true)]
        public Partner Partner { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string Comment { get; set; }
        [MapIgnore]
        public int Kod
        {
            get
            {
                return (Tovar == null) ? Partner.ID_Partner : Tovar.ID_Tovar;
            }
        }
        [MapIgnore]
        public string Name
        {
            get
            {
                return (Tovar == null) ? Partner.NamePartner : Tovar.NameTovar;
            }
        }
    }

    public class Partner
    {
        [PrimaryKey, NonUpdatable]
        public int ID_Partner { get; set; }
        public string NamePartner { get; set; }
        public PartnerType PartnerType { get; set; }
        public override string ToString()
        {
            return NamePartner;
        }
    }

    [TableName("Users")]
    public class User
    {
        [PrimaryKey, NonUpdatable]
        public int ID_User { get; set; }
        public string Login { get; set; }
        [MapField("PASS")]
        public string Password{ get; set; }
        public Rights Rights { get; set; }

        public static User CurrentUser { get; set; }

        public override string ToString()
        {
            return Login;
        }
        [MapIgnore]
        public bool IsAdmin { get { return Rights == Entities.Rights.Admin; } }


    }
    public enum Rights
    { 
        [MapValue(1)]
        Admin,
        [MapValue(2)]
        User
    }
    public enum PartnerType
    { 
        [MapValue(1)]
        Income,
        [MapValue(2)]
        Sale,
        [MapValue(3)]
        Reduce
    }
}
