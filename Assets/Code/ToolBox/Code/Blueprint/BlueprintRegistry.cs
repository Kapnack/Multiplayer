using ImageCampus.ToolBox.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.IO;

namespace ImageCampus.ToolBox.Blueprints
{
    public sealed class BlueprintRegistry : IService
    {
        public bool IsPersistance => true;


        private readonly Dictionary<string, BlueprintData> blueprintDatas;
        internal Dictionary<string, BlueprintData> BlueprintDatas => blueprintDatas;

        public BlueprintRegistry(string bluprintPath)
        {
            blueprintDatas = new Dictionary<string, BlueprintData>();
            using (FileStream file = new FileStream(bluprintPath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(file);

                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    blueprintDatas.Add(workbook.GetSheetName(i), new BlueprintData(workbook.GetSheet(workbook.GetSheetName(i))));
                }
            }
        }

        public List<string> BlueprintsOf(string blueprintTable) => blueprintDatas[blueprintTable].BluprintIDs;
        public List<string> ParametersOf(string blueprintTable) => blueprintDatas[blueprintTable].Parameters;
        public string this[string blueprintTable, string blueprintId, string parameter] => blueprintDatas[blueprintTable][blueprintId, parameter];
        public bool TryGetValue(string blueprintTable, string blueprintId, string parameter, out string value) => blueprintDatas[blueprintTable].TryGet(blueprintId, parameter, out value);
    }
}