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
           public DataTable ds { get; private set; }
            
            public Fk(string title)
            {
                name = title;
            }
            public void PutData(DataTable newDs)
            {
                ds = newDs;
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
            DataSet ds2;
         public   string Title { get; private set; }
     
            public  MyTable(string title)
            {
             
     
                con = new NpgsqlConnection(connectionString);
                con.Open();
                NpgsqlDataAdapter da2;
                var dataSource = NpgsqlDataSource.Create(connectionString);
                string sql = "SELECT ";  //* FROM "+title);
                string join="";
                using (var cmd = dataSource.CreateCommand("SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME =N'" + title + "';"))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        sql += title+"."+ reader.GetString(0)+",";
                    }
                }
                using (var cmd = dataSource.CreateCommand("select coll,foreign_table_name from get_fks('" + title + "');"))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sql += reader.GetString(1)+".name as "+reader.GetString(1).TrimEnd("_id".ToCharArray())+",";
                        join += " INNER JOIN " + reader.GetString(1) + " ON "+title+"."+reader.GetString(0)+" = " + reader.GetString(1) + ".id";
                      /*  Fk fk = new Fk(reader.GetString(0));

                        string sql2 = ("select id,name from " + reader.GetString(1));
                        da2 = new NpgsqlDataAdapter(sql2, con);
                        da2.Fill(ds, reader.GetString(1));
                        fk.PutData(ds.Tables[reader.GetString(1)]);
                        FKs.Add(fk);*/


                    }
                }
                sql = sql.TrimEnd(',');
                sql += " from " + title;
                sql = sql + join;
                da = new NpgsqlDataAdapter(sql, con);
                ds.Reset();
                da.Fill(ds,"main");
                NpgsqlCommandBuilder cb = new NpgsqlCommandBuilder(da);
                da.UpdateCommand = cb.GetUpdateCommand(true);
                da.DeleteCommand = cb.GetDeleteCommand(true);
                da.InsertCommand = cb.GetInsertCommand(true);
               
              
               /* for(int i = 0; i < FKs.Count; i++)
                {
                    DataRelation fk = new DataRelation(FKs[i].name,ds.Tables[i+1].Columns["id"],ds.Tables[0].Columns[FKs[i].name],true);
                    ds.Tables[0].ParentRelations.Add(fk);
                    DataColumn col=new DataColumn();
                    string s = FKs[i].name.Trim("_id".ToCharArray());
                    col.ColumnName = s;
                    col.DataType ="a".GetType();
                    ds.Tables[0].Columns.Add(col);
                     ds.Tables[0].Columns[s].Expression = "Parent(["+ FKs[i].name + "]).name";
                  
                }*/
                Title = title;
      
         
            }
            public ConstraintCollection GetConstraint() => ds.Tables[0].Constraints;
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
                da.Update(ds,"main");
            }
            public void Update(DataRow data,DataRow find)
            {
                int x = ds.Tables[0].Rows.IndexOf(find);
           
                for(int i = 0; i < data.ItemArray.Length-FKs.Count;i++)
                {
                    ds.Tables[0].Rows[x].ItemArray[i] = data.ItemArray[i];
                }
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
                DataGridUpdate();
            }
          

        }
        public void DataGridUpdate()
        {
            dataGrid.Columns.Clear();
            dataGrid.ItemsSource = currentTable.GetData().DefaultView;

          foreach (DataColumn c in currentTable.GetData().Columns)
            {
                if (!c.ColumnName.ToString().Contains("id")) {
                    string label = c.ColumnName.ToString();
                    DataGridTextColumn column = new DataGridTextColumn();
                    column.Header = label;
                    column.Binding = new Binding(label.Replace(' ', '_'));
                    dataGrid.Columns.Add(column);
                }
            } 
         }
        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            { 
                oldRow =((DataRowView)dataGrid.SelectedItem).Row;
                Window1 window = new Window1(oldRow, currentTable.GetData().Columns, this,currentTable.GetFks());
      
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
                Window1 window = new Window1(oldRow, currentTable.GetData().Columns, this,currentTable.GetFks());
                window.Show();
            
        }
    }
      
}
