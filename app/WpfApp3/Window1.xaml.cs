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
            private object currentData;
         public object CurrentData
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
            public MainWindow.Fk Fk;
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
            public string Name { get; set; }
       public RowForList(object data, string name, bool IsFk) { isFK = IsFk; this.data = data; Name = name; }
            public RowForList(MainWindow.Fk fk, string name, bool IsFk,object dat)
            { 
                Fk = fk; isFK = IsFk;  data = Fk.data; Name = name; int x; 
                if(fk.id.Length!=0)
                { if (dat.GetType()==1.GetType()) 
                    { x = Convert.ToInt32(dat); } 
                    else { x = fk.id[0]; }  
                    for (int i = 0; i < fk.id.Length; i++) 
                    { if (fk.id[i] == x) { currentData = (object)fk.data[i]; break; } 
                    }
                }
                else
                {
                    currentData = null;
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
       ObservableCollection<RowForList> colls =new ObservableCollection<RowForList>();
        MainWindow main;
        public Window1(DataRow row, List<DataGridColumn> coll,MainWindow main, List<MainWindow.Fk> fks)
        {
            this.main = main;
            InitializeComponent();
            data = row;
            bool isFk = false ;
            MainWindow.Fk FK=null;
      for(int i =0;i< data.ItemArray.Length; i++)
            {
                isFk=false;
                   foreach (MainWindow.Fk fk in fks)
                   { 
                           if (fk.name == coll[i].Header.ToString())
                           {
                               FK = fk;
                               isFk = true;
                           }

                   }
                   if (!isFk)
                   {
                       colls.Add(new RowForList(data.ItemArray[i], coll[i].Header.ToString(), isFk));
                   }
                   else
                   {
                       colls.Add(new RowForList(FK, coll[i].Header.ToString(), isFk, data.ItemArray[i]));
                   }
       
            }
        
            List.ItemsSource = colls;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            object[] newdata = new object[data.ItemArray.Length];
            test.Text += colls[1].Data.ToString();
            for (int i = 0; i < data.ItemArray.Length; i++)
            {
                if (colls[i].isFK) { 
                    
                    for(int j = 0; j< colls[i].Fk.data.Length;j++)
                    {
                        if (colls[i].CurrentData.ToString() == colls[i].Fk.data[j])
                        {
                            newdata[i] = colls[i].Fk.id[j];
                        }
                    }
                }
                else
                {
                    newdata[i] = colls[i].Data;
                }
            }
            data.ItemArray = newdata;
            test.Text += data.ItemArray[1].ToString();
            main.dataUpdate(data);
        }
    }
}
