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
       public int Type { get; set; }
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
            public void SetType(int type)
            {
                Type = type;
            }
   
           
       public RowForList(object data, string name) { this.data = data; Name = name; Type = 1; }
            public RowForList(DateTime data, string name) { this.data = data; Name = name; Type = 4; }
            public RowForList(DataTable d, string name,object dat)
            {
                Type = 2;
                 ds = d; Name = name; RowData = new List<RowToBox>(); foreach(DataRow r in ds.Rows)
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
           
        }
        
        DateTime time = DateTime.Now;
        ObservableCollection<RowForList> colls =new ObservableCollection<RowForList>();
        MainWindow main;
        DataTable col;
    
        int fkCount,doc;
        public Window1(DataRow row,MainWindow main, List<MainWindow.Fk> fks,DataTable coll,int docType)
        {
            this.main = main;
            InitializeComponent();
            col = coll;
            data = row;
            bool isFk;
            fkCount = fks.Count;
            MainWindow.Fk FK=null;
            string s,s1;
            int move = 0;
            doc = docType;
      for(int i =0;i< data.ItemArray.Length - fkCount; i++)
            {
                s1 = data.Table.Columns[i].ColumnName.ToString();
                //  test.Text +=" "+ data.ItemArray[i].ToString();
                if (s1 == "id")
                {
                    move++;
            
                }
                else
                {
                    if (docType == -1 || (s1 != "type_id" && s1 != "document_id"))
                    {
                        s = coll.Rows[i - move][1].ToString();
                        isFk = false;
                        foreach (MainWindow.Fk fk in fks)
                        {
                            if (fk.name == s1)
                            {
                                FK = fk;
                                isFk = true;
                            }

                        }
                        if (!isFk)
                        {
                            if (data.Table.Columns[s1].DataType == typeof(DateTime))
                            {
                                if (s1 == "change_date")
                                {
                                    colls.Add(new RowForList(time, s));
                                    colls.Last<RowForList>().SetType(5);
                                }
                                else
                                {
                                    if (data.ItemArray[0].GetType() != typeof(DBNull))
                                    {
                                        colls.Add(new RowForList((DateTime)data.ItemArray[i], s));
                                    }
                                    else
                                    {
                                        colls.Add(new RowForList(time, s));
                                    }
                                }

                            }
                            else
                            {
                                colls.Add(new RowForList(data.ItemArray[i], s));
                                if (data.Table.Columns[s1].DataType == typeof(bool))
                                {

                                    colls.Last<RowForList>().SetType(3);
                                }
                            }


                        }
                        else
                        {
                            colls.Add(new RowForList(FK.ds, s, data.ItemArray[i]));
                        }
                    }
                    else
                    { 
                      
                        move++;
                       
                    }
                }
       
            }
        
            List.ItemsSource = colls;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            object[] newdata = new object[data.ItemArray.Length];
            int move = 0;
            string s;
            RowForList row;
            for (int i = 0; i < data.ItemArray.Length - fkCount; i++)
            {
                s = data.Table.Columns[i].ColumnName.ToString();
                if (s == "id")
                {
                    move++;
                    newdata[i] = data.ItemArray[i];
          
                 
                }
                else
                {
                    if (doc == -1 || (s!="type_id" && s != "document_id"))
                    {
                        row = colls[i - move];
                        switch (row.Type)
                        {
                            case 2:
                                for (int j = 0; j < row.RowData.Count; j++)
                                {
                                    if (row.CurrentData.ToInt() == row.RowData[j].ToInt())
                                    {
                                        newdata[i] = row.CurrentData.ToInt();
                                    }
                                }
                                break;
                            default: newdata[i] = row.Data; break;

                        }
                    }
                    else
                    {
                        newdata[i] = doc;
                        move++;
                        
                    }
                }
                
                }
            data.ItemArray = newdata;

            main.dataUpdate(data);
        }
    }
}
