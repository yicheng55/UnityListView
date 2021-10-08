﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;
using System.Net;
using System.Timers;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using System.Windows.Forms;

public class DemoMainCanvas1 : MonoBehaviour
{
	private List<TCPTestClient> clients = new List<TCPTestClient>();
	//private TCPTestServer _server;
	private TCPTestClient _client;

	public ListView listViewVertical;
	public ListView listViewHorizontal;
	public ListView listViewTagID;
	public DemoItem itemVPrefab;
	public DemoItem itemHPrefab;

	public InputField[] tbx_Txt;

	public InputField tbx_IpAddr;
	public InputField tbx_Port;
	public Text UIlog_Status;
	public Dropdown mDropWakeUpSec;

	public Toggle m_ToggleConnect;

	public TMP_Text[] btn_Light_txt;

	private GameObject mtxt_Status;
	public GameObject[] Image_light;


	//public Text msgText;
	public static DemoMainCanvas1 instance;
	//public bool isShow;



	private string log_Status;


	////#region private members
	////private TcpClient socketConnection;
	////private Thread clientReceiveThread;
	////#endregion
	private string serverMessage;
	//public string testMsg;
	private bool running;
	private static System.Timers.Timer aTimer;
	//private char[] txt_Tagid = {'T','E','S','T'};
	public int buttonlock = -1;

	private int lastindex = -1;
	private int buttonIndexActive = -1;
	private int receiveindex = -1;

	List<string> mListMsg = new List<string>();
	List<string> statusMsg = new List<string>();

	List<string> item1 = new List<string>();

	//List<List<string>> ListView_Test = new List<List<string>>();
	//public List<TAG_ACTIVE_REPORT_STATUS> tagActiveReportStatusList = new List<TAG_ACTIVE_REPORT_STATUS>();
	private List<TAG_ACTIVE_REPORT_STATUS> tagActiveReportStatusList = new List<TAG_ACTIVE_REPORT_STATUS>();

	private List<TAGID_LIST_STATUS> tagid_status_list = new List<TAGID_LIST_STATUS>();
	TAGID_LIST_STATUS tagidListStatus = new TAGID_LIST_STATUS();

	private int tempIndex = -1;
	private int lastIndexTagId = -1;
	private int listViewTagIdIndex = -1;
	private int update_btn_LightFlage = -1;

	private struct TAG_ACTIVE_REPORT_STATUS
	{
		public string TagID;
		public string Rssi;
		public string Battery;
		public string Temperature;
		public string Counts;
		//public int Rssi;
		//public int Battery;
		//public int Temperature;
		//public int Counts;
		public string Time;
	}

	private struct TAGID_LIST_STATUS
	{
		public string TagID;
	}

	private object cacheLock = new object();
	private string cache;

	private void Awake()
	{
		instance = this;
		Debug.Log("Awake()");

		_client = GetComponent<TCPTestClient>();
		_client.OnConnected += OnClientConnected;
		_client.OnDisconnected += OnClientDisconnected;
		_client.OnMessageReceived += OnClientReceivedMessage;
		_client.OnLog += OnClientLog;
	}
	private int receiveNum = -2;
	// Start is called before the first frame update
	void Start()
	{
		int counter = 0, index = 0;
		string line;

		// Log some debug information only if this is a debug build
		//if (Debug.isDebugBuild)
		//{
		//	Debug.Log("This is a debug build!");
		//}

		//Fetch the Toggle GameObject
		//m_ToggleConnect = GetComponent<Toggle>();
		//Add listener for when the state of the Toggle changes, and output the state
		m_ToggleConnect.onValueChanged.AddListener(delegate { ToggleValueChanged(m_ToggleConnect); });
		TAG_ACTIVE_REPORT_STATUS tagActiveReportStatus = new TAG_ACTIVE_REPORT_STATUS();

		aTimer = new System.Timers.Timer(1000);
		aTimer.Elapsed += new ElapsedEventHandler(OnTick);
		aTimer.Start();


		string sPattern = "^#";
		// Read the file and display it line by line.  
		System.IO.StreamReader file = new System.IO.StreamReader(@"tcpclient.cfg");
		//System.IO.StreamReader file = new System.IO.StreamReader(@"TagidList.cfg");
		while ((line = file.ReadLine()) != null)
		{
			if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern))
			{
				Debug.Log(" - valid");
			}
			else
			{
				//Debug.Log(line);
				//Debug.Log(" - invalid");
				switch (index)
				{
					case 0:
						tbx_IpAddr.text = line;
						Debug.Log("Case= " + index + "  line: " + line);
						break;
					case 1:
						tbx_Port.text = line;
						Debug.Log("Case= " + index + "  line: " + line);
						break;

					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
					case 8:
					case 9:
					case 10:
					case 11:
						tbx_Txt[index - 2].text = line;
						tagActiveReportStatus.TagID = line;
						//tagActiveReportStatusList.Add(tagActiveReportStatus);
						////AddItem(listViewVertical, itemVPrefab, index.ToString());
						//AddItem(listViewTagID, itemVPrefab, line);
						//Debug.Log("tagActiveReportStatusList = " + tagActiveReportStatusList.Count.ToString());
						Debug.Log("Case= " + index + "  line: " + line);
						break;

					default:
						Debug.Log("Case= " + index + "  line: " + line);
						break;


				}
				index++;
			}

			counter++;
		}

		file.Close();
		Debug.Log("tcpclient.cfg were lines=" + counter);

		index = 0;
		counter = 0;
		file = new System.IO.StreamReader(@"TagidList.cfg");
		while ((line = file.ReadLine()) != null)
		{
			if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern))
			{
				Debug.Log(" - valid");
			}
			else
			{
				//Debug.Log(line);
				//Debug.Log(" - invalid");
				//////switch (index)
				//////{
				//////	//case 0:
				//////	//	//tbx_IpAddr.text = line;
				//////	//	//Debug.Log("Case= " + index + "  line: " + line);
				//////	//	//break;
				//////	//case 1:
				//////	//	//tbx_Port.text = line;
				//////	//	//Debug.Log("Case= " + index + "  line: " + line);
				//////	//	//break;
				//////	case int n when (n < 20):
				//////		//tbx_Txt[index - 2].text = line;
				//////		//tagActiveReportStatus.TagID = line;
				//////		//tagActiveReportStatusList.Add(tagActiveReportStatus);
				//////		////AddItem(listViewVertical, itemVPrefab, index.ToString());
				//////		///
				//////		tagidListStatus.TagID = line;
				//////		tagid_status_list.Add(tagidListStatus);
				//////		//AddItem(listViewTagID, itemVPrefab, line);
				//////		//Debug.Log("tagActiveReportStatusList = " + tagActiveReportStatusList.Count.ToString());
				//////		Debug.Log("Case= " + index + "  line: " + line);
				//////		break;

				//////	default:
				//////		Debug.Log("Case= " + index + "  line: " + line);
				//////		break;
				//////}

				if (String.Equals(line, "END"))
				{
					Debug.Log("index= " + index + "  line: " + line);
					break;
				}

				tagidListStatus.TagID = line;
				tagid_status_list.Add(tagidListStatus);
				Debug.Log("index= " + index + "  line: " + line);
				index++;
			}

			counter++;
		}

		file.Close();

		tagid_status_list = tagid_status_list.OrderBy(sel => sel.TagID).ToList();       //using System.Linq;

		Debug.Log("TagidList.cfg were lines=" + counter);
		Debug.Log("tagid_status_list = " + tagid_status_list.Count.ToString());

		counter = 0;
		foreach (TAGID_LIST_STATUS myStringList in tagid_status_list)
		{
			//Debug.Log(myStringList.TagID);
			cache = counter.ToString("000") + "   |   " + myStringList.TagID;
			AddItem(listViewTagID, itemVPrefab, cache, counter);
			counter++;
		}

		//this.OnClientLog("Start...............");
		mtxt_Status = GameObject.Find("txt_Status");
		mtxt_Status.GetComponent<Text>().text = "TextStatus - " + "Start...............";
		Debug.Log("mDropWakeUpSec.captionText.text =" + mDropWakeUpSec.captionText.text);
		//mDropWakeUpSec.captionText.text = "Select";
		//mDropWakeUpSec.value = 10;
		Debug.Log("mDropWakeUpSec.captionText.text =" + mDropWakeUpSec.captionText.text);

		Debug.Log("statusMsg.Count: " + statusMsg.Count);
	}

	private void Update()
	{
		string log_Status = "";
		//      if (Input.GetKeyDown(KeyCode.V))
		//      {
		//          if (Input.GetKey(KeyCode.LeftShift)) // shift + v: remove
		//          {
		//              RemoveItem(listViewVertical);
		//	}
		//          else // v: add
		//          {
		//              //AddItem(listViewVertical, itemVPrefab, serverMessage);
		//          }
		//      }

		//if (Input.GetKeyDown(KeyCode.A))
		//{
		//	if (Input.GetKey(KeyCode.LeftShift)) // shift + A: remove
		//	{
		//		RemoveItemAll(listViewVertical);
		//	}
		//	else // v: add
		//	{
		//		//AddItem(listViewVertical, itemVPrefab, serverMessage);
		//	}
		//}

		//Debug.Log("testMsg = " + testMsg);

		if (receiveNum > -2)
		{
			//if(receiveNum >=0 )
			//         {
			//	UpdateMsg(receiveNum);
			//	//txt_Status.text = "tagCnt = " + tagActiveReportStatusList.Count.ToString() + "  ListCnt = " + listViewVertical.ItemCount;
			//	receiveNum = -2;
			//}
		}

		//Debug.Log("tagCnt = " + tagid_status_list.Count.ToString() + "  ListCnt = " + listViewTagID.ItemCount);
		//mtxt_Status = GameObject.Find("txt_Status");
		//log_Status = mtxt_Status.GetComponent<Text>().text;

		//log_Status = UIlog_Status.text;
		//tempIndex = tagid_status_list.FindIndex(z => z.TagID == log_Status);
		//tempIndex = listViewTagIdIndex;

		//*********************************
		//////if (listViewTagIdIndex >= 0)
		//////      {

		//////          UpdateTagIdList(listViewTagIdIndex);
		//////          //cache = string.Format("<color=red>{0}</color>\n", tagid_status_list[listViewTagIdIndex].TagID);    //red
		//////          ////mtxt_Status.GetComponent<Text>().text = cache;

		//////          if (lastindex >= 0)
		//////          {
		//////              Image_light[lastindex].SetActive(true);
		//////              btn_Light_txt[lastindex].text = "OFF";
		//////              lastindex = -1;
		//////          }

		//////          lastIndexTagId = listViewTagIdIndex;
		//////	listViewTagIdIndex = -1;
		//////	//Debug.Log("tagCnt = " + tagid_status_list.Count.ToString() + "  ListCnt = " + listViewTagID.ItemCount);
		//////	//receiveNum = -2;
		//////}
		///
		//******************************************

		//if (Input.GetKeyDown(KeyCode.H))
		//{
		//    if (Input.GetKey(KeyCode.LeftShift)) // shift + h: remove
		//    {
		//        RemoveItem(listViewHorizontal);
		//    }
		//    else // h: add
		//    {
		//        AddItem(listViewHorizontal, itemHPrefab);
		//    }
		//}


		//if(mListMsg.Count > 0)
		//{
		//          //RemoveItemAll(listViewVertical);
		//          for (int i=0; i < mListMsg.Count; i++)
		//          {
		//              Debug.Log(mListMsg[i]);
		//              AddItem(listViewVertical, itemVPrefab, mListMsg[i]);

		//	}
		//	//foreach (string myStringList in mListMsg)  //error bug.
		//	//{
		//	//	Debug.Log(myStringList);
		//	//	AddItem(listViewVertical, itemVPrefab, myStringList);
		//	//}
		//	mListMsg.Clear();
		//}
		//if(statusMsg.Count > 0)
		//      {
		//	txt_Status.text = "";
		//	string s = new string(txt_Tagid);
		//	for (int i = 0; i<statusMsg.Count; i++)
		//          {
		//              //txt_Status.text += statusMsg[i];
		//              txt_Status.text += statusMsg[i] + "TagID =" + s;
		//          }
		//	statusMsg.Clear();
		//      }

		//Debug.Log("listViewVertical =" + listViewVertical.FindItems( ) );
		//lock (cacheLock)
		//{
		//	if (!string.IsNullOrEmpty(cache))
		//	{
		//		//TextWindow.text += string.Format("{0}", cache);
		//		AddItem(listViewVertical, itemVPrefab, cache);
		//		cache = null;
		//	}
		//}

		if (update_btn_LightFlage >= 0)
		{
			if (receiveindex == 1)
			{

				UIlog_Status.text = "WakeUp set successful";
				receiveindex = -1;
			}
			else if (receiveindex == -2)
			{

				UIlog_Status.text = "Time out......";
				receiveindex = -1;
			}
			else
			{
				Debug.Log("lastindex = " + lastindex + "    ,buttonIndexActive = " + buttonIndexActive);
				if (lastindex == buttonIndexActive)
				{
					//Image_light[lastindex].SetActive(true);
                    LightManager.instance.TurnOff(lastindex);
                    //btn_Light_txt[lastindex].text = "OFF";
					lastindex = -1;
					buttonIndexActive = -1;
					Debug.Log("btn_Light_txt = OFF");
					UIlog_Status.text = "Button light = OFF";
				}
				else
				{
					if (lastindex >= 0)
					{
						//Image_light[lastindex].SetActive(true);
						//btn_Light_txt[lastindex].text = "OFF";
						LightManager.instance.TurnOff(lastindex);
					}
					////buttonIndexActive = index;
					//Image_light[buttonIndexActive].SetActive(false);
					//btn_Light_txt[buttonIndexActive].text = "ON";
					LightManager.instance.TurnOn(buttonIndexActive);
					lastindex = buttonIndexActive;
					Debug.Log("btn_Light_txt = ON");
					UIlog_Status.text = "Button light = ON";
				}

			}

			update_btn_LightFlage = -1;
		}

	}

	private void OnTick(object source, ElapsedEventArgs e)
	{
		//print(e.SignalTime);
		//receiveNum = 0;

		//Debug.Log("tagCnt = " + tagid_status_list.Count.ToString() + "  ListCnt = " + listViewTagID.ItemCount);
		//tempIndex = tagid_status_list.FindIndex(z => z.TagID == log_Status.text);

		//if (tempIndex >= 0)
		//{
		//	Debug.Log("OnTick log_Status = " + log_Status.text + ",  index = " + tempIndex);
		//	UpdateTagIdList(tempIndex);
		//	//Debug.Log("tagCnt = " + tagid_status_list.Count.ToString() + "  ListCnt = " + listViewTagID.ItemCount);
		//	//receiveNum = -2;
		//}

	}

	public void getButtonClickMsg(string msg, int itemno)
	{
		//////Debug.Log("getButtonClickMsg(msg:)" + msg);
		Debug.Log("listViewTagID.ItemCount: " + listViewTagID.ItemCount + "   itemno=" + itemno);
		Debug.Log("TagID= " + tagid_status_list[itemno].TagID);
		UIlog_Status.text = msg;
		listViewTagIdIndex = itemno;

		UpdateTagIdList(listViewTagIdIndex);
		if (lastindex >= 0)
		{
			Image_light[lastindex].SetActive(true);
			btn_Light_txt[lastindex].text = "OFF";
			lastindex = -1;
		}

		lastIndexTagId = listViewTagIdIndex;
		//listViewTagIdIndex = -1;


		//listViewTagID.GetItem(i).GetComponent<DemoItem>().SetText(cache);
	}


	//private void OnDisable()
	//{
	//	aTimer.Dispose();
	//	print(aTimer == null);
	//}

	public void ConnectButton()
	{
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
		//Debug.Log("ItemCount: " + listViewVertical.ItemCount);
		//if (listViewVertical.ItemCount > 0)
		//      {
		//	RemoveItemAll(listViewVertical);
		//}

		ConnectClient();
	}


	public void OpenFileButton()
	{
		int counter = 0, index = 0;
		string line;
		string sPattern = "^#";
		string filename;

		//MessageBoxButtons buttons = MessageBoxButtons.OK;
		//MessageBox.Show(" Input TAG_ID warning!!! ", "Warning...    ", buttons);   //MessageBox.Show Test OK.

		OpenFileDialog dialog = new OpenFileDialog();
		dialog.Filter = "exe files (*.cfg)|*.cfg";  //过滤文件类型  
		dialog.InitialDirectory = ".";  //定义打开的默认文件夹位置，可以在显示对话框之前设置好各种属性  
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			filename = dialog.FileName;
			Debug.Log(filename);
			tagid_status_list.Clear();
			RemoveItemAll(listViewTagID);

			System.IO.StreamReader file = new System.IO.StreamReader(@filename);
			while ((line = file.ReadLine()) != null)
			{
				if (System.Text.RegularExpressions.Regex.IsMatch(line, sPattern))
				{
					Debug.Log(" - valid");
				}
				else
				{

					if (String.Equals(line, "END"))
					{
						Debug.Log("index= " + index + "  line: " + line);
						break;
					}

					tagidListStatus.TagID = line;
					tagid_status_list.Add(tagidListStatus);
					Debug.Log("index= " + index + "  line: " + line);
					index++;
				}
			}

			file.Close();
			tagid_status_list = tagid_status_list.OrderBy(sel => sel.TagID).ToList();       //using System.Linq;

			Debug.Log("TagidList.cfg were lines=" + counter);
			Debug.Log("tagid_status_list = " + tagid_status_list.Count.ToString());


			counter = 0;
			foreach (TAGID_LIST_STATUS myStringList in tagid_status_list)
			{
				//Debug.Log(myStringList.TagID);
				cache = counter.ToString("000") + "   |   " + myStringList.TagID;
				AddItem(listViewTagID, itemVPrefab, cache, counter);
				counter++;
			}

			//this.OnClientLog("Start...............");
			mtxt_Status = GameObject.Find("txt_Status");
			mtxt_Status.GetComponent<Text>().text = "TextStatus - " + "Start...............";
			Debug.Log("mDropWakeUpSec.captionText.text =" + mDropWakeUpSec.captionText.text);
			//mDropWakeUpSec.captionText.text = "Select";
			//mDropWakeUpSec.value = 10;
			Debug.Log("mDropWakeUpSec.captionText.text =" + mDropWakeUpSec.captionText.text);

			Debug.Log("statusMsg.Count: " + statusMsg.Count);

		}
	}



	void ToggleValueChanged(Toggle change)
	{
		//m_Text.text = "Toggle is : " + m_Toggle.isOn;
		//Debug.Log("Toggle is : " + change.isOn);
		Debug.Log("Toggle is : " + change.isOn);
		if (change.isOn == true)
		{
			if (!_client.IsConnected)
			{
				Debug.Log("ConnectButton();");
				ConnectButton();
			}
		}
		else
		{

			//_client.SendMessage("!disconnect");
			//running = false;
			Debug.Log("DisconnectClient();");
			DisconnectClient();
			//string clientMessage = "Socket Connection is diasble!!!";
			//AddItem(listViewVertical, itemVPrefab, clientMessage);
			//if (socketConnection != null)
			//{
			//	Debug.Log("socketConnection.Close();");
			//	socketConnection.Close();
			//	clientReceiveThread.Abort();
			//	string clientMessage = "Socket Connection is diasble!!!";
			//	AddItem(listViewVertical, itemVPrefab, clientMessage);
			//}
			//DisConnectButton();
		}

	}


	public void DropValueChanged(Text change)
	{
		String output;
		//m_Text.text = "Toggle is : " + m_Toggle.isOn;
		//Debug.Log("Toggle is : " + change.isOn);
		Debug.Log("DropValueChanged is : " + change.text);


		if (!_client.IsConnected)
		{
			string clientMessage = "SocketConnection is diasble!!!";
			//AddItem(listViewVertical, itemVPrefab, clientMessage);
			mtxt_Status = GameObject.Find("txt_Status");
			mtxt_Status.GetComponent<Text>().text = "TextStatus - " + clientMessage;
			return;
		}
		try
		{

			mtxt_Status = GameObject.Find("txt_Status");

			//string clientMessage = tbx_Txt[index].text;
			string clientMessage = "QSDM,2,{0},2,0,{1},0,0,1,QEDM";

			//output = String.Format(clientMessage, mtxt_Status.GetComponent<Text>().text, (index + 1).ToString("X02"));
			output = String.Format(clientMessage, tagid_status_list[lastIndexTagId].TagID, change.text);


			Debug.Log(" --- output : " + output);
			UIlog_Status.text = output;
			receiveindex = 1;

			//System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
			//string output = regex.Replace(clientMessage, "Bob");
			//Debug.Log(" --- output : " + output);

			//if (System.Text.RegularExpressions.Regex.IsMatch(clientMessage, sPattern))
			//{
			//	Debug.Log(" - IsMatch");
			//}

			clientMessage = output;
			if (!string.IsNullOrEmpty(clientMessage))
			{
				if (_client.strSendMessage(clientMessage))
				{
					//MessageInputField.text = string.Empty;
				}
			}
		}
		catch (ArgumentOutOfRangeException outOfRange)
		{
			string clientMessage = "Socket Connection is fail !!!";
			Debug.Log("outOfRange exception: " + outOfRange);
			//AddItem(listViewVertical, itemVPrefab, clientMessage);
			mtxt_Status = GameObject.Find("txt_Status");
			mtxt_Status.GetComponent<Text>().text = "Status - " + outOfRange;
		}


	}

	public void SendButton(int index)
	{
		Debug.Log("SendButton= " + index);
		if (!_client.IsConnected)
		{
			string clientMessage = "SocketConnection is diasble!!!";
			//AddItem(listViewVertical, itemVPrefab, clientMessage);
			return;
		}
		try
		{
			string clientMessage = tbx_Txt[index].text;

			if (!string.IsNullOrEmpty(clientMessage))
			{
				if (_client.strSendMessage(clientMessage))
				{
					//MessageInputField.text = string.Empty;
				}
			}

			//// Get a stream object for writing.
			//NetworkStream stream = socketConnection.GetStream();
			//if (stream.CanWrite)
			//{
			//	//string clientMessage = "This is a message from one of your clients.";
			//	string clientMessage = tbx_Txt[index].text;
			//	// Convert string message to byte array.
			//	byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
			//	// Write byte array to socketConnection stream.
			//	stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
			//	Debug.Log("Client sent his message - should be received by server: " + clientMessage);
			//	//AddItem(listViewVertical, itemVPrefab, clientMessage);
			//	OnLog(clientMessage);
			// }
		}
		catch (SocketException socketException)
		{
			string clientMessage = "Socket Connection is fail !!!";
			Debug.Log("Socket exception: " + socketException);
			//AddItem(listViewVertical, itemVPrefab, clientMessage);
		}


	}


	public void SendButtonGPIO(int index)
	{
		String output;
		//string sPattern = "^#";

		Debug.Log("SendButtonGPIO= " + index);
		//Debug.Log("SendButtonGPIO serverMessage= " + serverMessage);
		if (buttonlock > 0  || receiveindex > 0)
        {
			Debug.Log("SendButtonGPIO buttonlock= " + buttonlock + "  receiveindex= " + receiveindex);
			return;
		}

		//Image_red = GameObject.Find("Image_red_Light");
		//Image_red.GetComponent<Text>().text = "ONN222";
		//Text btn_Red_Light_text = btn_Red_Light.transform.GetComponent<Text>();

		if (!_client.IsConnected)
		{
			string clientMessage = "SocketConnection is diasble!!!";
			//AddItem(listViewVertical, itemVPrefab, clientMessage);
			mtxt_Status = GameObject.Find("txt_Status");
			mtxt_Status.GetComponent<Text>().text = "TextStatus - " + clientMessage;
			return;
		}
		try
		{

			mtxt_Status = GameObject.Find("txt_Status");
			
			string clientMessage = tbx_Txt[index].text;

			if(lastindex == index )
            {
				//output = String.Format(clientMessage, mtxt_Status.GetComponent<Text>().text, "00");
				output = String.Format(clientMessage, tagid_status_list[lastIndexTagId].TagID, "0");
				//////Image_light[index].SetActive(true);
				//////btn_Light_txt[index].text = "OFF";
				//////lastindex = -1;
			}
			else
            {
				//output = String.Format(clientMessage, tagid_status_list[lastIndexTagId].TagID, (index + 1).ToString("X02"));
				output = String.Format(clientMessage, tagid_status_list[lastIndexTagId].TagID, (index + 1).ToString());
                //////if ( lastindex >= 0)
                //////            {
                //////	Image_light[lastindex].SetActive(true);
                //////	btn_Light_txt[lastindex].text = "OFF";
                //////}
                buttonIndexActive = index;
                //////Image_light[index].SetActive(false);
                //////btn_Light_txt[index].text = "ON";
                //////lastindex = index;
            }

            Debug.Log(" --- output : " + output);
			UIlog_Status.text = output;

			//System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
			//string output = regex.Replace(clientMessage, "Bob");
			//Debug.Log(" --- output : " + output);

			//if (System.Text.RegularExpressions.Regex.IsMatch(clientMessage, sPattern))
			//{
			//	Debug.Log(" - IsMatch");
			//}

			clientMessage = output;
			if (!string.IsNullOrEmpty(clientMessage))
			{
				if (_client.strSendMessage(clientMessage))
				{
					buttonlock = 1;
					//MessageInputField.text = string.Empty;
				}
			}
		}
		catch (SocketException socketException)
		{
			string clientMessage = "Socket Connection is fail !!!";
			Debug.Log("Socket exception: " + socketException);
			//AddItem(listViewVertical, itemVPrefab, clientMessage);
			mtxt_Status = GameObject.Find("txt_Status");
			mtxt_Status.GetComponent<Text>().text = "TextStatus - " + clientMessage;
		}
	}

	//***************************************************************************************

	public void ConnectClient()
	{
		string message;
		if (!_client.IsConnected)
		{
			Debug.Log("ConnectClient()....");
			this.OnClientLog("ConnectClient()....");
			message = string.Format("Connecting to {0}:{1}", tbx_IpAddr.text, tbx_Port.text);
			UIlog_Status.text = message;
			_client.IPAddress = tbx_IpAddr.text;
			int.TryParse(tbx_Port.text, out _client.Port);
			_client.ConnectToTcpServer();
		}
	}

	public void DisconnectClient()
	{
		if (_client.IsConnected)
		{
			this.OnClientLog("DisconnectClient()....");
			Debug.Log("DisconnectClient()....");
			UIlog_Status.text = "DisconnectClient()....";
			_client.CloseConnection();
		}
	}

	////public void SendMessageToServer()
	////{
	////	if (_client.IsConnected)
	////	{
	////		string message = MessageInputField.text;
	////		if (message.StartsWith("!ping"))
	////		{
	////			message += " " + (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
	////		}

	////		if (!string.IsNullOrEmpty(message))
	////		{
	////			if (_client.SendMessage(message))
	////			{
	////				MessageInputField.text = string.Empty;
	////			}
	////		}
	////	}
	////}


	private void OnClientReceivedMessage(string message)
	{
		string finalMessage = message;
		Debug.Log("OnClientReceivedMessage: " + message);

		//CSV 解碼
		var regex = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
		var matches = regex.Matches(finalMessage);
		int csv_total_fields = matches.Count;
		string[] ack_data = new string[32];
		int idx = 0;
		string tag_id = "";

		if (csv_total_fields < 5)
		{
			Console.WriteLine("Receive Test CSV Field < 5");
			return;
		}
		//
		foreach (Match m in matches)
		{
			string s = (string)m.Value;
			ack_data[idx] = s.ToString().Replace('"', ' ').Trim();
			idx++;
		}
		for (; idx < 24; idx++)
		{
			ack_data[idx] = "";
		}
		tag_id = ack_data[2];

		string author1 = "2100";

        if (String.Equals(tag_id, author1))
        {
			Debug.Log("tag_id = 2100 compare");
			update_btn_LightFlage = 1;
			if (receiveindex == 1)
            {
				Debug.Log("Other receive");
			}
            else
            {
				buttonlock = -1;
			}
		}
		else
        {
			update_btn_LightFlage = 1;
			receiveindex = -2;
			buttonlock = -1;
			Debug.Log("Code: " + tag_id);
		}

		//return ;

//////if(receiveindex == 1)
//////      {
//////	if (String.Equals(tag_id, author1))
//////	{
//////		Debug.Log("tag_id = 2100 compare");
//////		update_btn_LightFlage = 1;
//////		receiveindex = -2;
//////	}
//////}
//////      else
//////      {
//////	if (String.Equals(tag_id, author1))
//////	{
//////		Debug.Log("tag_id = 2100 compare");
//////		update_btn_LightFlage = 1;

//////	}
//////	buttonlock = -1;
//////}

#if false
		tempIndex = tagActiveReportStatusList.FindIndex(z => z.TagID == tag_id);
		//Debug.Log("tempIndex... : " + tempIndex);
		if( tempIndex == -1)
        {
			TAG_ACTIVE_REPORT_STATUS tagActiveReportStatus = new TAG_ACTIVE_REPORT_STATUS();
			tagActiveReportStatus.TagID = tag_id;
			tagActiveReportStatus.Rssi = ack_data[3];
			tagActiveReportStatus.Battery = ack_data[4];
			tagActiveReportStatus.Temperature = ack_data[5];
			tagActiveReportStatus.Counts = "1";
			tagActiveReportStatus.Time = ConvertIntDateTime(Convert.ToInt32(ack_data[1])).ToString();
			tagActiveReportStatusList.Add(tagActiveReportStatus);

            //////cache = string.Format("<color=red>{0}  |  {1} </color>\n", tagActiveReportStatusList[tagActiveReportStatusList.Count-1].TagID, tagActiveReportStatusList[tagActiveReportStatusList.Count - 1].Counts);
            //////mListMsg.Add(cache);
            ////////AddMsg();
            //////receiveNum = -1;

            //cache = string.Format("<color=red>{0}  |  {1} </color>\n", tagActiveReportStatus.TagID, tagActiveReportStatus.Counts);
            //mListMsg.Add(cache);

            //for (int i = 0; i < tagActiveReportStatusList.Count; i++)
            //{
            //	Debug.Log(tagActiveReportStatusList[i].TagID);
            //	cache = string.Format("<color=red>{0}  |  {1} </color>\n", tagActiveReportStatusList[i].TagID, tagActiveReportStatusList[i].Counts);
            //	mListMsg.Add(cache);
            //}
            Debug.Log("tagActiveReportStatusList.Count = " + tagActiveReportStatusList.Count);
			Debug.Log(tagActiveReportStatus.TagID + "   |   " + tagActiveReportStatus.Counts);
		}
        else
        {
			//tagActiveReportStatusList[tempIndex].TagID = tag_id;

			//tagActiveReportStatusList[tempIndex].Counts = "2";
			//tagActiveReportStatusList[tempIndex].TagID = tag_id;
			TAG_ACTIVE_REPORT_STATUS tagActiveReportStatus = new TAG_ACTIVE_REPORT_STATUS();

			tagActiveReportStatus.TagID = tag_id;
			tagActiveReportStatus.Rssi = ack_data[3];
			tagActiveReportStatus.Battery = ack_data[4];
			tagActiveReportStatus.Temperature = ack_data[5];
			tagActiveReportStatus.Counts = (Convert.ToInt32(tagActiveReportStatusList[tempIndex].Counts)+1).ToString();
			tagActiveReportStatus.Time = ConvertIntDateTime(Convert.ToInt32(ack_data[1])).ToString();

			tagActiveReportStatusList[tempIndex] = tagActiveReportStatus;

			Debug.Log(tagActiveReportStatusList[tempIndex].TagID + "   |   " + tagActiveReportStatus.Counts);
		}
#endif
	}
	public void AddMsg()
    {
		//AddItem(listViewVertical, itemVPrefab, mListMsg[mListMsg.Count-1]);
	}
	public void AddMsg(string msg)
	{
		//AddItem(listViewVertical, itemVPrefab, msg);
	}

	public void UpdateMsg(int index)
    {
		if(tagActiveReportStatusList.Count > listViewVertical.ItemCount)
        {
            for (int i = listViewVertical.ItemCount ; i < tagActiveReportStatusList.Count; i++)
            {
                cache = string.Format("<color=red>{0}  |  {1} </color>\n", tagActiveReportStatusList[i].TagID, tagActiveReportStatusList[i].Counts);
				AddItem(listViewVertical, itemVPrefab, cache,i);
				//Debug.Log(cache);
			}

        }

		for (int i = 0; i < listViewVertical.ItemCount; i++)
		{
			cache = string.Format("<color=red>{0}  |  {1} </color>\n", tagActiveReportStatusList[i].TagID, tagActiveReportStatusList[i].Counts);
			listViewVertical.GetItem(i).GetComponent<DemoItem>().SetText(cache);
		}

		//listViewVertical.GetItem(mListMsg.Count - 1 - index).GetComponent<DemoItem>().SetText(mListMsg[index]);
		//if (mListMsg.Count > 0)
		//{
		//	//RemoveItemAll(listViewVertical);
		//	for (int i = 0; i < mListMsg.Count; i++)
		//	{
		//		Debug.Log(mListMsg[i]);
		//		AddItem(listViewVertical, itemVPrefab, mListMsg[i]);

		//	}
		//	//foreach (string myStringList in mListMsg)  //error bug.
		//	//{
		//	//	Debug.Log(myStringList);
		//	//	AddItem(listViewVertical, itemVPrefab, myStringList);
		//	//}
		//	//mListMsg.Clear();
		//}
	}



	public void UpdateTagIdList(int index)
	{
		if (tagid_status_list.Count > listViewTagID.ItemCount)
		{
			for (int i = listViewTagID.ItemCount; i < tagid_status_list.Count; i++)
			{
				//cache = string.Format("<color=black>{0}</color>\n", tagid_status_list[i].TagID);      //black
				//AddItem(listViewTagID, itemVPrefab, cache);
				//Debug.Log(cache);
				AddItem(listViewTagID, itemVPrefab, tagid_status_list[i].TagID, i);
			}

		}

		for (int i = 0; i < listViewTagID.ItemCount; i++)
		{
			if( i == index)
            {
				//Debug.Log("<color=red> tagCnt = " + tagid_status_list.Count.ToString() + "  ListCnt = " + listViewTagID.ItemCount);
				cache = string.Format("<color=red>{0}    |   {1}</color>\n", i.ToString("000"), tagid_status_list[i].TagID);    //red
				Debug.Log("<color=red> cache = " + cache);
			}
            else
            {
				//cache = string.Format("<color=black>{0}</color>\n", tagid_status_list[i].TagID);        //black
				cache = i.ToString("000") + "   |   " + tagid_status_list[i].TagID;
				Debug.Log("cache = " + cache);
			}

			listViewTagID.GetItem(i).GetComponent<DemoItem>().SetText(cache);
		}

	}


	/// 将Unix时间戳转换为DateTime类型时间
	/// </summary>
	/// <param name="d">double 型数字</param>
	/// <returns>DateTime</returns>
	public static System.DateTime ConvertIntDateTime(double d)
	{
		System.DateTime time = System.DateTime.MinValue;
		System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0));
		//Debug.Log(startTime);
		time = startTime.AddSeconds(d);
		return time;
	}


	////private void OnClientLog(string message)
	////{
	////	lock (cacheLock)
	////	{
	////		if (string.IsNullOrEmpty(cache))
	////		{
	////			cache = string.Format("<color=grey>{0}</color>\n", message);
	////		}
	////		else
	////		{
	////			cache += string.Format("<color=grey>{0}</color>\n", message);
	////		}
	////	}
	////}

	////private void OnServerReceivedMessage(string message)
	////{
	////	lock (cacheLock)
	////	{
	////		if (string.IsNullOrEmpty(cache))
	////		{
	////			cache = string.Format("<color=red>{0}</color>\n", message);
	////		}
	////		else
	////		{
	////			cache += string.Format("<color=red>{0}</color>\n", message);
	////		}
	////	}
	////}

	private void OnClientLog(string message)
	{
		lock (cacheLock)
		{
			Debug.Log("OnClientLog: " + message);
			//if (string.IsNullOrEmpty(cache))
			//{
			//	cache = string.Format("<color=black>{0}</color>\n", message);
			//	//cache = string.Format("<color=red>{0}</color>\n", message);
			//}
			//else
			//{
			//	cache += string.Format("<color=black>{0}</color>\n", message);
			//	//cache += string.Format("<color=red>{0}</color>\n", message);
			//}

			cache = string.Format("<color=black>{0}</color>\n", message);
            //mListMsg.Add(cache);
            statusMsg.Add(cache);
        }
	}


	private void AddItem(ListView lv, DemoItem prefab, string msg,int index)
    {
        var color = new Color()
        {
			//r = UnityEngine.Random.Range(0.0f, 1.0f),
			//g = UnityEngine.Random.Range(0.0f, 1.0f),
			//b = UnityEngine.Random.Range(0.0f, 1.0f),
			//r = UnityEngine.Random.Range(0.0f, 0.5f),
			//g = UnityEngine.Random.Range(0.0f, 0.5f),
			//b = UnityEngine.Random.Range(0.0f, 0.5f),
			//a = 1.0f,
			//r = UnityEngine.Random.Range(0.3f, 1.0f),
			//         g = UnityEngine.Random.Range(0.3f, 1.0f),
			//         b = UnityEngine.Random.Range(0.3f, 1.0f),
			//         a = 0.5f,

			r = 1.0f,
			g = 1.0f,
			b = 1.0f,
			a = 0.5f,

		};
		Debug.Log("AddItem msg: " + msg);
		var item = Instantiate(prefab,lv.transform.position,Quaternion.identity);

		if (msg == "")
		{
            item.SetContent(color.ToString(), color);
        }
		else
        {
			item.SetContent(msg, color);
		}

		item.index = index;

		lv.AddItem(item.gameObject);
        Debug.Log("lv.ItemCount= " + lv.ItemCount);

		//prefab.gameObject.GetComponent<Button>().onClick.AddListener(listViewOnClickTest);
    }

    private void RemoveItem(ListView lv)
    {
        lv.RemoveBottom();
        //lv.RemoveTop();
        Debug.Log("lv.ItemCount= " + lv.ItemCount);
    }

	private void RemoveItemAll(ListView lv)
	{
		Debug.Log("lv.RemoveAllItems= " + lv.ItemCount);
		//lv.RemoveBottom();
		lv.RemoveAllItems();

	}

#if false

    /// <summary>
    /// Setup socket connection.
    /// </summary>
    private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
			Debug.Log("clientReceiveThread.Start(); ");
		}
		catch (Exception e)
		{
			string clientMessage = "Socket Connection is fail !!!";
			Debug.Log("On client connect exception " + e);
			AddItem(listViewVertical, itemVPrefab, clientMessage);
		}
	}
	/// <summary>
	/// Runs in background clientReceiveThread; Listens for incomming data.
	/// </summary>
	private void ListenForData()
	{
		try
		{
			string host = tbx_IpAddr.text;
			//IPAddress ip = IPAddress.Parse(host);
			Debug.Log("host: " + host + "  port: " + tbx_Port.text);
            socketConnection = new TcpClient(host, Convert.ToInt16(tbx_Port.text));
			//socketConnection = new TcpClient("localhost", 21087);
			string clientMessage = "Socket Connection is successful !!!";
			mListMsg.Add(clientMessage);
			Byte[] bytes = new Byte[1024];
			running = true;
			while (running)
			{
				// Get a stream object for reading
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incomming stream into byte arrary.
					while (running && stream.CanRead)
					{

						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message.
							serverMessage = Encoding.ASCII.GetString(incommingData);
							Debug.Log("server message received as: " + serverMessage);
							//mListMsg.Add(serverMessage);
							OnLog(serverMessage);

						}
					}
				}
			}
			socketConnection.Close();
			Debug.Log("Disconnected from server");
			//OnDisconnected(this);
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
			string clientMessage = "Socket Connection is fail !!!";
			mListMsg.Add(clientMessage);
		}
	}
	/// <summary>
	/// Send message to server using socket connection.
	/// </summary>
	private void SendMessage()
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing.
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				string clientMessage = "This is a message from one of your clients.";
				// Convert string message to byte array.
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
#endif



	private void OnClientConnected(TCPTestClient client)
	{
		//Debug.Log("OnClientDisconnected: " + client);
		clients.Add(client);
	}

	private void OnClientDisconnected(TCPTestClient client)
	{
		Debug.Log("OnClientDisconnected: ");
        clients.Remove(client);
	}

}
