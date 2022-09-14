using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

using System.Threading;
using UnityEngine.UI;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


public class SetInfo : MonoBehaviour
{
    void Start() {}

    void Update() {}

    private string staInfo = "NULL";             //状态信息
    private string recMes = "NULL";              //接收到的消息
    private Socket socketSend;                   //客户端套接字，用来链接远端服务器
    private bool clickSend = false;
    
    public class StateJson {
        public State State {get; set;}
    }
    public class State {
        public string Kind {get; set;}
    }

    private void ClickConnect()
    {
        try {
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("175.178.12.81");
            IPEndPoint point = new IPEndPoint(ip, 3563);
            socketSend.Connect(point);

            Debug.Log("连接成功 , " + " ip = " + ip + " port = 3563");
            staInfo = ip + ":3563" + "  连接成功";

            Thread r_thread = new Thread(Received);             //开启新的线程，不停的接收服务器发来的消息
            r_thread.IsBackground = true;
            r_thread.Start();

            Thread s_thread = new Thread(SendMessage);          //开启新的线程，不停的给服务器发送消息
            s_thread.IsBackground = true;
            s_thread.Start();
        } catch (Exception) {
            Debug.Log("try error");
        }
    }

    void Received()
    {
        while (true)
        {
            try
            {
                byte[] buffer = new byte[1024 * 6];
                //实际接收到的有效字节数
                int len = socketSend.Receive(buffer);
                if (len == 0)
                {
                    break;
                }
                recMes = Encoding.UTF8.GetString(buffer, 2, len);
                var data = JsonConvert.DeserializeObject<StateJson>(recMes);
                Debug.Log(data.State.Kind);
            }
            catch (Exception ex) {
                Debug.Log(ex);
            }
        }
    }

    void SendMessage()
    {
        try
        {
            while (true)
            {
                if (clickSend)                             
                {
                    clickSend = false;
                    string msg = "{\"SetInfo\": {\"Name\": \"test\", \"ChapterID\": 1, \"LevelIndex\": 2, \"Pigment\": 22}}";

                    byte[] msgbuffer = new byte[msg.Length];
                    msgbuffer = Encoding.UTF8.GetBytes(msg);

                    byte[] lenbuffer = new byte[2];
                    lenbuffer[0] = (byte)((UInt16)msg.Length >> 8);//高位
                    lenbuffer[1] = (byte)((UInt16)msg.Length & 0xff);//低位

                    byte[] buffer = new byte[msg.Length + 2];
                    lenbuffer.CopyTo(buffer, 0);
                    msgbuffer.CopyTo(buffer, 2);

                    socketSend.Send(buffer);
                    Debug.Log("发送的数据为：" + msg);
                }
            }
        }
        catch { }
    }


    private void OnDisable()
    {
        Debug.Log("begin OnDisable()");
        if (socketSend.Connected)
        {
            try
            {
                socketSend.Shutdown(SocketShutdown.Both);    //禁用Socket的发送和接收功能
                socketSend.Close();                          //关闭Socket连接并释放所有相关资源
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }
        Debug.Log("end OnDisable()");
    }

    void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(65+600, 10, 60, 20), "状态信息");
        GUI.Label(new Rect(135+600, 10, 80, 60), staInfo);
        GUI.Label(new Rect(65+600, 150, 80, 20), "接收到消息：");
        GUI.Label(new Rect(155+600, 150, 800, 20), recMes);
        if (GUI.Button(new Rect(65+600, 230, 60, 20), "开始连接"))
        {
            ClickConnect();
        }
        if (GUI.Button(new Rect(65+600, 270, 60, 20), "设置信息"))
        {
            clickSend = true;
        }
    }
}
