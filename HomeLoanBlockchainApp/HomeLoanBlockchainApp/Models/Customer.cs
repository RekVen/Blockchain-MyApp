using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLoanBlockchainApp;
using static HomeLoanBlockchainApp.Model.MainModel;

namespace HomeLoanBlockchainApp.Model
{
    public class Customer
    {
        public Customer(string custId)
        {
            CustomerID = custId;
            AccountList = new List<Account>();
        }

        public bool CreateNewAccount(Account newAccount)
        {
            AccountList.Add(newAccount);
            return true;
        }

        bool DeleteAccount(string acctID)
        {
            return true;
        }

        public CustomerInfo Convert2CustomerInfo()
        {
            CustomerInfo info = new CustomerInfo();

            info.CustomerId = CustomerID;
            info.FirstName = FirstName;
            info.LastName = LastName;
            info.CustAddress = this.Address;
            info.SSN =SSN;
            info.BirthDate =BirthDate;
           
            return info;
        }

        #region Data memeber
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string SSN { get; set; }
        public string BirthDate { get; set; }
        public string CustomerID { get; set; }
        public List<Account>AccountList { get; set; }
        public string AccoutsStr
        {
            get { return AccountList.ToString(); }           
        }
        #endregion
    }
}
