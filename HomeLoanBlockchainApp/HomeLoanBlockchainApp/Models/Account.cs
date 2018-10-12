using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeLoanBlockchainApp.Model
{
    public class Account
    {
        public Account(string custId, uint acctId, uint acctType, float balance)
        {
            AccountID = acctId;
            CustomerID = custId;
            AccountType = acctType;
            Balance = balance;
        }

        #region Data memeber
        public uint AccountID { get; set; }
        public uint AccountType { get; set; }
        public float Balance { get; set; }
        public string CustomerID { get; set; }
        #endregion
    }
}
