using BinaryReadWrite;
using Excel;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public enum AutoType
{
    sigle,all
}

public enum GenType
{
    sigle,allFile
}

public class AutoExcelTool
{
    public static string SavePath = Application.streamingAssetsPath;
    public static string LoadPath = Application.dataPath + "/Editor/Data/";
    public static string scriptPath = Application.dataPath + "/Scripts/ProjectBase/Writer/ReadWrite/ExcelTool/";

    public static Action<string[]> OnReadRow;
    public static Action<DataTable> OnReadTable;
    public static Action<FileInfo> OnReadFile;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="savePath">二进制数据存储位置</param>
    /// <param name="loadPath">excel文件位置</param>
    /// <param name="type">找寻文件模式</param>
    /// <param name="gen">excel表读取模式</param>
    /// <param name="onReadRow">当读取完一行后触发</param>
    /// <param name="onReadTable">当读取完一表后触发</param>
    /// <param name="onReadFile">当读取完一文件后触发</param>
    /// <param name="ifGenScript">是否自动化生成脚本</param>
    public static void Triger(string savePath, string loadPath, AutoType type = AutoType.sigle, GenType gen = GenType.sigle, Action<string[]> onReadRow = null, Action<DataTable> onReadTable = null, Action<FileInfo> onReadFile = null, bool ifGenScript = true)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        if (loadPath == null) loadPath = LoadPath;
        if (savePath == null) savePath = "Data";
        OnReadRow = onReadRow;
        OnReadTable = onReadTable;
        OnReadFile = onReadFile;
        if (type == AutoType.all)
        {
            DirectoryInfo dinfo = Directory.CreateDirectory(loadPath);
            FileInfo[] fileInfos = dinfo.GetFiles();
            foreach (var fileinfo in fileInfos)
            {
                Gen(fileinfo, savePath, gen, ifGenScript);
                OnReadFile?.Invoke(fileinfo);
            }
        }
        else
        {
            FileInfo fileInfo = new FileInfo(loadPath);
            Gen(fileInfo, savePath, gen, ifGenScript);
            OnReadFile?.Invoke(fileInfo);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if(ifGenScript)
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Thread execution time: " + stopwatch.ElapsedMilliseconds + " ms");
    }

    public static DataTableCollection TrigerOne(string loadPath)
    {
        if (loadPath == null) loadPath = LoadPath;
        FileInfo fileInfo = new FileInfo(loadPath);
        DataTableCollection result;
        if (fileInfo.Extension != ".xlsx" && fileInfo.Extension != ".xls") return null;
        using (FileStream fs = fileInfo.OpenRead())
        {
            IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
            result = excelDataReader.AsDataSet().Tables;
            fs.Close();
        }
        return result;
    }

    private static void Gen(FileInfo fileinfo, string savePath, GenType gen, bool ifGenScript)
    {
        DataTableCollection result;
        if (fileinfo.Extension != ".xlsx" && fileinfo.Extension != ".xls") return;
        using (FileStream fs = fileinfo.OpenRead())
        {
            IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
            result = excelDataReader.AsDataSet().Tables;
            fs.Close();
        }

        Writer writer = new Writer();

        if(gen == GenType.sigle)
        {
            foreach (DataTable dt in result)
            {
                if (dt.Rows[0][0] != null && dt.Rows[0][0].ToString() == "#") continue;
                string name = dt.TableName;
                string path = savePath + "/" + name + ".data";
                int count = 0;
                writer.Reset();
                writer.Write(count);

                if (ifGenScript)
                {
                    GenScript(dt, name,path);
                }
                WriteData(dt, writer, ref count, fileinfo);

                int postion = writer.Position;
                writer.Position = 0;
                writer.Write(count);
                writer.Position = postion;

                if (!Directory.Exists(SavePath + "/" + savePath))
                    Directory.CreateDirectory(SavePath + "/" + savePath);
                File.WriteAllBytes(SavePath + "/" + savePath + "/" + name + ".data", writer.ToArray());
            }
        }
        else if(gen == GenType.allFile)
        {
            string name = Path.GetFileNameWithoutExtension(fileinfo.Name);
            string path = savePath + "/" + name + ".data";
            savePath = SavePath + "/" + savePath;
            int count = 0;
            writer.Reset();
            writer.Write(count);
            foreach (DataTable dt in result)
            {
                if (dt.Rows[0][0] != null && dt.Rows[0][0].ToString() == "#") continue;
                if (ifGenScript)
                {
                    GenScript(dt,name, path);
                    ifGenScript = false;
                }
                WriteData(dt, writer, ref count, fileinfo);
            }
            int postion = writer.Position;
            writer.Position = 0;
            writer.Write(count);
            writer.Position = postion;

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            File.WriteAllBytes(savePath + "/" + name + ".data", writer.ToArray());
        }
    }


    private static void GenScript(DataTable dt, string name,string Path)
    {
        var sb = new StringBuilder();
        var sbWrite = new StringBuilder();
        var sbRead = new StringBuilder();
        //类名
        string classNameStr = name;
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using BinaryReadWrite;");
        sb.AppendLine("namespace ExcelTool\n{");
        sb.AppendLine($"\tpublic class {classNameStr} : AutoData<{classNameStr}>, ICloneable");
        sb.AppendLine("\t{");
        for (int i = 1; i < dt.Columns.Count; i++)
        {
            if (dt.Rows[1][i] == null || dt.Rows[1][i].ToString() == "") continue;
            sb.AppendLine($"\t\tpublic {dt.Rows[2][i]} {dt.Rows[1][i]};");
            sbWrite.AppendLine($"\t\t\twriter.Write(value.{dt.Rows[1][i].ToString()});");
            sbRead.AppendLine(ReadStr(dt.Rows[2][i].ToString(), dt.Rows[1][i].ToString()));
        }
        sb.AppendLine("\t\tpublic static string Path = \"" + Path + "\";");
        sb.AppendLine("\t\tpublic object Clone()");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\treturn Utilities.Clone(this);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t\tpublic T Clone<T>()");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\treturn (T)Utilities.Clone(this);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        sb.AppendLine($"\tpublic static class {classNameStr}ReadWrite");
        sb.AppendLine("\t{");
        sb.AppendLine($"\t\tpublic static void Write{classNameStr}(this Writer writer, {classNameStr} value)");
        sb.AppendLine("\t\t{");
        sb.Append(sbWrite);
        sb.AppendLine("\t\t}");
        sb.AppendLine($"\t\tpublic static {classNameStr} Read{classNameStr}(this Reader reader)");
        sb.AppendLine("\t\t{");
        sb.AppendLine($"\t\t\t{classNameStr} value = new {classNameStr}();");
        sb.Append(sbRead);
        sb.AppendLine("\t\t\treturn value;");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        if (!Directory.Exists(scriptPath))
            Directory.CreateDirectory(scriptPath);
        File.WriteAllText(scriptPath + name + ".cs", sb.ToString());
        Console.WriteLine(classNameStr + " EXCELData已生成");
    }

    private static void WriteData(DataTable dt,Writer writer,ref int count,FileInfo fileInfo)
    {
        DataRow row;
        string[] names = new string[dt.Columns.Count - 1];
        string[] types = new string[dt.Columns.Count - 1];
        for(int j = 1; j < dt.Columns.Count; j++)
        {
            if (dt.Rows[1][j] == null || dt.Rows[1][j].ToString() == "") continue;
            types[j - 1] = dt.Rows[2][j].ToString().Trim();
        }
        for (int i = 3; i < dt.Rows.Count; i++)
        {
            row = dt.Rows[i];
            if (row[0].ToString() == "#") continue;
            if (row[1] == null || row[1].ToString() == "") continue;

            for (int j = 1; j < dt.Columns.Count; j++)
            {
                if (dt.Rows[1][j] == null || dt.Rows[1][j].ToString() == "") continue;
                try
                {
                    names[j - 1] = row[j].ToString().Trim();
                    Write(writer, types[j - 1], names[j - 1]);
                }
                catch
                {
                    UnityEngine.Debug.Log($"{fileInfo.Name}中的{dt.TableName}表{i + 1}行{j + 1},“{names[j - 1]}”列序列化出错");
                    return;
                }
            }
            count++;
            OnReadRow?.Invoke(names);
        }
        OnReadTable?.Invoke(dt);
    }

    private static void Write(Writer writer,string type ,string obj)
    {
        switch (type)
        {
            case "float":
                writer.Write(float.Parse(obj));
                break;
            case "double":
                writer.Write(double.Parse(obj));
                break;
            case "int":
                writer.Write(int.Parse(obj));
                break;
            case "long":
                writer.Write(long.Parse(obj));
                break;
            case "string":
                writer.WriteString(obj);
                break;
            case "bool":
                writer.Write(bool.Parse(obj));
                break;
            case "uint":
                writer.Write(uint.Parse(obj));
                break;
            case "ushort":
                writer.Write(ushort.Parse(obj));
                break;
            case "ulong":
                writer.Write(ulong.Parse(obj));
                break;
            default:
                for (int i = 0; i < type.Length; i++)
                {
                    if (type[i] == '<')
                    {
                        string[] str = { type[..i], type[(i + 1)..(type.Length - 1)] };
                        switch (str[0])
                        {
                            case "Dictionary":
                                if (obj == null || obj == "") 
                                {
                                    writer.Write(0);
                                    return;
                                }
                                str = str[1].Split(',');
                                obj = obj.Trim();
                                string[] value = obj.Split();
                                if (value == null)
                                {
                                    writer.Write(0);
                                    return;
                                }
                                writer.Write(value.Length);
                                foreach (string str2 in value)
                                {
                                    string[] kvp = str2.Split('=');
                                    Write(writer, str[0], kvp[0]);
                                    Write(writer, str[1], kvp[1]);
                                }
                                return;
                            case "List":
                                if (obj == null || obj == "")
                                {
                                    writer.Write(0);
                                    return;
                                }
                                obj = obj.Trim();
                                value = obj.Split();
                                if (value == null)
                                {
                                    writer.Write(0);
                                    return;
                                }
                                writer.Write(value.Length);
                                foreach (string str3 in value)
                                {
                                    Write(writer, str[1], str3);
                                }
                                return;
                        }
                        return;
                    }
                    else if (type[i] == '[')
                    {
                        if (obj == null || obj == "")
                        {
                            writer.Write(0);
                            return;
                        }
                        string str = type.Substring(0, i);
                        obj = obj.Trim();
                        string[] value = obj.Split();
                        if (value == null)
                        {
                            writer.Write(0);
                            return;
                        }
                        writer.Write(value.Length);
                        foreach (string str3 in value)
                        {
                            Write(writer, str, str3);
                        }
                        return;
                    }
                }
                break;
        }
    }

    private static string ReadStr(string type,string name)
    {
        switch (type)
        {
            case "float":
                return $"\t\t\tvalue.{name} = reader.ReadFloat();";
            case "double":
                return $"\t\t\tvalue.{name} = reader.ReadDouble();";
            case "int":
                return $"\t\t\tvalue.{name} = reader.ReadInt();";
            case "long":
                return $"\t\t\tvalue.{name} = reader.ReadLong();";
            case "string":
                return $"\t\t\tvalue.{name} = reader.ReadString();";
            case "bool":
                return $"\t\t\tvalue.{name} = reader.ReadBool();";
            case "uint":
                return $"\t\t\tvalue.{name} = reader.ReadUInt();";
            case "ushort":
                return $"\t\t\tvalue.{name} = reader.ReadUShort();";
            case "ulong":
                return $"\t\t\tvalue.{name} = reader.ReadULong();";
            default:
                for (int i = 0; i < type.Length; i++)
                {
                    if (type[i] == '<')
                    {
                        string[] str = { type[..i], type[(i + 1)..(type.Length - 1)] };
                        switch (str[0])
                        {
                            case "Dictionary":
                                str = str[1].Split(',');
                                return $"\t\t\tvalue.{name} = reader.ReadDictionary<{str[0]},{str[1]}>();";
                            case "List":
                                
                                return $"\t\t\tvalue.{name} = reader.ReadList<{str[1]}>();";
                        }
                        return "";
                    }
                    else if (type[i] == '[')
                    {
                        string str = type.Substring(0, i);
                        return $"\t\t\tvalue.{name} = reader.ReadArray<{str}>();";
                    }
                }
                break;
        }
        return "";
    }
}
