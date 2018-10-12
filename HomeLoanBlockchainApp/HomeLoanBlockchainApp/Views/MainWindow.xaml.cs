using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HomeLoanBlockchainApp.Model;
using HomeLoanBlockchainApp.ViewModel;

namespace HomeLoanBlockchainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            VM = new MainViewModel(this, "http://127.0.0.1:8500/");
            this.DataContext = VM;
            Initialize();          
        }

        private void Initialize()
        {
            Random random = new Random();
            this.txtAcctNumber.Text = (random.Next(1000, 100000)).ToString();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string[] actualDate = (dtBirthDate.ToString()).Split(' ');
            string custID = this.txtFirsName.Text.ToString() + txtSSN.Text.ToString();
            Customer cust = new Customer(custID);
            cust.FirstName = txtFirsName.Text.ToString();
            cust.LastName = txtLastName.Text.ToString();
            cust.Address = txtAddress.Text.ToString();
            cust.SSN = txtSSN.Text.ToString();
            cust.BirthDate = dtBirthDate.ToString();
            cust.CustomerID = custID;

            Account acct = new Account(custID, Convert.ToUInt32(txtAcctNumber.Text.ToString()), 1, 9999);
            cust.CreateNewAccount(acct);
            VM.AddNewCustomer(cust);
        }
        #region Data members
        public MainViewModel VM { get; set; }
        public string CustomerID { get; set; }
        public MainWindow MainWW { get; set; }
        #endregion


    }
}
