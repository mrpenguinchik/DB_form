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
        class MyTable
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=1234;Database=Sklad";
            private DataSet ds = new DataSet();
            NpgsqlDataAdapter da;
            NpgsqlConnection con;
         public   string Title { get; private set; }
     
            public  MyTable(string title)
            {
   
     
                con = new NpgsqlConnection(connectionString);
                con.Open();

                string sql = ("SELECT * FROM "+title);
                da = new NpgsqlDataAdapter(sql, con);
                ds.Reset();
                da.Fill(ds,"main");

                NpgsqlDataAdapter da2;
                List<string> names = new List<string>();
                List<string> cols = new List<string>();
                var dataSource = NpgsqlDataSource.Create(connectionString);
                using (var cmd = dataSource.CreateCommand("select coll,foreign_table_name from get_fks('"+title+"');"))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        cols.Add(reader.GetString(0));
                    names.Add( reader.GetString(1));
                        string s = names.Last<string>();
                        string sql2 =("select id,name from " + s);
                        da2 = new NpgsqlDataAdapter(sql2, con);
                        da2.Fill(ds, s);
                    }
                }
                ForeignKeyConstraint fk;
               for (int i = 0; i < names.Count; i++)
                {
                    fk = new ForeignKeyConstraint(ds.Tables[names[i]].Columns["id"], ds.Tables[0].Columns[cols[i]]);
                    fk.DeleteRule = Rule.None;
                    ds.Tables[0].Constraints.Add(fk);
                }
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
            public int GetTableCount() => ds.Tables.Count;
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
             //   Window1 window = new Window1(oldRow, dataGrid.Columns.ToList<DataGridColumn>());
      
               // window.Show();
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
             //   Window1 window = new Window1(oldRow, dataGrid.Columns.ToList<DataGridColumn>());
            //    window.Show();
            
        }
    }
      
}
