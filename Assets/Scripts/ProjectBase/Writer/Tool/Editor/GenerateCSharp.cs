using System;
using System.IO;
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
                string T = field.Attributes["T"].Value;
                writingStr += "\t\t\twriter.Write((short)value." + name + ".Count);\r\n";
                writingStr += "\t\t\tfor (int i = 0; i < value." + name + ".Count; ++i)\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStr(T, name + "[i]") + "\r\n";
            }
            else if (type == "array")
            {
                string data = field.Attributes["data"].Value;
                writingStr += "\t\t\twriter.Write((short)value." + name + ".Length);\r\n";
                writingStr += "\t\t\tfor (int i = 0; i < value." + name + ".Length; ++i)\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStr(data, name + "[i]") + "\r\n";
            }
            else if (type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                writingStr += "\t\t\twriter.Write((short)value." + name + ".Count);\r\n";
                writingStr += "\t\t\tforeach (" + Tkey + " key in value." + name + ".Keys)\r\n";
                writingStr += "\t\t\t{\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStrSingle(Tkey, "key") + "\r\n";
                writingStr += "\t\t\t\t" + GetFieldWritingStr(Tvalue, name + "[key]") + "\r\n";
                writingStr += "\t\t\t}\r\n";
            }
            else
            {
                writingStr += "\t\t\t" + GetFieldWritingStr(type, name) + "\r\n";
            }
        }
        return writingStr;
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

    private string GetFieldWritingStrSingle(string type, string name)
    {
        switch (type)
        {
            case "byte":
                return "writer.Write(" + name + ");";
            case "int":
                return "writer.Write(" + name + ");";
            case "short":
                return "writer.Write(" + name + ");";
            case "long":
                return "writer.Write(" + name + ");";
            case "float":
                return "writer.Write(" + name + ");";
            case "bool":
                return "writer.Write(" + name + ");";
            case "string":
                return "writer.Write(" + name + ");";
            case "enum":
                return "writer.Write((int)" + name + ");";
            default:
                return "writer.Write" + type + "(" + name + ");";
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
                readingStr += "\t\t\tvalue." + name + " = new List<" + T + ">();\r\n";
                readingStr += "\t\t\tshort " + name + "Count = reader.ReadShort();\r\n";
                readingStr += "\t\t\tfor (int i = 0; i < " + name + "Count; ++i)\r\n";
                readingStr += "\t\t\t\tvalue." + name + ".Add(" + GetFieldReadingStr(T) + ");\r\n";
            }
            else if (type == "array")
            {
                string data = field.Attributes["data"].Value;
                readingStr += "\t\t\tshort " + name + "Length = reader.ReadShort();\r\n";
                readingStr += "\t\t\tvalue." + name + " = new " + data + "[" + name + "Length];\r\n";
                readingStr += "\t\t\tfor (int i = 0; i < value." + name + ".Length; ++i)\r\n";
                readingStr += "\t\t\t\tvalue." + name + "[i] = " + GetFieldReadingStr(data) + ";\r\n";
            }
            else if (type == "dic")
            {
                string Tkey = field.Attributes["Tkey"].Value;
                string Tvalue = field.Attributes["Tvalue"].Value;
                readingStr += "\t\t\tvalue." + name + " = new Dictionary<" + Tkey + ", " + Tvalue + ">();\r\n";
                readingStr += "\t\t\tshort " + name + "Count = reader.ReadShort();\r\n";
                readingStr += "\t\t\tfor (int i = 0; i < " + name + "Count; ++i)\r\n";
                readingStr += "\t\t\t\tvalue." + name + ".Add(" + GetFieldReadingStr(Tkey) + ", " +
                                                            GetFieldReadingStr(Tvalue) + ");\r\n";
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
}
