using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        DataRow data;
        class RowForList : INotifyPropertyChanged
        {
            private object data;
            private RowToBox currentData;
         public RowToBox CurrentData
            {
                get
                {
                    return currentData;
                }
                set
                {
                    currentData = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentData"));
                }
            }
       public bool isFK { get; set; }
            DataTable ds;
            public object Data
            {
                get
                {
                    return data;
                }
                set
                {
                   data = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("data"));
                }
            }
          public List<RowToBox> RowData { get; set; }
     
            public string Name { get; set; }
       public RowForList(object data, string name, bool IsFk) { isFK = IsFk; this.data = data; Name = name; }
            public RowForList(DataTable d, string name, bool IsFk,object dat)
            { 
                 isFK = IsFk; ds = d; Name = name; RowData = new List<RowToBox>(); foreach(DataRow r in ds.Rows)
                { 
                    RowData.Add(new RowToBox(r));
                    if (dat.GetType() !=typeof( DBNull))
                    {
                        if ((int)dat == (int)r[0])
                        {
                            currentData = RowData.Last<RowToBox>();
                        }
                    }
                }
                
          
          
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
        public class RowToBox
        {
            DataRow Row;
            public RowToBox(DataRow row)
            {
                Row = row;
            }
            public override string ToString()
            {
               return Row[1].ToString();
            }
            public int ToInt()
            {
                return (int)Row[0];
            }
        }
       ObservableCollection<RowForList> colls =new ObservableCollection<RowForList>();
        MainWindow main;
        DataColumnCollection col;
        int fkCount;
        public Window1(DataRow row, DataColumnCollection coll,MainWindow main, List<MainWindow.Fk> fks)
        {
            this.main = main;
            InitializeComponent();
            col = coll;
            data = row;
            bool isFk;
            fkCount = fks.Count;
            MainWindow.Fk FK=null;
      for(int i =0;i< data.ItemArray.Length - fkCount; i++)
            {
              //  test.Text +=" "+ data.ItemArray[i].ToString();
                if (coll[i].ColumnName.ToString() == "id")
                {
                    continue;
                }
                else
                {
                    isFk = false;
                    foreach (MainWindow.Fk fk in fks)
                    {
                        if (fk.name == coll[i].ColumnName.ToString())
                        {
                            FK = fk;
                            isFk = true;
                        }

                    }
                    if (!isFk)
                    {
                        colls.Add(new RowForList(data.ItemArray[i], coll[i].ColumnName.ToString(), isFk));
                    }
                    else
                    {
                        colls.Add(new RowForList(FK.ds, coll[i].ColumnName.ToString(), isFk, data.ItemArray[i]));
                    }
                }
       
            }
        
            List.ItemsSource = colls;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            object[] newdata = new object[data.ItemArray.Length];
      
            for (int i = 0; i < data.ItemArray.Length - fkCount; i++)
            {
                if (col[i].ColumnName.ToString() == "id")
                {
                    continue;
                }
                else
                {
                     if (colls[i].isFK) { 

                         for(int j = 0;j< colls[i].RowData.Count;j++)
                         {
                             if (colls[i].CurrentData.ToInt() == colls[i].RowData[j].ToInt())
                             {
                                 newdata[i] = colls[i].CurrentData.ToInt();
                             }
                         }
                     }
                     else
                     {
                         newdata[i] = colls[i].Data;
                     }
                }
                }
            data.ItemArray = newdata;
            test.Text += data.ItemArray[1].ToString();
            main.dataUpdate(data);
        }
    }
}
