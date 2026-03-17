using NPOI.SS.UserModel;
using System.Collections.Generic;

namespace ImageCampus.ToolBox.Blueprints
{
    internal sealed class BlueprintData
    {
        private const int OFFSET = 1;
        internal string this[string blueprintID, string parameter]
        {
            get
            {
                int indexOfBlueprintID = bluprintIDs.IndexOf(blueprintID);
                int indexOfParameter = parameters.IndexOf(parameter);

                if (indexOfBlueprintID == -1)
                    throw new System.MissingFieldException($"Missing bluprint id: {blueprintID}");

                if (indexOfParameter == -1)
                    throw new System.MissingFieldException($"Missing parameter: {parameter}");

                return rawContent[indexOfBlueprintID + OFFSET, indexOfParameter + OFFSET];
            }
        }


        private readonly string[,] rawContent;
        private readonly List<string> bluprintIDs;
        private readonly List<string> parameters;
        internal List<string> BluprintIDs => bluprintIDs;
        internal List<string> Parameters => parameters;

        public BlueprintData(ISheet sheet)
        {
            int maxRow = 0;
            int maxColumn = 0;

            for (int row = sheet.FirstRowNum; row <= sheet.LastRowNum; row++)
            {
                IRow sheetRow = sheet.GetRow(row);
                if (sheetRow == null)
                    continue;

                for (int column = sheetRow.FirstCellNum; column < sheetRow.LastCellNum; column++)
                {
                    ICell cell = sheetRow.GetCell(column);

                    if (cell == null)
                        continue;

                    if (cell.CellType == CellType.Blank)
                        continue;

                    if (row + OFFSET > maxRow)
                        maxRow = row + OFFSET;

                    if (column + OFFSET > maxColumn)
                        maxColumn = column + OFFSET;
                }
            }

            rawContent = new string[maxRow, maxColumn];

            for (int row = sheet.FirstRowNum; row <= sheet.LastRowNum; row++)
            {
                IRow sheetRow = sheet.GetRow(row);
                if (sheetRow == null)
                    continue;

                for (int column = sheetRow.FirstCellNum; column < sheetRow.LastCellNum; column++)
                {
                    ICell cell = sheetRow.GetCell(column);

                    if (cell == null)
                        continue;

                    if (cell.CellType == CellType.Blank)
                        continue;

                    rawContent[row, column] = cell.ToString();
                }
            }

            bluprintIDs = new List<string>();
            for (int i = OFFSET; i < rawContent.GetLength(0); i++)
            {
                bluprintIDs.Add(rawContent[i, 0]);
            }

            parameters = new List<string>();
            for (int i = OFFSET; i < rawContent.GetLength(1); i++)
            {
                parameters.Add(rawContent[0, i]);
            }
        }

        internal bool TryGet(string blueprintID, string parameter, out string value) 
        {
			try
			{
                value = this[blueprintID, parameter];
			}
			catch (System.Exception)
			{
                value = string.Empty;
                return false;
			}
            return true;
        }
    }
}