using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

public class GenerateCSharp
{
    //生成数据结构类
    public void GenerateData(XmlNodeList nodes,string SAVE_PATH)
    {
        string namespaceStr;
        string classNameStr;
        string fieldStr;
        string writingStr;
        string readingStr;

        foreach (XmlNode dataNode in nodes)
        {
            //命名空间
            namespaceStr = dataNode.Attributes["namespace"].Value;
            //类名
            classNameStr = dataNode.Attributes["name"].Value;
            //读取所有字段节点
            XmlNodeList fields = dataNode.SelectNodes("field");
            //通过这个方法进行成员变量声明的拼接 返回拼接结果
            fieldStr = GetFieldStr(fields);
            //通过某个方法 对Writing函数中的字符串内容进行拼接 返回结果
            writingStr = GetWritingStr(fields);
            //通过某个方法 对Reading函数中的字符串内容进行拼接 返回结果
            readingStr = GetReadingStr(fields);

            string dataStr = "using System;\r\n" +
                             "using System.Collections.Generic;\r\n" +
                             "using System.Text;\r\n" +
                             "using BinaryReadWrite;\r\n" +
                             $"namespace {namespaceStr}\r\n" +
                              "{\r\n";
            if(dataNode.Attributes["type"].Value == "Inside")
            {
                dataStr += $"\tpublic class {classNameStr}\r\n" +
                            "\t{\r\n" +
                                  $"{fieldStr}" +
                            "\t}\r\n" +
                            "\r\n";
            }

            dataStr +=        $"\tpublic static class {classNameStr}ReadWrite\r\n" +
                              "\t{\r\n" +
                                    $"\t\tpublic static void Write{classNameStr}(this Writer writer, {classNameStr} value)\r\n" +
                                    "\t\t{\r\n" +
                                        $"{writingStr}" +
                                    "\t\t}\r\n" +
                                    $"\t\tpublic static {classNameStr} Read{classNameStr}(this Reader reader)\r\n" +
                                    "\t\t{\r\n" +
                                        $"\t\t\t{classNameStr} value = new {classNameStr}();\r\n" +
                                        $"{readingStr}" +
                                        "\t\t\treturn value;\r\n" +
                                    "\t\t}\r\n" +
                              "\t}\r\n" +
                              "}";

            //保存为 脚本文件
            //保存文件的路径
            string path = SAVE_PATH + namespaceStr + "/";
            //如果不存在这个文件夹 则创建
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //字符串保存 存储为枚举脚本文件
            File.WriteAllText(path + classNameStr + "ReadWrite.cs", dataStr);
            Console.WriteLine(classNameStr + "ReadWrite已生成");
        }
    }

    public void GenerateData(Assembly assembly, string SAVE_PATH)
    {
        string namespaceStr;
        string classNameStr;

        Dictionary<Type, List<Type>> typeDic = new Dictionary<Type, List<Type>>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<AutoWriteAttribute>() == null) continue;

            typeDic.Add(type, new List<Type>());
            if (typeDic.TryGetValue(type.BaseType, out var list))
            {
                list.Add(type);
            }
        }
        foreach(var kvp in typeDic)
        {
            Type type = kvp.Key;
            //命名空间
            namespaceStr = type.Namespace;
            if (namespaceStr == null || namespaceStr == "") namespaceStr = "BinaryReadWrite";
            //类名
            classNameStr = type.Name;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using BinaryReadWrite;");
            sb.AppendLine($"namespace {namespaceStr}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static class {classNameStr}ReadWrite");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\tpublic static void Write{classNameStr}(this Writer writer, {classNameStr} value)");
            sb.AppendLine("\t\t{");
            if (kvp.Value.Count != 0)
            {
                foreach(var t in kvp.Value)
                {
                    sb.AppendLine($"\t\t\tif(value is {t.Name} child{t.Name})");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine($"\t\t\t\twriter.Write({t.GetHashCode()});");
                    sb.AppendLine($"\t\t\t\twriter.Write(child{t.Name});");
                    sb.AppendLine("\t\t\t\treturn;");
                    sb.AppendLine("\t\t\t}");
                }
                sb.AppendLine($"\t\t\twriter.Write(0);");
            }
            sb.AppendLine(GetWritingStr(type, "\t\t\t"));
            sb.AppendLine("\t\t}");
            sb.AppendLine($"\t\tpublic static {classNameStr} Read{classNameStr}(this Reader reader)");
            sb.AppendLine("\t\t{");
            if (kvp.Value.Count != 0)
            {
                sb.AppendLine("\t\t\tswitch (reader.ReadInt())");
                sb.AppendLine("\t\t\t{");
                foreach (var t in kvp.Value)
                {
                    sb.AppendLine($"\t\t\t\tcase {t.GetHashCode()}:");
                    sb.AppendLine($"\t\t\t\t\treturn reader.Read{t.Name}();");

                }
                sb.AppendLine("\t\t\t}");
            }
            sb.AppendLine($"\t\t\t{classNameStr} value = new {classNameStr}();");
            sb.AppendLine(GetReadingStr(type, "\t\t\t"));
            sb.AppendLine("\t\t\treturn value;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            //保存为 脚本文件
            //保存文件的路径
            string path = SAVE_PATH + namespaceStr + "/";
            //如果不存在这个文件夹 则创建
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //字符串保存 存储为枚举脚本文件
            File.WriteAllText(path + classNameStr + "ReadWrite.cs", sb.ToString());
            Console.WriteLine(classNameStr + "ReadWrite已生成");
        }
    }

    /// <summary>
    /// 获取成员变量声明内容
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    private string GetFieldStr(XmlNodeList fields)
    {
        string fieldStr = "";
        foreach (XmlNode field in fields)
        {
            //变量类型
            string type = field.Attributes["type"].Value;
            //变量名
            string fieldName = field.Attributes["name"].Value;
            if(type == "list")
            {
                string T = field.Attributes["T"].Value;
                fieldStr += "\t\tpublic List<" + T + "> ";
            }
            else if(type == "array")
            {
                string data = field.Attributes["data"].Value;
                fieldStr += "\t\tpublic " + data + "[] ";
            }
            else if(type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                fieldStr += "\t\tpublic Dictionary<" + Tkey +  ", " + Tvalue + "> ";
            }
            else if(type == "enum")
            {
                string data = field.Attributes["data"].Value;
                fieldStr += "\t\tpublic " + data + " ";
            }
            else
            {
                fieldStr += "\t\tpublic " + type + " ";
            }

            fieldStr += fieldName + ";\r\n";
        }
        return fieldStr;
    }


    //拼接 Writing函数的方法
    private string GetWritingStr(XmlNodeList fields)
    {
        string writingStr = "";

        string type = "";
        string name = "";
        foreach (XmlNode field in fields)
        {
            type = field.Attributes["type"].Value;
            name = field.Attributes["name"].Value;
            if (type == "list")
            {
                writingStr += "\t\t\twriter.WriteList(value." + name + ");\r\n";
            }
            else if (type == "array")
            {
                writingStr += "\t\t\twriter.WriteArray(value." + name + ");\r\n";
            }
            else if (type == "dic")
            {
                writingStr += "\t\t\twriter.WriteDictionary(value." + name + ");\r\n";
            }
            else
            {
                writingStr += "\t\t\t" + GetFieldWritingStr(type, name) + "\r\n";
            }
        }
        return writingStr;
    }

    private string GetWritingStr(Type type,string tap)
    {
        StringBuilder writingStr = new StringBuilder();
        foreach (var field in type.GetFields())
        {
            if (field.GetCustomAttribute<NotWriteAttribute>() != null) continue;
            GetTypeWritingStr(field.FieldType, "value." + field.Name, tap, writingStr);
        }
        return writingStr.ToString();
    }

    private void GetTypeWritingStr(Type type,string name,string tap,StringBuilder writingStr)
    {
        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elType = type.GetGenericArguments()[0];
                if (elType.IsGenericType || elType.IsArray)
                {
                    writingStr.AppendLine($"{tap}if({name} == null)writer.Write(-1);");
                    writingStr.AppendLine(tap + "else {");
                    writingStr.AppendLine($"{tap}\twriter.Write({name}.Count);");
                    writingStr.AppendLine($"{tap}tforeach(var item in {name})");
                    writingStr.AppendLine(tap + "\t{");
                    GetTypeWritingStr(elType, "item",tap + "\t\t", writingStr);
                    writingStr.AppendLine(tap + "\t}");
                    writingStr.AppendLine(tap + "}");
                }
                else
                {
                    writingStr.AppendLine(tap + $"writer.WriteList({name});");
                }
            }
            else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[] elType = type.GetGenericArguments();
                if (elType[0].IsGenericType || elType[0].IsArray || elType[1].IsGenericType || elType[1].IsArray)
                {
                    writingStr.AppendLine(tap + $"if({name} == null)writer.Write(-1);");
                    writingStr.AppendLine(tap + "else {");
                    writingStr.AppendLine(tap + $"\twriter.Write({name}.Count);");
                    writingStr.AppendLine(tap + $"\tforeach(var kvp in {name})");
                    writingStr.AppendLine(tap + "\t{");
                    GetTypeWritingStr(elType[0], "kvp.Key", tap + "\t\t", writingStr);
                    GetTypeWritingStr(elType[1], "kvp.Value", tap + "\t\t", writingStr);
                    writingStr.AppendLine(tap + "\t}");
                    writingStr.AppendLine(tap + "}");
                }
                else
                {
                    writingStr.AppendLine(tap + $"writer.WriteDictionary({name});");
                }
            }
            else
            {
                writingStr.AppendLine(tap + $"writer.Write({name});");
            }
        }
        else if (type.IsArray)
        {
            Type elType = type.GetElementType();
            if (elType.IsGenericType || elType.IsArray)
            {
                writingStr.AppendLine(tap + $"if({name} == null)writer.Write(-1);");
                writingStr.AppendLine(tap + "else {");
                writingStr.AppendLine(tap + $"\twriter.Write({name}.Length);");
                writingStr.AppendLine(tap + $"\tforeach(var item in {name})");
                writingStr.AppendLine(tap + "\t{");
                GetTypeWritingStr(elType, "item", tap + "\t\t", writingStr);
                writingStr.AppendLine(tap + "\t}");
                writingStr.AppendLine(tap + "}");
            }
            else
            {
                writingStr.AppendLine(tap + $"writer.WriteArray({name});");
            }
        }
        else if (type.IsEnum)
        {
            writingStr.AppendLine(tap + $"writer.Write((int){name});");
        }
        else
        {
            writingStr.AppendLine(tap + $"writer.Write({name});");
        }
    }

    private string GetFieldWritingStr(string type, string name)
    {
        switch (type)
        {
            case "byte":
                return "writer.Write(value." + name +");";
            case "int":
                return "writer.Write(value." + name + ");";
            case "short":
                return "writer.Write(value." + name + ");";
            case "long":
                return "writer.Write(value." + name + ");";
            case "float":
                return "writer.Write(value." + name + ");";
            case "bool":
                return "writer.Write(value." + name + ");";
            case "string":
                return "writer.Write(value." + name + ");";
            case "enum":
                return "writer.Write((int)value." + name + ");";
            default:
                return "writer.Write" + type + "(value." + name + ");";
        }
    }

    private string GetReadingStr(XmlNodeList fields)
    {
        string readingStr = "";

        string type = "";
        string name = "";
        foreach (XmlNode field in fields)
        {
            type = field.Attributes["type"].Value;
            name = field.Attributes["name"].Value;
            if (type == "list")
            {
                string T = field.Attributes["T"].Value;
                readingStr += $"\t\t\tvalue.{name} = reader.ReadList<{T}>();\r\n";
            }
            else if (type == "array")
            {
                string data = field.Attributes["data"].Value;
                readingStr += $"\t\t\tvalue.{name} = reader.ReadArrary<{data}>();\r\n";
            }
            else if (type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                readingStr += $"\t\t\tvalue.{name} = reader.ReadDictionary<{Tkey},{Tvalue}>();\r\n";
            }
            else if (type == "enum")
            {
                string data = field.Attributes["data"].Value;
                readingStr += "\t\t\tvalue." + name + " = (" + data + ")reader.ReadInt();\r\n";
            }
            else
                readingStr += "\t\t\tvalue." + name + " = " + GetFieldReadingStr(type) + ";\r\n";
        }

        return readingStr;
    }

    private string GetReadingStr(Type type,string tap)
    {
        StringBuilder readingStr = new StringBuilder();
        foreach (var field in type.GetFields())
        {
            if (field.GetCustomAttribute<NotWriteAttribute>() != null) continue;
            GetTypeReadingStr(field.FieldType, "value." + field.Name, tap, readingStr);
        }
        return readingStr.ToString();
    }

    private void GetTypeReadingStr(Type type,string name,string tap,StringBuilder readingStr)
    {
        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elType = type.GetGenericArguments()[0];
                if (elType.IsGenericType || elType.IsArray)
                {
                    readingStr.AppendLine(tap + "int count = reader.ReadInt();");
                    readingStr.AppendLine(tap + $"if(count == -1) {name} = null;");
                    readingStr.AppendLine(tap + "else {");
                    readingStr.AppendLine(tap + $"\t{name} = new List<{GetTypeName(elType)}>();");
                    readingStr.AppendLine(tap + $"\tfor(int i = 0; i < count; i++)");
                    readingStr.AppendLine(tap + "\t{");
                    GetTypeReadingStr(elType, "name[i]",tap + "\t\t", readingStr);
                    readingStr.AppendLine(tap + "\t}");
                    readingStr.AppendLine(tap + "}");
                }
                else
                {
                    readingStr.AppendLine(tap + $"{name} = reader.ReadList<{GetTypeName(elType)}>();");
                }
            }
            else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type[] elType = type.GetGenericArguments();
                if (elType[0].IsGenericType || elType[0].IsArray || elType[1].IsGenericType || elType[1].IsArray)
                {
                    readingStr.AppendLine(tap + "int count = reader.ReadInt();");
                    readingStr.AppendLine(tap + $"if(count == -1) {name} = null;");
                    readingStr.AppendLine(tap + "else {");
                    readingStr.AppendLine(tap + $"\t{name} = new Dictionary<{GetTypeName(elType[0])},{GetTypeName(elType[1])}>();");
                    readingStr.AppendLine(tap + $"\tfor(int i = 0; i < count; i++)");
                    readingStr.AppendLine(tap + "\t{");
                    readingStr.AppendLine(tap + $"\t\t{GetTypeName(elType[0])} key;");
                    readingStr.AppendLine(tap + $"\t\t{GetTypeName(elType[1])} val;");
                    GetTypeReadingStr(elType[0], "key", tap + "\t\t", readingStr);
                    GetTypeReadingStr(elType[1], "val", tap + "\t\t", readingStr);
                    readingStr.AppendLine(tap + $"\t\t{name}[key] = val;");
                    readingStr.AppendLine(tap + "\t}");
                    readingStr.AppendLine(tap + "}");
                }
                else
                {
                    readingStr.AppendLine(tap + $"{name} = reader.ReadDictionary<{GetTypeName(elType[0])},{GetTypeName(elType[1])}>();");
                }
            }
            else
            {
                readingStr.AppendLine(tap + $"{name} = {GetFieldReadingStr(type)};");
            }
        }
        else if (type.IsArray)
        {
            Type elType = type.GetElementType();
            if (elType.IsGenericType || elType.IsArray)
            {
                readingStr.AppendLine(tap + "int count = reader.ReadInt();");
                readingStr.AppendLine(tap + $"if(count == -1) {name} = null;");
                readingStr.AppendLine(tap + "else {");
                readingStr.AppendLine(tap + $"\t{name} = new {GetTypeName(elType)}[count];");
                readingStr.AppendLine(tap + $"\tfor(int i = 0; i < count; i++)");
                readingStr.AppendLine(tap + "\t{");
                GetTypeReadingStr(elType, "name[i]", tap + "\t\t", readingStr);
                readingStr.AppendLine(tap + "}");
                readingStr.AppendLine(tap + "}");
            }
            else
            {
                readingStr.AppendLine(tap + $"{name} = reader.ReadArray<{GetTypeName(type.GetElementType())}>();");
            }
        }
        else if (type.IsEnum)
        {
            readingStr.AppendLine(tap + $"{name}= ({ type.Name })reader.ReadInt();");
        }
        else
        {
            readingStr.AppendLine(tap + $"{name} = {GetFieldReadingStr(type)};");
        }
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string typeName = type.GetGenericTypeDefinition().Name;
            for(int i = 0; i < typeName.Length; i++)
            {
                if (typeName[i] == '`')
                {
                    typeName = typeName.Substring(0, i);
                }
            }
            Type[] el = type.GetGenericArguments();
            string[] elName = new string[el.Length];
            for(int i = 0; i < el.Length; i++)
            {
                elName[i] = GetTypeName(el[i]);
            }
            typeName += "<" + string.Join(",", elName) + ">";
            return typeName;
        }
        return type.ToString();
    }

    private string GetFieldReadingStr(string type)
    {
        switch (type)
        {
            case "byte":
                return "reader.ReadByte()";
            case "int":
                return "reader.ReadInt()";
            case "short":
                return "reader.ReadShort()";
            case "long":
                return "reader.ReadLong()";
            case "float":
                return "reader.ReadFloat()";
            case "bool":
                return "reader.ReadBool()";
            case "string":
                return "reader.ReadString()";
            default:
                return "reader.Read" + type + "()";
        }
    }

    private string GetFieldReadingStr(Type type)
    {
        if (type == typeof(byte))
            return "reader.ReadByte()";
        if (type == typeof(int))
            return "reader.ReadInt()";
        if (type == typeof(short))
            return "reader.ReadShort()";
        if (type == typeof(long))
            return "reader.ReadLong()";
        if (type == typeof(float))
            return "reader.ReadFloat()";
        if (type == typeof(bool))
            return "reader.ReadBool()";
        if (type == typeof(string))
            return "reader.ReadString()";
        return "reader.Read" + type + "()";
        
    }
}
