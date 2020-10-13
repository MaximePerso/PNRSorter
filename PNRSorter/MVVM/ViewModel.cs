﻿using OfficeOpenXml;
using PNRSorter.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PNRSorter.MVVM
{
    public class ViewModel : VMBase
    {

        #region Fields
        private ObservableCollection<MyPNR> _myPNR;
        private List<MyPNR> _PMS;
        private List<MyPNR> _SNC;
        private List<MyPNR> _KE24;
        private List<string> _PNRPMS;
        private List<string> _PNRSNC;
        private List<string> _uniquePNR;
        private List<string> _toBeSorted;
        private string _PNRSearch;
        private ObservableCollection<string> _PNRList;
        private int _salesCol;
        private int _productCol;
        private int _unitSoldCol;
        private int _colNb;
        private Dictionary<string, int> _colInfo = new Dictionary<string, int>();
        private Dictionary<string, MyPNR> _PNRDic;
        #endregion

        #region Properties
        public ObservableCollection<MyPNR> MyPNR
        {
            get => _myPNR;
            set { _myPNR = value; OnPropertyChanged("MyPNR"); }
        }
        public List<MyPNR> PMS
        {
            get => _PMS;
            set { _PMS = value; OnPropertyChanged("PMS"); }
        }
        public List<MyPNR> SNC
        {
            get => _SNC;
            set { _SNC = value; OnPropertyChanged("SNC"); }
        }
        public List<MyPNR> KE24
        {
            get => _KE24;
            set { _KE24 = value; OnPropertyChanged("KE24"); }
        }
        public List<string> PNRPMS
        {
            get => _PNRPMS;
            set { _PNRPMS = value; OnPropertyChanged("PNRPMS"); }
        }
        public List<string> PNRSNC
        {
            get => _PNRSNC;
            set { _PNRSNC = value; OnPropertyChanged("PNRSNC"); }
        }
        public string PNRSearch
        {
            get => _PNRSearch;
            set { _PNRSearch = value; OnPropertyChanged("PNRSearch"); }
        }
        public List<string> UniquePNR
        {
            get => _uniquePNR;
            set { _uniquePNR = value; OnPropertyChanged("UniquePNR"); }
        }
        public List<string> ToBeSorted
        {
            get => _toBeSorted;
            set { _toBeSorted = value; OnPropertyChanged("ToBeSorted"); }
        }
        public ObservableCollection<string> PNRList
        {
            get => _PNRList;
            set { _PNRList = value; OnPropertyChanged("PNRList"); }
        }
        public Dictionary<string, int> colInfo
        {
            get => _colInfo;
            set { _colInfo = value; OnPropertyChanged("colInfo"); }
        }
        public Dictionary<string, MyPNR> PNRDic
        {
            get => _PNRDic;
            set { _PNRDic = value; OnPropertyChanged("PNRDic"); }
        }
        #endregion

        #region Commands
        public ICommand TestCommand { get; set; }
        #endregion

        #region Initialise
        public void Initialise()
        {
            //Required for EEPLUS to be used
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //Commands
            TestCommand = new RelayCommand(o => test(), o => true);
            //Extracting data
            FileInfo SNCFile = new FileInfo(@"C:\Users\msag\Desktop\PNRSorter\Families_S_C_v3.xlsx");
            FileInfo PMSFile = new FileInfo(@"C:\Users\msag\Desktop\PNRSorter\Families_PMS_v3.xlsx");
            FileInfo KE24File = new FileInfo(@"C:\Users\msag\Desktop\PNRSorter\KE24_2020.xlsx");
            //List for the autocompletion
            PNRList = new ObservableCollection<string>();
            PNRDic = new Dictionary<string, MyPNR>();
            //List of already classified PNRs
            PNRPMS = new List<string>();
            PNRSNC = new List<string>();
            SNC = ExtractHierarchy(SNCFile, "S&C");
            PMS = ExtractHierarchy(PMSFile, "PMS");
            //Selecting the desired values
            colInfo = new Dictionary<string, int>();
            colInfo.Add("Product", -1);
            colInfo.Add("Sales", -1);
            colInfo.Add("Billing Quantity", -1);
            FindColumn(KE24File, colInfo);
            foreach (var key in colInfo.Keys)
                Console.WriteLine("key: " + key + ", colNb: " + colInfo[key].ToString());
            GenerateDB(KE24File);
            //Isolating PNRs
            //UniquePNR = FindUnique(KE24);
            ToBeSorted = FilterExistingPNR(PNRDic.Keys.ToList());
            //Verification procedures
        }
        #endregion

        #region Constructor
        public ViewModel()
        {
            Initialise();
        }
        #endregion

        public void test()
        {
            //var file = new FileInfo(@"L:\Engineering_Energy\Monthly_ProductLine_Reviews\_Dashboard\Families_PMS_v2.xlsx");
            Console.WriteLine("total unique: " + UniquePNR.Count());
            Console.WriteLine("total to be sorted: " + ToBeSorted.Count());
        }

        #region Private Methods
        private List<MyPNR> ExtractHierarchy(FileInfo file, string group)
        {
            List<MyPNR> ExtractHierarchy = new List<MyPNR>();
            using (var package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                int rows = worksheet.Dimension.Rows;
                int col = worksheet.Dimension.Columns;
                for (int i = 1; i < col + 1; i++)
                {
                    for (int j = 1; j < rows + 1; j++)
                    {
                        MyPNR curPNR = new MyPNR();
                        if (worksheet.Cells[j, i].Value.ToString() != null)
                        {
                            string PNR = worksheet.Cells[j, i].Value.ToString();
                            if (!PNRDic.Keys.Contains(PNR))
                            {
                                if (group == "S&C")
                                    PNRSNC.Add(curPNR.PNR);
                                else
                                    PNRPMS.Add(curPNR.PNR);
                                curPNR.Family = worksheet.Cells[1, i].Value.ToString();
                                curPNR.Group = group;
                                curPNR.PNR = worksheet.Cells[j, i].Value.ToString();
                                curPNR.Sales = 0;
                                curPNR.UnitSold = 0;
                                PNRDic.Add(PNR, curPNR);
                            }
                            else
                                Console.WriteLine("DOUBLON: " + PNR);
                        }
                    }
                }
            }
            return ExtractHierarchy;
        }

        private void GenerateDB(FileInfo file)
        {
            using (var package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                int rows = worksheet.Dimension.Rows;
                for (int j = 1; j < rows + 1; j++)
                {
                    if(worksheet.Cells[j, _productCol].Value != null)
                    {
                        string PNR = worksheet.Cells[j, _productCol].Value.ToString();
                        if (PNRDic.Keys.Contains(PNR))
                        {
                            PNRDic[PNR].Sales += Convert.ToDouble(worksheet.Cells[j, _salesCol].Value);
                            PNRDic[PNR].UnitSold += Convert.ToDouble(worksheet.Cells[j, _unitSoldCol].Value);
                        }
                        else
                        {
                            MyPNR curPNR = new MyPNR();
                            curPNR.PNR = PNR;
                            curPNR.Family = "unknown";
                            curPNR.Group = "unknown";
                            curPNR.Sales = Convert.ToDouble(worksheet.Cells[j, _salesCol].Value);
                            curPNR.UnitSold = Convert.ToDouble(worksheet.Cells[j, _unitSoldCol].Value);
                            PNRDic.Add(PNR, curPNR);
                        }
                    }
                } 
            }
        }

        private List<string> FindUnique(List<MyPNR> pnrList)
        {
            List<string> uniquePNR = new List<string>();
            foreach (var pnr in pnrList)
            {
                if (!uniquePNR.Contains(pnr.PNR))
                {
                    uniquePNR.Add(pnr.PNR);
                    PNRList.Add(pnr.PNR);
                }
            }

            return uniquePNR;
        }

        private List<string> FilterExistingPNR(List<string> uniquePNR)
        {
            List<string> toBeSorted = new List<string>();
            foreach (var pnr in uniquePNR)
            {
                if (!(PNRPMS.Contains(pnr) || PNRSNC.Contains(pnr)))
                    toBeSorted.Add(pnr);
            }

            return toBeSorted;
        }

        private void FindColumn(FileInfo excel, Dictionary<string, int> name)
        {
            using (var package = new ExcelPackage(excel))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                int columns = worksheet.Dimension.Columns;
                for (int i = 1; i < columns + 1; i++)
                {
                    if (name.Keys.Contains(worksheet.Cells[1, i].Value.ToString()))
                    {
                        name[worksheet.Cells[1, i].Value.ToString()] = i;
                    }
                }
            }

            return;
        }
        #endregion

        #region Research
        private MyPNR PNRFetcher(FileInfo excel, string pnrNb)
        {
            using (var package = new ExcelPackage(excel))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                for (int j = 1; j < (_colNb + 1); j++)
                {
                    //if(worksheet.Cells[j, _productCol].Value.ToString() == pnrNb)
                }
            }
            MyPNR pnr = new MyPNR();
            return pnr;
        }
        #endregion
    }
}
