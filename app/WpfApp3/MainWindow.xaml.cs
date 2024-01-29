using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
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
            public string target;
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
        public class Filter : INotifyPropertyChanged
        { public bool Data
            {
                get
                {
                 
                    return data;
             
                }
                set
                {
                   data = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("text"));
                    text = data.ToString();
                        table.updateData();
                    
                }
            }
            bool data=false;
           
            public int type { get; set; }
            MyTable table;
            string coll;
          public string name { get; set; }
            public string text;
            public string Text
            {
                get
                {
                    return text;
                }
                set
                {
                    text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("text"));
               
                        table.updateData();
                    
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            public Filter(string name,string coll, MyTable myTable,int type) { this.name = name;this.coll = coll; table = myTable; this.type = type; }
            public override string ToString()
            {
                if (text != null)
                {
                    if (text.Length > 1)
                    {
                        switch (type)
                        {
                            case 1: return " " + coll + " ILIKE " + "'%" + text + "%' ";
                            case 2: return " CAST(" + coll + " as TEXT) ILIKE " + "'%" + text + "%' ";
                            case 3: return " " + coll + " = " + text;
                            default: return " ";
                        }
                    }
                    else
                    {
                        return " ";
                    }
                }
                else
                {
                    return " ";
                }
            
            }
            public string GetColl() => coll;
        }
        public class MyTable
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=1234;Database=Sklad";
            private DataSet ds = new DataSet();
          public  TextBox test;

            List<Fk> FKs = new List<Fk>();
            public List<Fk> GetFks() => FKs;
            public List<Filter> filters = new List<Filter>();
            public void updateData()
            {
                con.Open();
                string sql1 = sql;
                if (addition.Length < 2)
                {
                   addition= " where ";
                }
                string sql2 = addition;
                int i = 0;

                for (; i < filters.Count; i++)
                {
                    if (filters[i].ToString() != " ")
                    {
                        if (sql2.Length > addition.Length)
                        {
                            sql2 += " AND" + filters[i].ToString();

                        }
                        else
                        {
                            sql2 += " " + filters[i].ToString();
                        }
                    }
                }
                if(sql2.Trim(' ') == "where")
                {
                    sql2 = " ";
                }
                test.Text = sql1 + sql2;
           
                da = new NpgsqlDataAdapter(sql1+sql2, con);

                ds.Tables[0].Rows.Clear();
                da.Fill(ds, "main");
           
                foreach (DataColumn c in ds.Tables[0].Columns)
                {
                   // test.Text += c.ColumnName.ToString()+" ";
                }
                NpgsqlCommandBuilder cb = new NpgsqlCommandBuilder(da);
                da.UpdateCommand = cb.GetUpdateCommand();
                da.DeleteCommand = cb.GetDeleteCommand();
                da.InsertCommand = cb.GetInsertCommand();
                con.Close();
            }
          public  NpgsqlDataAdapter da;
            NpgsqlConnection con;
            string sql;
            string addition=" ";
         public   string Title { get; private set; }
  
  
                public MyTable(string title,int document_type)
            {


             
               
                Title = title;
                sql = ("SELECT * FROM " + title);
                if (document_type == -1) {
             
                }
                else
                {

                   
                }
                createTable(sql+addition);
               
            }
                   public MyTable(string title,string doc)
            {


       
                Title = title;
                sql = ("SELECT * FROM " + title );
                addition =" where document_id=" + doc;
                createTable(sql+addition);
            
            }
            void createTable(string sql1)
            {
                con = new NpgsqlConnection(connectionString);
                con.Open();
                string sql=sql1;
                da = new NpgsqlDataAdapter(sql, con);
                NpgsqlDataAdapter da2;
                ds.Reset();
                da.Fill(ds, "main");
                NpgsqlCommandBuilder cb = new NpgsqlCommandBuilder(da);
                da.UpdateCommand = cb.GetUpdateCommand();
                da.DeleteCommand = cb.GetDeleteCommand();
                da.InsertCommand = cb.GetInsertCommand();



                var dataSource = NpgsqlDataSource.Create(connectionString);
                using (var cmd = dataSource.CreateCommand("select coll,foreign_table_name from get_fks('" + Title + "');"))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        Fk fk = new Fk(reader.GetString(0));
                        if (reader.GetString(1) == "documents") { fk.target = "pid"; }
                        else
                        {
                            fk.target = "name";
                           
                        }
                        sql = ("select id, "+fk.target+" ,deleted from " + reader.GetString(1));
                        da2 = new NpgsqlDataAdapter(sql, con);
                        da2.Fill(ds, reader.GetString(1));
                        fk.PutData(ds.Tables[reader.GetString(1)]);
                        FKs.Add(fk);


                    }
                    reader.Close();
                }



                ds.Tables[0].Columns["deleted"].DefaultValue = false;


                sql = ("select column_name,description from get_desc('" + Title + "');");
                da2 = new NpgsqlDataAdapter(sql, con);
                da2.Fill(ds, "names");


                int type;
                Type t;
                string target;
               foreach (DataRow row in ds.Tables["names"].Rows)
                {
                    t = ds.Tables[0].Columns[row[0].ToString()].DataType;
                    if (t == typeof(string) )
                    {
                        type = 1;
                    }
                    else
                    {
                        if(t == typeof(bool) )
                        {
                            type = 3;
                        }
                        else
                        {
                            type = 2;
                        }
                    }
                    target = Title+"."+ row[0].ToString();
                   foreach(Fk fk in FKs)
                    {
                        //test.Text += fk.name + " ";
                        if (fk.name == target)
                        {
                     
                            target=fk.ds.TableName + "." + fk.target;
                           
                        }
                    }
                    filters.Add(new Filter(row[1].ToString(),target,this,type));
                }


                sql = "SELECT ";
                foreach(DataColumn c in ds.Tables[0].Columns)
                {
                    sql +=" "+Title+"."+ c.ColumnName + " , ";

                }
                sql = sql.Remove(sql.Length - 2,2);
                sql += " ";
                sql += " from " + Title;
                foreach(Fk fk1 in FKs)
                {
                    sql += " join "+fk1.ds.TableName+" on " + Title + "." + fk1.name + " = " + fk1.ds.TableName + "." + "id ";
                }
                sql = sql + addition;
                this.sql= sql;



                for (int i = 0; i < FKs.Count; i++)
                {
                    DataRelation fk = new DataRelation(FKs[i].name, ds.Tables[i + 1].Columns["id"], ds.Tables[0].Columns[FKs[i].name], true);
                    ds.Tables[0].ParentRelations.Add(fk);
                    DataColumn col = new DataColumn();
                    string s = FKs[i].name.Replace("_id", "");
                    col.ColumnName = s;
                    col.DataType = "a".GetType();
                    ds.Tables[0].Columns.Add(col);
                    if (FKs[i].name == "document_id")
                    {
                        ds.Tables[0].Columns[s].Expression = "Parent([" + FKs[i].name + "]).pid";
                    }
                    else
                    {
                        ds.Tables[0].Columns[s].Expression = "Parent([" + FKs[i].name + "]).name";
                    }

                }

                dataSource.Dispose();
                con.Close();

            }
            public DataTable GetNames() =>ds.Tables["names"];

            public bool IsDeleted(DataRow row)
            {
                bool del =(bool)ds.Tables[0].Rows[ ds.Tables[0].Rows.IndexOf(row)]["deleted"];
                return del;
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
            public void Delete(DataRow data)
            {
                ds.Tables[0].Rows[ds.Tables[0].Rows.IndexOf(data)]["deleted"] = true;
         


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
                currentTable = new MyTable(((TreeViewItem)(tree.SelectedItem)).Tag.ToString(),-1);
                currentDoc = -1;
                IsDocument = false;
                changeDataGrids();
                             DataGridUpdate(dataGrid,currentTable);
            }
          

        }
        public void DataGridUpdate(DataGrid dataGrid,MyTable currentTable)
        {
            currentTable.test = test;
            foreach(Fk fk in currentTable.GetFks())
            {
                test.Text += fk.name + " ";
            }
            filterList.ItemsSource = currentTable.filters;
            dataGrid.Columns.Clear();
            dataGrid.ItemsSource = currentTable.GetData().DefaultView;

          foreach (DataRow c in currentTable.GetNames().Rows)
            {
              
                    if (currentTable.GetData().Columns[c.ItemArray[0].ToString()].DataType !=true.GetType())
                    {
                        DataGridTextColumn column = new DataGridTextColumn();
                        column.Header = c.ItemArray[1].ToString();
                    column.Binding = new Binding(c.ItemArray[0].ToString().Replace("_id",""));
                        dataGrid.Columns.Add(column);
                    }
                    else
                    {
                        DataGridCheckBoxColumn column = new DataGridCheckBoxColumn();
                    column.Header = c.ItemArray[1].ToString();
                    column.Binding = new Binding(c.ItemArray[0].ToString().Replace("_id", ""));
                    dataGrid.Columns.Add(column);
                    }
                
            } 
         }
        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
            if (dataGrid.SelectedItem != null )
            {
            
                oldRow =((DataRowView)dataGrid.SelectedItem).Row;
                if (!currentTable.IsDeleted(oldRow))
                {
                    openWindow();
                }
                else
                {
                    popup1.IsOpen = true;
               
                        }
            }
        }
        DataRow oldRow;
        public void dataUpdate(DataRow data)
        {
            if (data.Table == currentTable.GetData())
            {
                currentTable.Update(data, oldRow);
            }
            else
            {
                docLines.Update(data, oldRow);
            }
      
     
        }
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
                if (IsDocument && dataGrid.SelectedItems!=null)
            { 
                docLines = new MyTable("documentlines",((DataRowView)dataGrid.SelectedItem).Row["id"].ToString());
              //  test.Text = ((DataRowView)dataGrid.SelectedItem).Row["id"].ToString();
                DataGridUpdate(dataGrid1, docLines);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (currentTable != null)
            {
                oldRow = currentTable.AddData();
                openWindow();
            }

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
         
            if (dataGrid.SelectedItem != null) {
                currentTable.Delete(((DataRowView)dataGrid.SelectedItem).Row);
                dataGrid.ItemsSource = currentTable.GetData().DefaultView;
            }
         
        }

        private void popupclose_Click(object sender, RoutedEventArgs e)
        {
            popup1.IsOpen = false;
        }

        private void popupopen_Click(object sender, RoutedEventArgs e)
        {
            openWindow();
            popup1.IsOpen = false;
        }
        int currentDoc;
        public void openWindow()
        {
            int currentDoc;
            MyTable table;
            if(oldRow.Table==currentTable.GetData())
            {
                table = currentTable;
               currentDoc = this.currentDoc;
            }
            else
            {
                table = docLines;
               currentDoc=Convert.ToInt32( docLines.GetData().Rows[0]["document_id"]);
            }
            Window1 window = new Window1(oldRow, this, table.GetFks(), table.GetNames(),currentDoc);

            window.Show();
        }
        bool IsDocument;
        private void doc_Click(object sender, RoutedEventArgs e)
        { 
            IsDocument = true;
            currentTable = new MyTable("documents",Convert.ToInt32( ((Button)sender).Tag.ToString()));
        
            currentDoc = Convert.ToInt32(((Button)sender).Tag.ToString());
            changeDataGrids();
            DataGridUpdate(dataGrid, currentTable);
        }
        MyTable docLines;
        private void changeDataGrids()
        {
            if (IsDocument)
            {
                Grid.SetRowSpan(dataGrid, 1);
               grid2.Visibility = Visibility.Visible;
            }
            else
            {
                Grid.SetRowSpan(dataGrid, 4);
                grid2.Visibility = Visibility.Collapsed;
            }

        }

        private void addLine_Click(object sender, RoutedEventArgs e)
        {
            if (docLines != null)
            {
                oldRow = docLines.AddData();
                openWindow();
            }
        }

        private void delLine_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItem != null)
            {
                docLines.Delete(((DataRowView)dataGrid1.SelectedItem).Row);
       
            }
        }

        private void saveLine_Click(object sender, RoutedEventArgs e)
        {
           docLines?.Save();

        }

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
            if (dataGrid1.SelectedItem != null)
            {

                oldRow = ((DataRowView)dataGrid1.SelectedItem).Row;
                if (!docLines.IsDeleted(oldRow))
                {
                    openWindow();
                }
                else
                {
                    popup1.IsOpen = true;

                }
            }
        }
    }
      
}
