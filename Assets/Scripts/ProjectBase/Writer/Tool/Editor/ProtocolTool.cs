using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class ProtocolTool
{
    //配置文件所在路径
    private static string XML_INFO_PATH = Application.dataPath + "/Scripts/ProjectBase/Writer/Tool/Editor/ProtocolInfo.xml";
    private static string SAVE_PATH = Application.dataPath + "/Scripts/ProjectBase/Writer/ReadWrite/";

    private static GenerateCSharp generateCSharp = new GenerateCSharp();
    [MenuItem("Tools/GenerateDataReadWrite")]
    private static void GenerateCSharp()
    {
        //生成对应的数据类读写脚本
        generateCSharp.GenerateData(GetNodes("data"), SAVE_PATH);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取指定名字的所有子节点 的 List
    /// </summary>
    /// <param name="nodeName"></param>
    /// <returns></returns>
    private static XmlNodeList GetNodes(string nodeName)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(XML_INFO_PATH);
        XmlNode root = xml.SelectSingleNode("messages");
        return root.SelectNodes(nodeName);
    }
}
