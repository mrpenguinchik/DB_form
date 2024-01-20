using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataGridExtensions;
namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

 public class Fk
        {
           public string name;
            public int[] id;
            public string[] data;
            
            public Fk(string title)
            {
                name = title;
            }
            public void PutData(int[] id, string[] names)
            {
                data = names;
                this.id = id;
            }
        }
        class MyTable
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=1234;Database=Sklad";
            private DataSet ds = new DataSet();

            List<Fk> FKs = new List<Fk>();
            public List<Fk> GetFks() => FKs;
            NpgsqlDataAdapter da;
            NpgsqlConnection con;
         public   string Title { get; private set; }
     
            public  MyTable(string title)
            {
                List<int> ids = new List<int>();
                List<string> names = new List<string>();
     
                con = new NpgsqlConnection(connectionString);
                con.Open();

                string sql = ("SELECT * FROM "+title);
                da = new NpgsqlDataAdapter(sql, con);
                var dataSource = NpgsqlDataSource.Create(connectionString);
                using (var cmd = dataSource.CreateCommand("select coll,foreign_table_name from get_fks('"+title+"');"))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                           ids.Clear();
                           names.Clear();
                           Fk fk = new Fk(reader.GetString(0));
                             
                        using (var cmd2 = dataSource.CreateCommand("select id,name from " + reader.GetString(1)))
                        using (var reader2 = cmd2.ExecuteReader())
                        {
                            while (reader2.Read())
                            {

                               ids.Add(reader2.GetInt32(0));
                                names.Add(reader2.GetString(1)); 
                            }
                        }
                       fk.PutData(ids.ToArray(), names.ToArray());
                        FKs.Add(fk); 


                    }
                }
                ds.Reset();
                da.Fill(ds);
                NpgsqlCommandBuilder cb = new NpgsqlCommandBuilder(da);
                da.UpdateCommand = cb.GetUpdateCommand(true);
                da.DeleteCommand = cb.GetDeleteCommand(true);
                da.InsertCommand = cb.GetInsertCommand(true);
                Title = title;
              
         
            }
            public void CreateFkCols()
            {
              
                List<string> s = new List<string>();
                for(int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    if (ds.Tables[0].Columns[i].ColumnName.ToString().EndsWith("_id"))
                    {
                      
                        s.Add(ds.Tables[0].Columns[i].ColumnName.ToString().TrimEnd("_id".ToCharArray()));
                    }
                }
                for(int i = 0; i < s.Count; i++)
                {
                    ds.Tables[0].Columns.Add(s[i]);
                }
            }
            public void UpdateFkCols()
            {

            }
            public DataTable GetData() => ds.Tables[0];
           public void Save()
            {
                da.Update(ds);
            }
            public void Update(DataRow data,DataRow find)
            {
                ds.Tables[0].Rows[ds.Tables[0].Rows.IndexOf(find)].ItemArray = data.ItemArray;
            }
            public DataRow AddData()
            {
                ds.Tables[0].Rows.Add(ds.Tables[0].NewRow());
                return ds.Tables[0].Rows[GetData().Rows.Count - 1];
            }
            
        }
      MyTable currentTable;
        public MainWindow()
        {
            InitializeComponent();

        
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            currentTable?.Save();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (((TreeViewItem)(tree.SelectedItem)).Tag!=null) {
                currentTable = new MyTable(((TreeViewItem)(tree.SelectedItem)).Tag.ToString());
                dataGrid.ItemsSource = currentTable.GetData().DefaultView;
                
            }
          

        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                oldRow = ((DataRowView)dataGrid.SelectedItem).Row;
                Window1 window = new Window1(oldRow, dataGrid.Columns.ToList<DataGridColumn>(),this,currentTable.GetFks());
      
                window.Show();
            }
        }
        DataRow oldRow;
        public void dataUpdate(DataRow data)
        { 
            currentTable.Update(data, oldRow);
            dataGrid.ItemsSource = currentTable.GetData().DefaultView;
     
        }
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
   
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            oldRow = currentTable.AddData();
                Window1 window = new Window1(oldRow, dataGrid.Columns.ToList<DataGridColumn>(), this,currentTable.GetFks());
                window.Show();
            
        }
    }
      
}
