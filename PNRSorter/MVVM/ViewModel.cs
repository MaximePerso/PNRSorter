﻿using Microsoft.Win32;
using OfficeOpenXml;
using PNRSorter.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Office.Interop.Excel;

namespace PNRSorter.MVVM
{
    public class ViewModel : VMBase
    {

        #region Fields
        //Config file chaos
        private FileInfo _configFile;
        private ObservableCollection<FileInfo> _fileList;
        private FileInfo _selectedFile;
        private FileInfo _KE24File;
        #region PNR manipulation
        private ObservableCollection<MyPNR> _myPNR;
        private ObservableCollection<string> _groups;
        private ObservableCollection<string> _families;
        private ObservableCollection<Group> _hierarchy;
        private Dictionary<string, Dictionary<string, List<string>>> _PNRHierarchy;
        private List<string> _uniquePNR;
        private ObservableCollection<ListItems> _toBeSorted;
        private string _PNRSearch;
        private ObservableCollection<string> _PNRList;
        private int _salesCol;
        private int _productCol;
        private int _unitSoldCol;
        private Dictionary<string, int> _colInfo = new Dictionary<string, int>();
        private Dictionary<string, MyPNR> _PNRDic;
        private double _previousMinSales;
        private string _previousPNR;
        private ObservableCollection<ListItems> _toBeSortedIni;
        private Dictionary<string, List<string>> _groupToFam;
        private SavedFile _config;
        #endregion
        #region GUI variables
        private double _curSales;
        private double _curQT;
        private string _curGroup;
        private string _curFam;
        private string _errorMsg;
        private double _minSales;
        private string _PNRFilter;
        private int _nbTBS;
        private string _selectedGroup;
        private string _selectedFam;
        private string _clickedPNR;
        #endregion
        #region Edit window
        private Object _selectedTreeItem;
        private string _modifySelection;
        private string _newGroup;
        private string _newFamily;
        private ObservableCollection<string> _groupList;
        private string _myGroup;
        #endregion
        //test variables
        private string pouet;
        #endregion

        #region Properties
        #region Config File Chaos
        public FileInfo ConfigFile
        {
            get => _configFile;
            set { _configFile = value; OnPropertyChanged("ConfigFile"); }
        }
        public ObservableCollection<FileInfo> FileList
        {
            get => _fileList;
            set { _fileList = value; OnPropertyChanged("FileList"); }
        }
        public FileInfo SelectedFile
        {
            get => _selectedFile;
            set { _selectedFile = value; OnPropertyChanged("SelectedFile"); }
        }
        public FileInfo KE24File
        {
            get => _KE24File;
            set { _KE24File = value; OnPropertyChanged("KE24File"); }
        }
        public SavedFile Config
        {
            get => _config;
            set { _config = value; OnPropertyChanged("Config"); }
        }
        #endregion
        #region Backend prop
        public ObservableCollection<MyPNR> MyPNR
        {
            get => _myPNR;
            set { _myPNR = value; OnPropertyChanged("MyPNR"); }
        }
        public ObservableCollection<string> Groups
        {
            get => _groups;
            set { _groups = value; GetAssociatedFam(); OnPropertyChanged("Groups"); }
        }
        public ObservableCollection<string> Families
        {
            get => _families;
            set { _families = value; GetAssociatedGroup(); OnPropertyChanged("Families"); }
        }
        public ObservableCollection<Group> Hierarchy
        {
            get => _hierarchy;
            set 
            { 
                _hierarchy = value;  
                OnPropertyChanged("Hierarchy");
            }
        }
        public Dictionary<string, Dictionary<string,List<string>>> PNRHierarchy
        {
            get => _PNRHierarchy;
            set { _PNRHierarchy = value; OnPropertyChanged("PNRHierarchy"); }
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
        public ObservableCollection<ListItems> ToBeSorted
        {
            get => _toBeSorted;
            set { _toBeSorted = value; UpdateCount(); OnPropertyChanged("ToBeSorted"); }
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
        public Dictionary<string, List<string>> GroupToFam
        {
            get => _groupToFam;
            set { _groupToFam = value; OnPropertyChanged("GroupToFam"); }
        }
        #endregion
        #region GUI prop
        public double CurSales
        {
            get => _curSales;
            set { _curSales = value; OnPropertyChanged("CurSales"); }
        }
        public double CurQt
        {
            get => _curQT;
            set { _curQT = value; OnPropertyChanged("CurQt"); }
        }
        public string CurFam
        {
            get => _curFam;
            set { _curFam = value; OnPropertyChanged("CurFam"); }
        }
        public string CurGroup
        {
            get => _curGroup;
            set { _curGroup = value; OnPropertyChanged("CurGroup"); }
        }
        public string ErrorMsg
        {
            get => _errorMsg;
            set { _errorMsg = value; OnPropertyChanged("ErrorMsg"); }
        }
        public double MinSales
        {
            get => _minSales;
            set { _minSales = value; UpdateMainWin(); OnPropertyChanged("MinSales"); }
        }
        public string PNRFilter
        {
            get => _PNRFilter;
            set { _PNRFilter = value; UpdateMainWin(); OnPropertyChanged("PNRFilter"); }
        }
        public int NbTBS
        {
            get => _nbTBS;
            set { _nbTBS = value; OnPropertyChanged("NbTBS"); }
        }
        public string SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; GetAssociatedFam(); OnPropertyChanged("SelectedGroup"); }
        }
        public string SelectedFam
        {
            get => _selectedFam;
            set { _selectedFam = value; GetAssociatedGroup(); OnPropertyChanged("SelectedFam"); }
        }
        public string ClickedPNR
        {
            get => _clickedPNR;
            set { _clickedPNR = value; OnPropertyChanged("ClickedPNR"); }
        }
        #endregion
        #region Edit win
        public Object SelectedTreeItem
        {
            get => _selectedTreeItem;
            set { _selectedTreeItem = value; UpdateEditWin(); OnPropertyChanged("SelectedTreeItem"); }
        }
        public string ModifySelection
        {
            get => _modifySelection;
            set { _modifySelection = value; OnPropertyChanged("ModifySelection"); }
        }
        public string NewGroup
        {
            get => _newGroup;
            set { _newGroup = value; OnPropertyChanged("NewGroup"); }
        }
        public string NewFamily
        {
            get => _newFamily;
            set { _newFamily = value; OnPropertyChanged("NewFamily"); }
        }
        public ObservableCollection<string> GroupList
        {
            get => _groupList;
            set { _groupList = value; OnPropertyChanged("GroupList"); }
        }
        public string MyGroup
        {
            get => _myGroup;
            set { _myGroup = value; OnPropertyChanged("MyGroup"); }
        }
        #endregion
        #region Test
        public string Pouet
        {
            get => pouet;
            set { pouet = value; Console.WriteLine("POUETPOUETPOUETPOUET" + Pouet); OnPropertyChanged("Pouet"); }
        }
        #endregion
        #endregion

        #region Commands
        public ICommand TestCommand { get; set; }
        public ICommand UpdateCmd { get; set; }
        public ICommand DisplayDataCmd { get; set; }
        public ICommand ResetCmd { get; set; }
        public ICommand SelectAllCmd { get; set; }
        public ICommand EditCmd { get; set; }
        public ICommand DeleteItemCmd { get; set; }
        public ICommand RenameItemCmd { get; set; }
        public ICommand AddGroupCmd { get; set; }
        public ICommand AddFamilyCmd { get; set; }
        public ICommand LoadKE24Cmd { get; set; }
        public ICommand LinkPNRCmd { get; set; }
        #endregion

        #region Initialise
        private void InitialiseCommands()
        {
            //Commands
            TestCommand = new RelayCommand(o => UpdateExcelPNR(), o => true);
            UpdateCmd = new RelayCommand(o => UpdateExcelHeader(), o => true);
            DisplayDataCmd = new RelayCommand(o => DisplayData(), o => true);
            ResetCmd = new RelayCommand(o => Reset(), o => true);
            SelectAllCmd = new RelayCommand(o => SelectAll(), o => true);
            EditCmd = new RelayCommand(o => Edit(), o => true);
            DeleteItemCmd = new RelayCommand(o => DeleteItem(), o => { return SelectedTreeItem != null; });
            RenameItemCmd = new RelayCommand(o => RenameItem(), o => { return SelectedTreeItem != null; });
            AddGroupCmd = new RelayCommand(o => AddGroup(), o => { return NewGroup != null; });
            AddFamilyCmd = new RelayCommand(o => AddFamily(), o => { return ((MyGroup != null) && (NewFamily != "")); });
            LoadKE24Cmd = new RelayCommand(o => LoadKE24(), o => true);
            LinkPNRCmd = new RelayCommand(o => LinkPNR(), o => { return (SelectedGroup != "") || (SelectedFam != ""); });
        }

        public void Initialise()
        {
            //Required for EEPLUS to be used
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //Commands
            InitialiseCommands();
            //Default config file location
            ConfigFile = new FileInfo(@"\\vm.dom\ns1\DATA\Engineering_Energy\Monthly_ProductLine_Reviews\_Dashboard\PNRSorter\configPNRSorter.txt");
            //Extracting data
            //FileInfo KE24File = new FileInfo(@"C:\Users\msag\Desktop\PNRSorter\KE24_Extract_Total.xlsx");
            Config = LoadConfig();

            //List for the autocompletion
            PNRDic = new Dictionary<string, MyPNR>();

            //Extracting data from files
            Hierarchy = new ObservableCollection<Group>();
            Families = new ObservableCollection<string>();
            ExtractHierarchy(Config);

            //Variables for the treeview in Edit Window
            ToBeSorted = new ObservableCollection<ListItems>();
            _toBeSortedIni = new ObservableCollection<ListItems>();

            //Isolating PNRs
            //UniquePNR = FindUnique(KE24);
            //ToBeSorted = new ObservableCollection<string>(FilterExistingPNR(PNRDic.Keys.ToList()));
            //GUI Initialisation
            CurGroup = "N/A";
            CurFam = "N/A";
            CurSales = 0.0;
            CurQt = 0.0;
            ErrorMsg = "";
            MinSales = 0;
            PNRFilter = "";
            SelectedGroup = "";
            NewFamily = "";
            //Win GUI intialisation
            //Verification procedures
            Pouet = "";

            SaveConfig(Config);
        }

        #endregion

        #region Constructor
        public ViewModel()
        {
            Initialise();
        }
        #endregion

        #region Command Methods
        private void DisplayData()
        {
            try
            {
                CurGroup = PNRDic[PNRSearch].Group;
                CurFam = PNRDic[PNRSearch].Family;
                CurSales = PNRDic[PNRSearch].Sales;
                CurQt = PNRDic[PNRSearch].UnitSold;
                ErrorMsg = "";
            }
            catch
            {
                ErrorMsg = "Something went wrong, please check PNR";
            }
        }

        private void Reset()
        {
            MinSales = 0;
            PNRFilter = "";
            ToBeSorted = _toBeSortedIni;
            _nbTBS = ToBeSorted.Count();
            //unselecting all PNR
            foreach (var pnr in ToBeSorted)
                pnr.IsSelected = false;
        }

        private void SelectAll()
        {
            foreach (var pnr in ToBeSorted)
                pnr.IsSelected = true;
        }

        private void UpdateCount()
        {
            NbTBS = ToBeSorted.Count();
        }

        private void CreateHierarchy()
        {
            foreach (var group in Groups)
            {
                if (group != "")
                {
                    ObservableCollection<Family> famCollection = new ObservableCollection<Family>();
                    foreach (var fam in PNRHierarchy[group].Keys)
                        famCollection.Add(new Family(fam));
                    Hierarchy.Add(new Group(group, famCollection));
                }
            }
        }

        private void Edit()
        {
            EditGroupsAndFamilies editWin = new EditGroupsAndFamilies();
            editWin.Show();
        }

        public void DeleteItem()
        {
            if (SelectedTreeItem.GetType() == typeof(Group))
            {
                foreach (var group in Hierarchy)
                {
                    if (group.GroupName == ModifySelection)
                    {
                        string ini = ModifySelection;
                        Hierarchy.Remove(group);
                        GroupList.Remove(ini);
                        break;
                    }
                }
            }
            else
            {
                foreach (var group in Hierarchy)
                {
                    foreach (var family in group.Families)
                    {
                        if (family.FamilyName == ModifySelection)
                        {
                            group.Families.Remove(family);
                            break;
                        }
                    }
                }
            }
        }

        public void RenameItem()
        {
            if (SelectedTreeItem.GetType() == typeof(Group))
            {
                foreach (var group in Hierarchy)
                {
                    if (group.GroupName == ((Group)SelectedTreeItem).GroupName)
                    {
                        //we need to keep somewhere the previous word, otherwise it cannot find it in the list GroupList
                        string iniName = group.GroupName;
                        group.GroupName = ModifySelection;
                        for(int i =0; i<GroupList.Count();i++)
                        {
                            if(GroupList[i] == iniName)
                            {
                                GroupList[i] = ModifySelection;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                foreach (var group in Hierarchy)
                {
                    foreach (var family in group.Families)
                    {
                        if (family.FamilyName == ((Family)SelectedTreeItem).FamilyName)
                        {
                            family.FamilyName = ModifySelection;
                            break;
                        }
                    }
                }
            }
        }

        private void AddGroup()
        {
            Hierarchy.Add(new Group(NewGroup, new ObservableCollection<Family>()));
            GroupList.Add(NewGroup);
        }

        private void AddFamily()
        {
            foreach (var group in Hierarchy)
            {
                if(group.GroupName == MyGroup)
                {
                    group.Families.Add(new Family(NewFamily));
                    break;
                }
            }
        }

        private void SaveEdits()
        {
            if(MessageBox.Show("Attention ma cacahuette, tu es sur le point de modifier la hierarchie des données, on continue ?", "tu crois vraiment que je n'ai rien d'autre à faire que d'écrire une caption ?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach(Group group in Hierarchy)
                {
                    if (!Groups.Contains(group.GroupName))
                    {
                    }
                }
            }
        }

        private void LoadKE24()
        {
            OpenFileDialog ke24 = new OpenFileDialog();
            ke24.DefaultExt = "xls, xlsx";
            ke24.InitialDirectory = ConfigFile.DirectoryName;
            ke24.ShowDialog();
            GetKE24Data(new FileInfo(ke24.FileName));
            UpdateCount();
        }

        private void LinkPNR()
        {
            if(SelectedGroup != "")
            {
                if(SelectedFam != "")
                {
                    List<string> _selectedPNR = new List<string>();
                    List<string> _doublon = new List<string>();

                    foreach(var pnr in ToBeSorted)
                    {
                        //On récupère les PNRs selectionnés et on verifie qu'ils ne soient pas déjà classé
                        if (pnr.IsSelected)
                        {
                            _selectedPNR.Add(pnr.PNRName);
                            //Are there PNR that were already sorted
                            if (!ToBeSorted.Contains(pnr))
                                _doublon.Add(pnr.PNRName);
                        }

                        if(_doublon.Count() != 0)
                        {
                            string _stringDoublon = "";
                            foreach (string dbl in _doublon)
                                _stringDoublon += dbl + " ";
                            if(MessageBox.Show("The following PNRs have already been classified. Should they be moved to this new Group/Family ? \r This will remove their previous classification"," ", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            {
                                foreach(var group in PNRHierarchy.Keys)
                                {
                                    foreach(var fam in PNRHierarchy[group].Keys)
                                    {
                                        foreach(var dbl in _doublon)
                                        {
                                            for(int i =0; i< PNRHierarchy[group][fam].Count(); i++)
                                            {
                                                if(PNRHierarchy[group][fam][i] == dbl)
                                                {
                                                    PNRHierarchy[group][fam].RemoveAt(i);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                    foreach (string pnr in _selectedPNR)
                    {
                        //Add the PNR to dictionnary
                        PNRHierarchy[SelectedGroup][SelectedFam].Add(pnr);
                        //Remove PNR from the displayed list
                        for (int i = 0; i < ToBeSorted.Count(); i++)
                        {
                            if (ToBeSorted[i].PNRName == pnr)
                                ToBeSorted.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a Family first");
                }
            }
            else
            {
                MessageBox.Show("Please selected a Group first");
            }
        }

        private void UpdateExcelHeader()
        {
            foreach(var famFile in Config.GroupList)
            {
                //Excel Variable
                Microsoft.Office.Interop.Excel.Application oXL;
                Microsoft.Office.Interop.Excel._Workbook oWB;
                Microsoft.Office.Interop.Excel._Worksheet oSheet;
                Microsoft.Office.Interop.Excel.Range oRng;
                object misvalue = System.Reflection.Missing.Value;
                FileInfo file = new FileInfo(famFile.Path);
                foreach (var group in Hierarchy)
                {
                    if (group.GroupName == famFile.Name)
                    {
                        try
                        {
                            //Start Excel
                            oXL = new Microsoft.Office.Interop.Excel.Application();
                            oXL.Visible = true;

                            //Get proper sheet
                            //oWB = (Microsoft.Office.Interop.Excel._Workbook)(oXL.Workbooks[0]);
                            oWB = oXL.Workbooks.Open(famFile.Path);
                            oSheet = (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet;

                            //Create first column
                            oSheet.Cells[1, 1] = "Family Name";
                            oSheet.Cells[2, 1] = group.GroupName;
                            //Create headers
                            for (int i = 2; i < group.Families.Count() + 2; i++)
                            {
                                string curFam = group.Families[i - 2].FamilyName;
                                oSheet.Cells[1, i] = curFam;
                            }

                            //Fermeture du fichier
                            oXL.Visible = false;
                            oXL.UserControl = false;
                            oWB.Save();
                            oWB.Close();
                            oXL.Quit();
                        }
                        catch { }
                    }
                }
            }
            return;
        }

        private void UpdateExcelPNR()
        {
            foreach (var famFile in Config.GroupList)
            {
                //Excel Variable
                Microsoft.Office.Interop.Excel.Application oXL;
                Microsoft.Office.Interop.Excel._Workbook oWB;
                Microsoft.Office.Interop.Excel._Worksheet oSheet;
                Microsoft.Office.Interop.Excel.Range oRng;
                object misvalue = System.Reflection.Missing.Value;
                FileInfo file = new FileInfo(famFile.Path);
                foreach (var group in Hierarchy)
                {
                    if (group.GroupName == famFile.Name)
                    {
                        try
                        {
                            //Start Excel
                            oXL = new Microsoft.Office.Interop.Excel.Application();
                            oXL.Visible = true;

                            //Get proper sheet
                            //oWB = (Microsoft.Office.Interop.Excel._Workbook)(oXL.Workbooks[0]);
                            oWB = oXL.Workbooks.Open(famFile.Path);
                            oSheet = (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet;

                            //Create headers
                            for (int i = 2; i < group.Families.Count() + 2; i++)
                            {
                                string curFam = group.Families[i - 2].FamilyName;
                                //Fill with family PNRs
                                for (int j = 2; j < PNRHierarchy[group.GroupName][curFam].Count() + 2; j++)
                                {
                                    oSheet.Cells[j, i] = PNRHierarchy[group.GroupName][curFam][j - 2];
                                }
                            }

                            //Fermeture du fichier
                            oXL.Visible = false;
                            oXL.UserControl = false;
                            oWB.Save();
                            oWB.Close();
                            oXL.Quit();
                        }
                        catch { }
                    }
                }
            }
            return;
        }

        #endregion

        public void test()
        {
            //var file = new FileInfo(@"L:\Engineering_Energy\Monthly_ProductLine_Reviews\_Dashboard\Families_PMS_v2.xlsx");
            //Console.WriteLine("total unique: " + UniquePNR.Count());
            if (SelectedTreeItem.GetType() == typeof(Group))
                Console.WriteLine("GROUP" + SelectedTreeItem);
            else
                Console.WriteLine("FAMILY" + SelectedTreeItem);
        }


        #region Private Methods
        #region Config File Mayhem
        private SavedFile LoadConfig()
        {
            SavedFile savedFile = new SavedFile();
            if (!File.Exists(ConfigFile.FullName))
            {
                MessageBox.Show("No Config File Found, creating one: " + ConfigFile.FullName);
                if (!ConfigFile.Directory.Exists)
                {
                    try
                    {
                        ConfigFile.Directory.Create();
                    }
                    catch
                    {
                        MessageBox.Show("It is not looking greate my friend, something is going wrong.\r Check if you are connected to VM network.\r If yes, contact Adrien Corne and tell him \"Coconut\"");
                    }
                }
                //File.Create(ConfigFile.FullName);
                //ConfigFile.Create();
                savedFile = PopulateConfig();
            }
            else
            {
                var serialiser = new XmlSerializer(typeof(SavedFile));
                using (var fs = new FileStream(ConfigFile.FullName,FileMode.Open))
                {
                    try
                    {
                        savedFile = (SavedFile)serialiser.Deserialize(fs);
                    }
                    catch
                    {
                        Console.WriteLine("THERE WAS AN ERROR !");
                        PopulateConfig();
                    }
                }
            }

            //Group List
            Groups = new ObservableCollection<string>() { "" };
            //second group list used in the edit windows. Has to be different from "Groups" to allow the user not to save his modifications in the Edit Window. Does not include the initial ""
            GroupList = new ObservableCollection<string>();
            //Association Group Family PNR
            PNRHierarchy = new Dictionary<string, Dictionary<string, List<string>>>();

            //Populating each group based on the config file
            foreach (GroupFile groupName in savedFile.GroupList)
            {
                Groups.Add(groupName.Name);
                GroupList.Add(groupName.Name);
                //GroupToFam.Add(groupName.Name, new List<string>());
                ObservableCollection<string> newInstance = new ObservableCollection<string>();
                PNRHierarchy.Add(groupName.Name, new Dictionary<string, List<string>>());
            }

            return savedFile;
        }

        public void SaveConfig(SavedFile sf)
        {
            if(sf != null)
            {
                var doc = new XDocument();
                using (var writer = doc.CreateWriter())
                {
                    var serialiser = new XmlSerializer(typeof(SavedFile));
                    serialiser.Serialize(writer, sf);
                }
                doc.Save(ConfigFile.FullName);
            }
        }

        public SavedFile PopulateConfig()
        {
            SavedFile savedFile = new SavedFile();
            MessageBox.Show("No valid configuration file has been found. Please select at leat on .xlsx sorting product families");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "xls, xlsx";
            ofd.InitialDirectory = ConfigFile.DirectoryName;
            ofd.Multiselect = true;
            ofd.ShowDialog();
            foreach(var file in ofd.FileNames)
            {
                savedFile.GroupList.Add(new GroupFile() { Name = file.Split('\\').Last(), Path = file });
            }
            return savedFile;
        }

        public void GetKE24Data(FileInfo KE24File)
        {
            //Selecting desired value fields
            colInfo = new Dictionary<string, int>();
            colInfo.Add("Product", -1);
            colInfo.Add("Sales", -1);
            colInfo.Add("Billing Quantity", -1);

            //Look for the column numbers in KE24
            FindColumn(KE24File, colInfo);
            _productCol = colInfo["Product"];
            _salesCol = colInfo["Sales"];
            _unitSoldCol = colInfo["Billing Quantity"];

            //Synt. data from KE24 for each PNRs
            GenerateDB(KE24File);
            _nbTBS = ToBeSorted.Count();
            PNRList = new ObservableCollection<string>(PNRDic.Keys.ToList());
        }
        #endregion

        #region Main window
        //Create a dictionnary with the hierarchy Group -> Families -> PNRs
        private void ExtractHierarchy(SavedFile sf)
        {
            foreach(var sfFile in sf.GroupList)
            {
                FileInfo file = new FileInfo(sfFile.Path);
                using (var package = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    int rows = worksheet.Dimension.Rows;
                    int col = worksheet.Dimension.Columns;
                    //i and j start at 2 to ignore the first roy (header) and first column (file name)
                    for (int i = 2; i < col + 1; i++)
                    {
                        for (int j = 2; j < rows + 1; j++)
                        {
                            MyPNR curPNR = new MyPNR();
                            try
                            {
                                if (!string.IsNullOrEmpty(worksheet.Cells[j, i].Text))
                                {
                                    string PNR = worksheet.Cells[j, i].Value.ToString();
                                    if (!PNRDic.Keys.Contains(PNR))
                                    {
                                        curPNR.Family = worksheet.Cells[1, i].Value.ToString();
                                        if (!PNRHierarchy[sfFile.Name].ContainsKey(curPNR.Family))
                                            PNRHierarchy[sfFile.Name].Add(curPNR.Family, new List<string>());
                                        PNRHierarchy[sfFile.Name][curPNR.Family].Add(PNR);
                                        curPNR.Group = sfFile.Name;
                                        curPNR.PNR = worksheet.Cells[j, i].Value.ToString();
                                        curPNR.Sales = 0;
                                        curPNR.UnitSold = 0;
                                        PNRDic.Add(PNR, curPNR);
                                    }
                                    else
                                        Console.WriteLine("DOUBLON: " + PNR);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
            }
            CreateHierarchy();
        }

        private void GenerateDB(FileInfo file)
        {
            using (var package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                int rows = worksheet.Dimension.Rows;
                //rows have to start at 2, we don't want headers
                for (int j = 2; j < rows + 1; j++)
                {
                    if (worksheet.Cells[j, _productCol].Value != null)
                    {
                        string PNR = worksheet.Cells[j, _productCol].Value.ToString();
                        MyPNR curPNR = new MyPNR();
                        if (PNRDic.Keys.Contains(PNR))
                        {
                            //To be uncommented if data is also wanted for PNRs that have already been classified 
                            PNRDic[PNR].Sales += Convert.ToDouble(worksheet.Cells[j, _salesCol].Value);
                            PNRDic[PNR].UnitSold += Convert.ToDouble(worksheet.Cells[j, _unitSoldCol].Value);
                        }
                        else
                        {
                            curPNR.PNR = PNR;
                            curPNR.Family = "unknown";
                            curPNR.Group = "unknown";
                            if (PNRDic.Keys.Contains("PNR"))
                            {
                                PNRDic[PNR].Sales += Convert.ToDouble(worksheet.Cells[j, _salesCol].Value);
                                PNRDic[PNR].UnitSold += Convert.ToDouble(worksheet.Cells[j, _unitSoldCol].Value);
                            }
                            else
                            {
                                curPNR.Sales = Convert.ToDouble(worksheet.Cells[j, _salesCol].Value);
                                curPNR.UnitSold = Convert.ToDouble(worksheet.Cells[j, _unitSoldCol].Value);
                                PNRDic.Add(PNR, curPNR);
                                var loc = new ListItems();
                                loc.PNRName = PNR;
                                loc.IsSelected = false;
                                ToBeSorted.Add(loc);
                                _toBeSortedIni.Add(loc);
                            }
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

        #region Edit Win
        private void UpdateMainWin()
        {
            List<ListItems> tempString = new List<ListItems>();
            if ((MinSales < _previousMinSales) || PNRFilter != _previousPNR)
                ToBeSorted = _toBeSortedIni;
            foreach (var pnr in ToBeSorted)
            {
                if ((PNRFilter != null) && (PNRDic[pnr.PNRName].Sales > MinSales) && pnr.PNRName.StartsWith(PNRFilter))
                    tempString.Add(pnr);
            }
            //ToBeSorted.Clear();
            _previousMinSales = MinSales;
            _previousPNR = PNRFilter;
            ToBeSorted = new ObservableCollection<ListItems>(tempString);
        }

        private void UpdateEditWin()
        {
            if (SelectedTreeItem != null)
            {
                if (SelectedTreeItem.GetType() == typeof(Group))
                    ModifySelection = ((Group)SelectedTreeItem).GroupName;
                else
                    ModifySelection = ((Family)SelectedTreeItem).FamilyName;
            }
        }

        private void BackUpFiles()
        {

        }
        #endregion

        #endregion

        #region Research
        private void GetAssociatedFam()
        {
            {
                if (SelectedGroup != null)
                {
                    Families.Clear();
                    if (SelectedGroup == "")
                    {
                        foreach (var group in PNRHierarchy.Keys)
                        {
                            foreach (var fam in PNRHierarchy[group].Keys)
                                Families.Add(fam);
                        }
                    }
                    else
                    {
                        foreach (var fam in PNRHierarchy[SelectedGroup].Keys)
                            Families.Add(fam);
                    }
                }
            }
        }

        private void GetAssociatedGroup()
        {
            if ((SelectedGroup == null) || (SelectedGroup == ""))
            {
                if (SelectedFam != null)
                {
                    foreach (var group in PNRHierarchy.Keys)
                    {
                        if (PNRHierarchy[group].ContainsKey(SelectedFam))
                            SelectedGroup = group;
                    }
                }
            }
        }
        #endregion

    }
}
