﻿using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.IO;
using System.Timers;

using bpac;

using FastReport.Barcode;
using FastReport;




namespace MESPrintService
{
    public partial class Service1 : ServiceBase
    {



        public Service1()
        {
            InitializeComponent();
            System.Timers.Timer _MyTimer = new System.Timers.Timer();

            _MyTimer.Interval = 5 * 1000;
            //_MyTimer.AutoReset = true;

            // Start the timer
            _MyTimer.Enabled = true;

            Console.WriteLine("Press the Enter key to exit the program at any time... ");
            Console.ReadLine();

            _MyTimer.Elapsed += _MyTimerElapsed;

        }



        protected override void OnStart(string[] args)
        {
            Debugger.Launch();
            FileStream stream2 = File.Open(@"c:\brother\PrintServiceLog.txt", FileMode.OpenOrCreate, FileAccess.Write);
            stream2.Seek(0, SeekOrigin.Begin);
            stream2.SetLength(0); //清空txt文件
            stream2.Close();
            _MyTimer.Start();

        }

        protected override void OnStop()
        {
            _MyTimer.Stop();

        }


        private List<string> GetCSVData(string filePath)
        {

            try
            {
                Encoding encoding = Encoding.UTF8;

                //System.Text.Encoding  encoding = GetType(filePath); //Encoding.ASCII;//
                // DataTable dt = new DataTable();

                System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                // System.IO.StreamReader sr = new System.IO.StreamReader(fs, encoding);
                System.IO.StreamReader sr = new System.IO.StreamReader(fs, System.Text.Encoding.UTF8);

                //记录每次读取的一行记录

                string strLine = "";

                //记录每行记录中的各字段内容

                string[] aryLine = null;

                //string[] tableHead = null;

                //标示列数

                //  int columnCount = 0;

                //标示是否是读取的第一行

                bool IsFirst = true;

                //逐行读取CSV中的数据

                while ((strLine = sr.ReadLine()) != null)

                {
                    if (IsFirst == true)
                    {
                        aryLine = strLine.Split(new string[] { "#*#" }, StringSplitOptions.None);
                        //  aryLine = Regex.Split(strLine, "#*#", RegexOptions.IgnoreCase);
                        //  string[] sArray = Regex.Split(strLine, "#*#", RegexOptions.IgnoreCase);
                    }
                }

                sr.Close();
                fs.Close();

                return new List<System.String>(aryLine);
            }
            catch
            { return null; }
        }


        public void DeleteFile(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }

        public string AddLetter(string mystring)
        {

            string tem = " \"" + mystring + " \"";
            return tem;
        }


     public void   moveFile(string path, string OldFile, StreamWriter sw )
        {
            
            string NewFile;

         
            NewFile = path + OldFile;
            string oldfile = @"c:\brother\" + OldFile;

          

            File.Copy(oldfile, NewFile, true);

            if (File.Exists(oldfile))
                DeleteFile(oldfile);


            
            sw.WriteLine("-----------Error file have moved to :"  + NewFile   +  "------------ -" );
            sw.WriteLine("-----------------------------------error---------------------------------------");
            sw.Flush();
            sw.Close();


        }

        public string GetPrinterName(string Location)
        {

            string filePath = @"c:\brother\SelectPrinter.ini";



            try
            {
                Encoding encoding = Encoding.UTF8;


                System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                System.IO.StreamReader sr = new System.IO.StreamReader(fs, System.Text.Encoding.UTF8);
                string strLine = "";
                string[] aryLine = null;
                bool IsFirst = true;
                //逐行读取CSV中的数据
                while ((strLine = sr.ReadLine()) != null)

                {
                    if (IsFirst == true)
                    {
                        aryLine = strLine.Split(new string[] { ":" }, StringSplitOptions.None);
                        if (aryLine[0].Equals(Location))
                        {
                            return aryLine[1];
                        }

                    }
                    else
                    {
                        return "NoPrinter";
                    }
                }

                sr.Close();
                fs.Close();
                return "NoPrinter";

            }
            catch
            { return null; }




        }




        public bool IsInteger(string strIn)
        {
            bool bolResult = true;
            if (strIn == "")
            {
                bolResult = false;
            }
            else
            {
                foreach (char Char in strIn)
                {
                    if (char.IsNumber(Char))
                        continue;
                    else
                    {
                        bolResult = false;
                        break;
                    }
                }
            }
            return bolResult;
        }



        internal void _MyTimerElapsed(object sender, ElapsedEventArgs e)
        {

            string logFileName = @"C:\\brother\PrintServiceLog.txt"; // 文件路径
            List<string> myList = new List<string>();
            FileStream fs = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);


            try
            {
 
                DirectoryInfo folder = new DirectoryInfo(@"c:\brother");
 
                List<string> FileName = new List<string>();
                List<string> FileSingleName = new List<string>();

                foreach (FileInfo file in folder.GetFiles("Label*.txt"))
                {
                    FileName.Add(file.FullName);
                    FileName.Sort();
                    FileSingleName.Add(file.Name);
                    FileSingleName.Sort();
                }

                    
            

              
                //        foreach (FileInfo file in folder.GetFiles("Label*.txt"))

                for (int j=0;j<=FileName.Count-1;j++)
                {
    
                                                                             //-----
              

                    FileInfo file = new FileInfo(FileName[j]);

                    myList = GetCSVData(file.FullName);



                    if (myList is null)
                    {
                        sw.WriteLine("  没有数据打印, 或者打印参数不对!");
                        sw.Flush();
                    }
                    else
                    {



                        string arg = "";
                        myList[0] = myList[0].Trim();
                        arg = myList[0].Replace("\"", "");
                        myList[0] = myList[0].Trim();
                        for (int i = 1; i <= myList.Count - 1; i++)
                        {
                            string tem = myList[i].Replace("\"", "");

                            arg = arg + "," + tem;

                        }


                         sw.WriteLine("日期:" + DateTime.Now.ToString() + "打印开始");
                        sw.WriteLine("打印文件名:" + file.FullName);
                        sw.WriteLine("打印参数数量:" + myList.Count().ToString());
                        sw.WriteLine("打印参数:" + arg);
                        sw.Flush();

          
                        string txt = "BASEINKSCREENFIXTURECOATINGMIXINKIQCGROUPLABELDILUTION";

                        if (!txt.Contains(myList[0]))
                        {
                             
                            sw.WriteLine("------------------------------------------error--------------------------------");
                            sw.WriteLine("------------参数不对,没有相应标签模板:"+ myList[0] +" 不打印------------ - " );
                            sw.WriteLine("-----------------------------------error---------------------------------------");
                            sw.Flush();
                            sw.Close();
                          moveFile(@"c:\brother\error\", FileSingleName[0], sw);

                            
                        }
                       


             

                        if (  myList[0].Trim().Equals("BASEINK"))
                        {
                            bpac.DocumentClass doc = new DocumentClass();
                       


                           // Console.WriteLine( "doc ok");
                            if (doc.Open(@"c:\brother\baseink.lbx"))
                            {
                                //BASEINK#*#IQC#*#SPX190300001#*#固化剂 Firming agent#*#MS-100#*#9/13/2020#*#10
                                {
                                  //  Console.WriteLine("Open OK");
                                    
                                    doc.GetObject("myBarcode").Text = myList[2];
                                    doc.GetObject("objOne").Text = myList[2];
                                 

                                    doc.GetObject("objTwo").Text = myList[3];

                                    doc.GetObject("objThree").Text = myList[4];
                                    doc.GetObject("objFour").Text = myList[5];
                                    //   int copys = int.Parse(myList[myList.Count()-1]);
                                    int copys = 1;
                                    if (IsInteger(myList[myList.Count() - 1]))
                                    {
                                        copys = int.Parse(myList[myList.Count() - 1]);
                                    }

                                    string myPrinter = GetPrinterName("BASEINK");
                                    if (!myPrinter.Equals("NoPrinter"))
                                    {
                                        doc.SetPrinter(myPrinter, true);

                                        sw.WriteLine("打印机:  " + myPrinter);

                                    }

                                    Boolean result = doc.StartPrint("", PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("开始准备打印,状态为:" + result.ToString());

                                    Boolean result2 = doc.PrintOut(copys, PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("正在打印,状态为:" + result2.ToString());

                                    sw.WriteLine("程序成功打印成功,正常退出！");
                                    doc.EndPrint();
                                }
                                doc.Close();
                            }


                        }


                        if (myList[0].Equals("SCREEN"))
                        {
                            bpac.DocumentClass doc = new DocumentClass();



                            // Console.WriteLine( "doc ok");
                            if (doc.Open(@"c:\brother\screen.lbx"))
                            {
                                //BASEINK#*#IQC#*#SPX190300001#*#固化剂 Firming agent#*#MS-100#*#9/13/2020#*#10
                                {
                                    //  Console.WriteLine("Open OK");

                                    doc.GetObject("myBarcode").Text = myList[2];
                                    doc.GetObject("objOne").Text = myList[2];
                           

                                    doc.GetObject("objTwo").Text = myList[3];

                                    doc.GetObject("objThree").Text = myList[4];
                                    doc.GetObject("objFour").Text = myList[5];
                                    doc.GetObject("objFive").Text = myList[6];
                                    // int copys = int.Parse(myList[myList.Count() - 1]);
                                    int copys = 1;
                                    if (IsInteger(myList[myList.Count() - 1]))
                                    {
                                        copys = int.Parse(myList[myList.Count() - 1]);
                                    }

                                    string myPrinter = GetPrinterName("SCREEN");
                                    if (!myPrinter.Equals("NoPrinter"))
                                    {
                                        doc.SetPrinter(myPrinter, true);

                                        sw.WriteLine("打印机:  " + myPrinter);

                                    }

                                    Boolean result = doc.StartPrint("", PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("开始准备打印,状态为:" + result.ToString());

                                    Boolean result2 = doc.PrintOut(copys, PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("正在打印,状态为:" + result2.ToString());

                                    sw.WriteLine("程序成功打印成功,正常退出！");
                                    doc.EndPrint();
                                }
                                doc.Close();
                            }


                        }

                        if (myList[0].Equals("FIXTURE"))
                        {
                            bpac.DocumentClass doc = new DocumentClass();



                            // Console.WriteLine( "doc ok");
                            if (doc.Open(@"c:\brother\fixture.lbx"))
                            {
                                //BASEINK#*#IQC#*#SPX190300001#*#固化剂 Firming agent#*#MS-100#*#9/13/2020#*#10
                                {
                                    //  Console.WriteLine("Open OK");

                                    doc.GetObject("myBarcode").Text = myList[2];
                                    doc.GetObject("objOne").Text = myList[2];


                                    doc.GetObject("objTwo").Text = myList[3];

                                    doc.GetObject("objThree").Text = myList[4];
                                    doc.GetObject("objFour").Text = myList[5];
                                    // int copys = 1;  //= int.Parse(myList[myList.Count() - 1]);

                                    int copys = 1;
                                    if (IsInteger(myList[myList.Count() - 1]))
                                    {
                                        copys = int.Parse(myList[myList.Count() - 1]);
                                    }



                                    string myPrinter = GetPrinterName("FIXTURE");
                                    if (!myPrinter.Equals("NoPrinter"))
                                    {
                                        doc.SetPrinter(myPrinter, true);

                                        sw.WriteLine("打印机:  " + myPrinter);

                                    }

                                    Boolean result = doc.StartPrint("", PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("开始准备打印,状态为:" + result.ToString());

                                    Boolean result2 = doc.PrintOut(copys, PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("正在打印,状态为:" + result2.ToString());

                                    sw.WriteLine("程序成功打印成功,正常退出！");
                                    doc.EndPrint();
                                }
                                doc.Close();
                            }


                        }


                        if (myList[0].Equals("COATING"))
                        {
                            bpac.DocumentClass doc = new DocumentClass();



                            // Console.WriteLine( "doc ok");
                            if (doc.Open(@"c:\brother\coating.lbx"))
                            {
                                //BASEINK#*#IQC#*#SPX190300001#*#固化剂 Firming agent#*#MS-100#*#9/13/2020#*#10
                                {
                                    //  Console.WriteLine("Open OK");

                                    doc.GetObject("myBarcode").Text = myList[2];
                                    doc.GetObject("objOne").Text = myList[2];


                                    doc.GetObject("objTwo").Text = myList[3];

                                    doc.GetObject("objThree").Text = myList[4];
                                    doc.GetObject("objFour").Text = myList[5];
                                    doc.GetObject("objFive").Text = myList[6];
                                    doc.GetObject("objFive").Text = myList[7];
                                    int copys = 1;
                                    if (IsInteger(myList[myList.Count() - 1]))
                                    {
                                          copys  = int.Parse(myList[myList.Count() - 1]);
                                    }
                                 
                                    string myPrinter = GetPrinterName("COATING");
                                    if (!myPrinter.Equals("NoPrinter"))
                                    {
                                        doc.SetPrinter(myPrinter, true);

                                        sw.WriteLine("打印机:  " + myPrinter);

                                    }

                                    Boolean result = doc.StartPrint("", PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("开始准备打印,状态为:" + result.ToString());

                                    Boolean result2 = doc.PrintOut(copys, PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("正在打印,状态为:" + result2.ToString());

                                    sw.WriteLine("程序成功打印成功,正常退出！");
                                    doc.EndPrint();
                                }
                                doc.Close();
                            }


                        }



                        if (myList[0].Equals("DILUTION"))
                        {
                            bpac.DocumentClass doc = new DocumentClass();
                            //pac.DocumentClass doc = new DocumentClass();


                            Console.WriteLine(DateTime.Now.ToString());
                            if (doc.Open(@"c:\brother\ec.lbx"))
                            {

                                {
                                    Console.WriteLine(DateTime.Now.ToString());

                                    doc.GetObject("myBarcode").Text = myList[2];
                                    doc.GetObject("name").Text = myList[2];
                                    doc.GetObject("desc").Text = myList[3];

                                    doc.GetObject("date1").Text = myList[4];

                                    int copys = 1;
                                    if (IsInteger(myList[myList.Count() - 1]))
                                    {
                                        copys = int.Parse(myList[myList.Count() - 1]);
                                    }





                                    string myPrinter = GetPrinterName("DILUTION");
                                    if ( !myPrinter.Equals("NoPrinter"))
                                        { 
                                    doc.SetPrinter(myPrinter, true);

                                    sw.WriteLine("打印机:  "+ myPrinter) ;

                                    }

                                    Boolean result = doc.StartPrint("", PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("开始准备打印,状态为:" + result.ToString());

                                    Boolean result2 = doc.PrintOut(copys, PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("正在打印,状态为:" + result2.ToString());

                                    sw.WriteLine("程序成功打印成功,正常退出！");
                                    doc.EndPrint();
                                }
                                doc.Close();
                            }

                        }



                        if (myList[0].Equals("MIXINK"))
                        {
                            bpac.DocumentClass doc = new DocumentClass();
                            //pac.DocumentClass doc = new DocumentClass();

                        //    bpac.Printer prn = new bpac.Printer();
                         //   prn.GetInstalledPrinters();

                            doc.SetPrinter("BrotherMixInk",true);


                            Console.WriteLine(DateTime.Now.ToString());
                            if (doc.Open(@"c:\brother\mixink.lbx"))
                            {

                                {
                                    Console.WriteLine(DateTime.Now.ToString());

                                    doc.GetObject("myBarcode").Text = myList[2];
                                    doc.GetObject("name").Text = myList[2];
                                    doc.GetObject("desc").Text = myList[3];
                                    doc.GetObject("level").Text = myList[6];
                                    doc.GetObject("date1").Text = myList[4];
                                    doc.GetObject("date2").Text = myList[5];
                                    //       sw.WriteLine("打印机:BrotherMixInk");

                                    int copys = 1;
                                    if (IsInteger(myList[myList.Count() - 1]))
                                    {
                                        copys = int.Parse(myList[myList.Count() - 1]);
                                    }


                                    string myPrinter = GetPrinterName("MIXINK");

                                    if (!myPrinter.Equals("NoPrinter"))
                                    {
                                        doc.SetPrinter(myPrinter, true);

                                        sw.WriteLine("打印机:  " + myPrinter);

                                    }


                                    Boolean result = doc.StartPrint("", PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("开始准备打印,状态为:" + result.ToString());

                                    Boolean result2 = doc.PrintOut(copys, PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("正在打印,状态为:" + result2.ToString());

                                    sw.WriteLine("程序成功打印成功,正常退出！");
                                    doc.EndPrint();
                                }
                                doc.Close();
                            }

                        }

                        if (myList[0].Equals("IQC"))
                        {
                            bpac.DocumentClass doc = new DocumentClass();
                            //pac.DocumentClass doc = new DocumentClass();
                            bpac.PrinterClass myPrinter = new PrinterClass();
                            object myobj = new object();
                            myobj = myPrinter.GetInstalledPrinters();



                            if (doc.Open(@"c:\brother\material.lbx"))
                            {

                                {
                                    Console.WriteLine(DateTime.Now.ToString());

                                    //   String[] myData = myList[0].ToString().Split('|');
                                    doc.GetObject("myBarcode").Text = myList[2];

                                    doc.GetObject("name").Text = myList[2];
                                    doc.GetObject("desc").Text = myList[3].Replace("\"", "");

                                    doc.GetObject("date1").Text = myList[4];
                                    int copys = 1;
                                    if (IsInteger(myList[myList.Count() - 1]))
                                    {
                                        copys = int.Parse(myList[myList.Count() - 1]);
                                    }


                                    string myP = GetPrinterName("IQC");

                                    if (!myP.Equals("NoPrinter"))
                                    {
                                        doc.SetPrinter(myP, true);

                                        sw.WriteLine("打印机:  " + myPrinter);

                                    }

                                    Boolean result = doc.StartPrint("", PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("开始准备打印,状态为:" + result.ToString());

                                 //   int CopyCount = int.Parse(myList[5]);
                                    Boolean result2 = doc.PrintOut(copys, PrintOptionConstants.bpoHalfCut | PrintOptionConstants.bpoChainPrint);
                                    sw.WriteLine("正在打印,状态为:" + result2.ToString());

                                    sw.WriteLine("程序成功打印成功,正常退出！");
                                    doc.EndPrint();
                                }
                                doc.Close();
                            }

                        }

                        if (myList[0].Equals("GROUPLABEL"))
                        {
                            string filename;


                            filename = @"C:\brother\PhireGroupLabel.frx";


                            Report report1 = new Report();

                            report1.Load(filename);
                            (report1.FindObject("ProductType") as TextObject).Text = myList[3];
                            (report1.FindObject("Barcode1") as BarcodeObject).Text = myList[2];

                        //    (report1.FindObject("GroupLabel") as TextObject).Text = myList[0];

                            report1.PrintSettings.Clear();
                            report1.PrintSettings.ShowDialog = false;
                          //  report1.PrintSettings.Copies = int.Parse(iniList[1]);
                            report1.PrintSettings.Printer =GetPrinterName("GROUPLABEL");

                            //  report1.PrintSettings.Printer =""//
                            // ((String)report1.GetColumnValue(myList[2]));
                            //   report1.Show();
                            report1.Print();
                            sw.WriteLine("Print Success:  " + DateTime.Now.ToString());


                        }


                        sw.WriteLine("------------以上为一次打印记录-------------");
                        sw.Flush();
                        sw.Close();
                        myList.Clear();

      

                        Random random = new Random();
                        string  NewFile;

                      //  OrignFile = @"c:\brother";
                        NewFile = "c:\\brother\\backup\\Label" + DateTime.Now.ToString("yyyyMMddhhmmss-") + random.Next().ToString() + ".txt";
                        //    NewFile = "c:\\brother\\backup\\Label" + file.FullName + "-" +random.Next().ToString() + ".txt";
                        File.Copy(file.FullName, NewFile, true);

                        if (File.Exists(FileName[j]))
                        DeleteFile(FileName[j]);

                        


                    }

                }
            }
            catch (Exception ex)
            {
                //  ex.ToString()

                sw.WriteLine("------------Program error-------------");
                sw.WriteLine(ex.ToString());
                sw.WriteLine("------------Print error log-------------");

                sw.Flush();
                sw.Close();
            }



        }


    }

}
