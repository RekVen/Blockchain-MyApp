using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using HomeLoanBlockchainApp.ViewModel;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace HomeLoanBlockchainApp.Model
{
    public class MainModel
    {
         public MainModel(MainViewModel mainViewModel, string web3Url)
        {
            this.mainViewModel = mainViewModel;
            Accounts = new string[] { };
            CustomerList = new List<CustomerInfo>();
            this.web3Url = web3Url;
            Web3 = new Web3(web3Url);
            GetInfo();
        }

        internal async Task  RetrieveCustInfo()
        {
            List<AccountsInfo> AccountsInfoList = new List<MainModel.AccountsInfo>();
            try
            {
                uint num = await NumOfCustomersFunction.CallAsync<uint>();
                if (num > 0)
                {
                    if (CustomerList.Count > 0)
                    {
                        for (int index = CustomerList.Count - 1; index >= 0; index--)
                            CustomerList.RemoveAt(index);
                    }
                    for (int item = 0; item < num; item++)
                    {
                        var custInfo = await CustomerArray.CallDeserializingToObjectAsync<CustomerInfo>(item).ConfigureAwait(false);
                        var custAccountsInfo = await CustAccountsListFunction.CallDeserializingToObjectAsync<AccountsInfo>(custInfo.CustomerId).ConfigureAwait(false);

                        var numOfAccounts = await NumberOfAccountsFunction.CallAsync<uint>();
                        if (numOfAccounts > 0)
                        {
                            for (uint itemAcct = 0; itemAcct < numOfAccounts; itemAcct++)
                            {
                                var AccountInfo = await AccountsArrayFunction.CallDeserializingToObjectAsync<AccountsInfo>(itemAcct).ConfigureAwait(false);
                                AccountsInfoList.Add(AccountInfo);
                            }
                        }
                        CustomerList.Add(custInfo);
                    }
                }
                var filterAll = await NewAccountAddedEvent.CreateFilterAsync();
                var log = await NewAccountAddedEvent.GetFilterChanges<NewAccountAddedEventDTO>(filterAll);
                if (log.Count > 0)
                {
                    for (int x = 0; x < log.Count; x++)
                    {
                        NewAccountAddedInfo custInfo = new NewAccountAddedInfo();
                        var data = log.ElementAt(x);
                        custInfo.custID = data.Event.CustID;
                        custInfo.sender = data.Event.Sender;
                        Console.WriteLine("Transactions : {0}  ---  {1}", data.Event.Sender, data.Event.CustID);
                        AllNewAccountsList.Add(custInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        async void GetInfo()
        {
            Connect();
        }

        void Connect()
        {
            GetContract_CustomerInfo();
            GetContract_AccountInfo();
        }

        async void GetContract_CustomerInfo()
        {
            string strCustTxt = @"CustomerInfo.txt";           
            try
            {
                Accounts = await Web3.Eth.Accounts.SendRequestAsync();
                List<NewAccountAddedInfo> AllNewAccountsList = new List<NewAccountAddedInfo>();
                var inputCompiled = System.IO.File.ReadAllText(strCustTxt);

                JavaScriptSerializer js = new JavaScriptSerializer();
                System.Collections.Generic.Dictionary<string, object> inputCompliedContext = js.Deserialize<Dictionary<string, object>>(inputCompiled.ToString());
                var infoValue = inputCompliedContext["unlinked_binary"];
                var abiInfo = inputCompliedContext["abi"];

                var contractAddress = "0x45a2c215697743be0117dcebf81d139f8c187a28"; 
                Contract Contract = Web3.Eth.GetContract(abiInfo.ToString(), contractAddress);
                NewCustomerObj = Contract.GetFunction("newCustomerObj");
                CustomerListFunction = Contract.GetFunction("customerList");
                NumOfCustomersFunction = Contract.GetFunction("numberOfCustomers");
                GetCustomerObj = Contract.GetFunction("GetCustomer");
                CustomerArray = Contract.GetFunction("customerArray");
                NewAccountAddedEvent = Contract.GetEvent("NewAccountAdded");

                await mainViewModel.GetCustomers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        async void GetContract_AccountInfo()
        {
            string strAcctTxt = @" AccountInfo.txt";      
            try
            {
                Accounts = await Web3.Eth.Accounts.SendRequestAsync();
                List<NewAccountAddedInfo> AllNewAccountsList = new List<NewAccountAddedInfo>();
                var inputCompiled = System.IO.File.ReadAllText(strAcctTxt);

                JavaScriptSerializer js = new JavaScriptSerializer();
                System.Collections.Generic.Dictionary<string, object> inputCompliedContext = js.Deserialize<Dictionary<string, object>>(inputCompiled.ToString());
                var infoValue = inputCompliedContext["unlinked_binary"];
                var abiInfo = inputCompliedContext["abi"];

                var contractAddress = "0xe430aa9ee76843e807c92a5a50d70a2d6f281c44";
                Contract Contract = Web3.Eth.GetContract(abiInfo.ToString(), contractAddress);
                CustAccountsListFunction = Contract.GetFunction("CustAccountList");
                AccountsArrayFunction = Contract.GetFunction("AccountsArray");
                NewAccountObjFunction = Contract.GetFunction("AddNewAccount");
                GetAccountFunction = Contract.GetFunction("GetAccount");
                NumberOfAccountsFunction = Contract.GetFunction("GetNumberOfAccounts");
                DeleteAccountFunction = Contract.GetFunction("DeleteAccount");
                DeleteAllAccountFunction = Contract.GetFunction("DeleteAllAccount");
                AddFundsFunction = Contract.GetFunction("AddFunds");
                SubtractFundsFunction = Contract.GetFunction("SubtractFunds");
                BeforeBalanceChangeEvent = Contract.GetEvent("BeforeBalanceChange");                 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public async void AddNewCustomer(Customer cust)
        {
            try
            {
                Random random = new Random();
                var custId = cust.CustomerID ;
                var fName = cust.FirstName;
                var lName =  cust.LastName;
                var custAddress = cust.Address;
                var ssn =  cust.SSN;
                var birthDate =  cust.BirthDate;

                if (cust.AccountList.Count > 0)
                {
                    foreach (Account acct in cust.AccountList)
                    {
                        var acctNumber = acct.AccountID;
                        var acctType = acct.AccountType;
                        var balance = acct.Balance;
                        var transHash = await NewCustomerObj.SendTransactionAsync(Accounts[0], new HexBigInteger(900000), null, custId, fName, lName, custAddress, ssn, birthDate, acctNumber,
                                                                                   acctType, balance);
                        var receipt = await MineAndGetReceiptAsync(Web3, transHash);
                        var numOfAccounts = await NumberOfAccountsFunction.CallAsync<uint>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public async void UpdateCustomer(Customer cust)
        {
            try
            {
                var custId = cust.CustomerID;
                var fName = cust.FirstName;
                var lName = cust.LastName;
                var custAddress = cust.Address;
                var ssn = cust.SSN;
                var birthDate = cust.BirthDate;

                // var transHash = await this.UpdateCustomer.SendTransactionAsync(Accounts[0], new HexBigInteger(900000), null, custId, cust.FirstName, cust.LastName, cust.Address, cust.SSN, cust.BirthDate);
                // var receipt = await MineAndGetReceiptAsync(Web3, transHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        internal async void DeleteCustomer(Customer cust)
        {
            try
            {
                var custId = cust.CustomerID;
                //var transHash = await this.DeleteCustomer.SendTransactionAsync(Accounts[0], new HexBigInteger(900000), null, custId);
                //var receipt = await MineAndGetReceiptAsync(Web3, transHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
  
        async void GetAllTransactions(Web3 Web3, Contract Contract, string[] Accounts)
        {
            var date = DateTime.Now.ToShortTimeString();
            var hour = DateTime.Now.Hour;
            var minTimestamp = (hour - 1) / 1000;
            Console.WriteLine("Retreiving all transactions from {0}", date);
            try
            {
                var block = await Web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();     
                //--
             }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
       
        async Task<TransactionReceipt> MineAndGetReceiptAsync(Web3 web3, string transactionHash)
        {
            var miningResult = await web3.Miner.Start.SendRequestAsync(6);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            while (receipt == null)
            {
                Thread.Sleep(3000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }
            miningResult = await web3.Miner.Stop.SendRequestAsync();
            return receipt;
        }

        #region Data Member
        Web3 Web3 { get; set; }
        MainViewModel mainViewModel { get; set; }
        string web3Url { get; set; }
        string[] Accounts { get; set; }
        public List<CustomerInfo> CustomerList { get; set; }

        Function NewCustomerObj { get; set; }
        Function CustomerListFunction { get; set; }
        Function NumOfCustomersFunction { get; set; }
        Function GetCustomerObj { get; set; }
        Function CustomerArray { get; set; }
        Event NewAccountAddedEvent { get; set; }

        public Function CustAccountsListFunction { get; private set; }
        public Function NewAccountObjFunction { get; private set; }
        public Function GetAccountFunction { get; private set; }
        public Function NumberOfAccountsFunction { get; private set; }
        public Function DeleteAccountFunction { get; private set; }
        public Function DeleteAllAccountFunction { get; private set; }
        public Function AddFundsFunction { get; private set; }
        public Function SubtractFundsFunction { get; private set; }
        public Event BeforeBalanceChangeEvent { get; set; }
        public Function AccountsArrayFunction { get; private set; }
        public List<NewAccountAddedInfo> AllNewAccountsList { get; private set; }
        
        [FunctionOutput]
        public class CustomerInfo
        {

            [Parameter("address", "customerId", 1)]
            public string CustomerId { get; set; }

            [Parameter("string", "firstName", 2)]
            public string FirstName { get; set; }

            [Parameter("string", "LastName", 3)]
            public string LastName { get; set; }

            [Parameter("string", "custAddress", 4)]
            public string CustAddress { get; set; }

            [Parameter("string", "SSN", 5)]
            public string SSN { get; set; }

            [Parameter("string", "birthDate", 6)]
            public string BirthDate { get; set; }
        }

        [FunctionOutput]
        public class AccountsInfo
        {

            [Parameter("address", "customerId", 1)]
            public string CustomerId { get; set; }

            [Parameter("uint", "accountNumber", 2)]
            public BigInteger AccountNumber { get; set; }

            [Parameter("uint", "accountType", 3)]
            public BigInteger AccountType { get; set; }

            [Parameter("uint", "balance", 4)]
            public BigInteger Balance { get; set; }

            [Parameter("uint", "timeStamp", 5)]
            public BigInteger TimeStamp { get; set; }
        }

        public class NewAccountAddedEventDTO
        {
            [Parameter("address", "senderAddress", 1, true)]
            public string Sender { get; set; }

            [Parameter("address", "custid", 2, true)]
            public string CustID { get; set; }
        }

        public class NewAccountAddedInfo
        {
            public string custID;
            public string sender;
        }
        #endregion

    }
}



