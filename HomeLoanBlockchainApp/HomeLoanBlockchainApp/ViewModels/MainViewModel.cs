using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLoanBlockchainApp.Model;

namespace HomeLoanBlockchainApp.ViewModel
{
    public class MainViewModel
    {
        public MainViewModel(MainWindow mainWindow, string webUrl)
        {
            Web3Url = webUrl;
            MainWindow = mainWindow;
            Model = new MainModel( this, Web3Url);   
        }

        public async Task GetCustomers()
        {
           await Model.RetrieveCustInfo();
           MainWindow.dgCustomers.ItemsSource = null;
           MainWindow.dgCustomers.ItemsSource = Model.CustomerList;
        }

        public void AddNewCustomer(Customer cust)
        {
            Model.AddNewCustomer(cust);
            GetCustomers();
        }

        #region data Memebers
        string Web3Url { get; set; }
        MainWindow MainWindow { get; set; }
        MainModel Model { get; set; }       
        #endregion
    }
}
