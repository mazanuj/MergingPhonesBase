using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Caliburn.Micro;
using MergingPhonesBase.Config;
using MergingPhonesBase.XmlWorker;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace MergingPhonesBase.ViewModels
{
    [Export(typeof (MainViewModel))]
    public class MainViewModel : PropertyChangedBase
    {
        private string inputFile, baseFile;
        private string labelStatus = "NOTHING";
        private bool canButtonFile = true;
        private bool canButtonDB = true;

        public bool RecipientsFileLabel { get; set; }
        public bool BaseFileLabel { get; set; }
        public bool CanButtonStart { get; set; }

        private void GetButtonStartEnabledStatus()
        {
            CanButtonStart = !string.IsNullOrEmpty(inputFile) && !string.IsNullOrEmpty(baseFile);
            NotifyOfPropertyChange(() => CanButtonStart);
        }

        public bool CanButtonCanButtonDb
        {
            get { return canButtonDB; }
            set
            {
                canButtonDB = value;
                NotifyOfPropertyChange(() => CanButtonCanButtonDb);
            }
        }

        public bool CanButtonCanButtonFile
        {
            get { return canButtonFile; }
            set
            {
                canButtonFile = value;
                NotifyOfPropertyChange(() => CanButtonCanButtonFile);
            }
        }

        private void ChangeRecipientsFileLabelStatus(bool status)
        {
            RecipientsFileLabel = status;
            NotifyOfPropertyChange(() => RecipientsFileLabel);
        }

        private void ChangeBaseFileLabelStatus(bool status)
        {
            BaseFileLabel = status;
            NotifyOfPropertyChange(() => BaseFileLabel);
        }

        public string LabelStatus
        {
            get { return labelStatus; }
            set
            {
                labelStatus = value;
                NotifyOfPropertyChange(() => LabelStatus);
            }
        }

        public void ButtonFile()
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "File (*.xls or *.xlsx)|*.xls;*.xlsx",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dlg.ShowDialog() == false)
                return;

            inputFile = dlg.FileName;
            ChangeRecipientsFileLabelStatus(true);
            GetButtonStartEnabledStatus();
        }

        public void ButtonDB()
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "File *.xml|*.xml;",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dlg.ShowDialog() == false)
                return;

            baseFile = dlg.FileName;
            ChangeBaseFileLabelStatus(true);
            GetButtonStartEnabledStatus();
        }

        public void ButtonStart()
        {
            CanButtonStart = false;
            LabelStatus = "IN PROGRESS";
            var fileExt = Path.GetExtension(inputFile);
            ISheet sheet;
            try
            {
                if (fileExt != null && fileExt.ToLower() == ".xls")
                {

                    HSSFWorkbook hssfwb;
                    using (var file = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                        hssfwb = new HSSFWorkbook(file);
                    sheet = hssfwb.GetSheetAt(hssfwb.ActiveSheetIndex);
                }
                else
                {
                    XSSFWorkbook xssfwb;
                    using (var file = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                        xssfwb = new XSSFWorkbook(file);
                    sheet = xssfwb.GetSheetAt(xssfwb.ActiveSheetIndex);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format("Close {0} canButtonFile before reading", inputFile));
                GetButtonStartEnabledStatus();
                LabelStatus = "ERROR";
                return;
            }

            var cellsValues = new List<InfoHolder>();
            for (var row = 4; row <= sheet.LastRowNum; row++)
            {
                try
                {
                    var currRow = sheet.GetRow(row);
                    if (currRow == null || !Regex.IsMatch(currRow.GetCell(3).StringCellValue, @"\+[0-9]{9,}"))
                        continue;

                    cellsValues.Add(new InfoHolder
                    {
                        Site = SiteEnum.torgshop,
                        Direction = DirectionEnum.@base,
                        Name = currRow.GetCell(1).StringCellValue,
                        City = currRow.GetCell(2).StringCellValue,
                        Phone = Regex.Match(currRow.GetCell(3).StringCellValue, @"\d+").Value
                    });
                }
                catch
                {
                }
            }

            var existedTels = DataXmlWorker.GetTels(baseFile);

            DataXmlWorker.SetTels(cellsValues
                .Where(x => x != null &&
                            !string.IsNullOrEmpty(x.Phone) &&
                            Regex.IsMatch(x.Phone, @"^380\d{9}$"))
                .GroupBy(holder => holder.Phone)
                .Select(x => !existedTels.Contains(x.Key) ? x.First() : null)
                .Where(x => x != null), baseFile);

            GetButtonStartEnabledStatus();
            LabelStatus = "COMPLETE";
        }
    }
}