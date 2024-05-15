using BinaryReadWrite;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class ProtocolTool
{
    //�����ļ�����·��
    private static string XML_INFO_PATH = Application.dataPath + "/Scripts/ProjectBase/Writer/Tool/Editor/ProtocolInfo.xml";
    private static string SAVE_PATH = Application.dataPath + "/Scripts/ProjectBase/Writer/ReadWrite/";

    private static GenerateCSharp generateCSharp = new GenerateCSharp();
    [MenuItem("Tools/DataReadWrite/ͨ��xml����")]
    private static void GenerateCSharp()
    {
        //���ɶ�Ӧ���������д�ű�
        generateCSharp.GenerateData(GetNodes("data"), SAVE_PATH);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// ��ȡָ�����ֵ������ӽڵ� �� List
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
    [MenuItem("Tools/DataReadWrite/ͨ����������")]
    private static void AutoGenerateCSharp()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach(var value in WriterManage.assemblyList)
        {
            generateCSharp.GenerateData(value.assembly, value.savePath);
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("Thread execution time: " + stopwatch.ElapsedMilliseconds + " ms");
        AssetDatabase.Refresh();
    }
}
