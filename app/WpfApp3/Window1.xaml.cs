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

            public event PropertyChangedEventHandler PropertyChanged;
        }
       ObservableCollection<RowForList> colls =new ObservableCollection<RowForList>();
        MainWindow main;
        public Window1(DataRow row, List<DataGridColumn> coll,MainWindow main)
        {
            this.main = main;
            InitializeComponent();
            data = row;
            bool isFk = false ;

      for(int i =0;i< data.ItemArray.Length; i++)
            {
                isFk=false;
              
              
                  
                       colls.Add(new RowForList(data.ItemArray[i], coll[i].Header.ToString(), isFk));
                   
            
       
            }
        
            List.ItemsSource = colls;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            object[] newdata = new object[data.ItemArray.Length];
            test.Text += colls[1].Data.ToString();
            for (int i = 0; i < data.ItemArray.Length; i++)
            {
      
              
                    newdata[i] = colls[i].Data;
                
            }
            data.ItemArray = newdata;
            test.Text += data.ItemArray[1].ToString();
            main.dataUpdate(data);
        }
    }
}
