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
using System.Text.RegularExpressions;


public class DemoMain : MonoBehaviour
{
	private List<TCPTestClient> clients = new List<TCPTestClient>();
	//private TCPTestServer _server;
	private TCPTestClient _client;

	public ListView listViewVertical;
    public ListView listViewHorizontal;
    public DemoItem itemVPrefab;
    public DemoItem itemHPrefab;

	public InputField[] tbx_Txt;

	public InputField tbx_IpAddr;
	public InputField tbx_Port;

	////#region private members
	////private TcpClient socketConnection;
	////private Thread clientReceiveThread;
	////#endregion
	private string serverMessage;
	private bool running;
	private static System.Timers.Timer aTimer;

	public Toggle m_ToggleConnect;

    List<string> mListMsg = new List<string>();

	List<string> item1 = new List<string>();

    //List<List<string>> ListView_Test = new List<List<string>>();
    //public List<TAG_ACTIVE_REPORT_STATUS> tagActiveReportStatusList = new List<TAG_ACTIVE_REPORT_STATUS>();
    private List<TAG_ACTIVE_REPORT_STATUS> tagActiveReportStatusList = new List<TAG_ACTIVE_REPORT_STATUS>();
    int tempIndex;

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

	private object cacheLock = new object();
	private string cache;

	private void Awake()
	{
		Debug.Log("Awake()");
		_client = GetComponent<TCPTestClient>();
		_client.OnConnected += OnClientConnected;
		_client.OnDisconnected += OnClientDisconnected;
		_client.OnMessageReceived += OnClientReceivedMessage;
		_client.OnLog += OnClientLog;
	}

		// Start is called before the first frame update
	void Start()
	{
		int counter = 0, index=0;
		string line;


        //Fetch the Toggle GameObject
        //m_ToggleConnect = GetComponent<Toggle>();
        //Add listener for when the state of the Toggle changes, and output the state
        m_ToggleConnect.onValueChanged.AddListener(delegate { ToggleValueChanged(m_ToggleConnect); });


		aTimer = new System.Timers.Timer(200);
		aTimer.Elapsed += new ElapsedEventHandler(OnTick);
		aTimer.Start();

		string sPattern = "^#";
		// Read the file and display it line by line.  
		System.IO.StreamReader file = new System.IO.StreamReader(@"tcpclient.cfg");
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
				switch(index)
                {
					case 0:
						tbx_IpAddr.text = line;
						Debug.Log("Case= " + index +"  line: " + line);
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
		Debug.Log("There were lines=" + counter);
	}

	private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (Input.GetKey(KeyCode.LeftShift)) // shift + v: remove
            {
                RemoveItem(listViewVertical);
			}
            else // v: add
            {
                //AddItem(listViewVertical, itemVPrefab, serverMessage);
            }
        }

		if (Input.GetKeyDown(KeyCode.A))
		{
			if (Input.GetKey(KeyCode.LeftShift)) // shift + A: remove
			{
				RemoveItemAll(listViewVertical);
			}
			else // v: add
			{
				//AddItem(listViewVertical, itemVPrefab, serverMessage);
			}
		}

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


		if(mListMsg.Count > 0)
		{
            RemoveItemAll(listViewVertical);
            for (int i=0; i < mListMsg.Count; i++)
            {
                Debug.Log(mListMsg[i]);
                AddItem(listViewVertical, itemVPrefab, mListMsg[i]);

			}
			//foreach (string myStringList in mListMsg)  //error bug.
			//{
			//	Debug.Log(myStringList);
			//	AddItem(listViewVertical, itemVPrefab, myStringList);
			//}
			mListMsg.Clear();
		}

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


	}

	private void OnTick(object source, ElapsedEventArgs e)
	{
		//print(e.SignalTime);
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

	void ToggleValueChanged(Toggle change)
	{
		//m_Text.text = "Toggle is : " + m_Toggle.isOn;
		//Debug.Log("Toggle is : " + change.isOn);
		Debug.Log("Toggle is : " + change.isOn);
		if(change.isOn == true)
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


	public void SendButton(int index)
	{
		Debug.Log("SendButton= " + index);
		if (!_client.IsConnected)
		{
			string clientMessage = "SocketConnection is diasble!!!";
			AddItem(listViewVertical, itemVPrefab, clientMessage);
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
			AddItem(listViewVertical, itemVPrefab, clientMessage);
		}


	}

	//***************************************************************************************

	public void ConnectClient()
	{
		if (!_client.IsConnected)
		{
			Debug.Log("ConnectClient()....");
			_client.IPAddress = tbx_IpAddr.text;
			int.TryParse(tbx_Port.text, out _client.Port);
			_client.ConnectToTcpServer();
		}
	}

	public void DisconnectClient()
	{
		if (_client.IsConnected)
		{
			Debug.Log("DisconnectClient()....");
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

	private void OnClientReceivedMessage_old(string message)
	{
		string finalMessage = message;
		Debug.Log("OnClientLog: " + message);
		lock (cacheLock)
		{
			cache = string.Format("<color=red>{0}</color>\n", finalMessage);
			mListMsg.Add(cache);
		}
	}


	private void OnClientReceivedMessage(string message)
	{
		string finalMessage = message;
		Debug.Log("OnClientLog: " + message);
		int flag = 0;


		//CSV 解碼
		var regex = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
		var matches = regex.Matches(finalMessage);
		int csv_total_fields = matches.Count;
		string[] ack_data = new string[24];
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

		Debug.Log("tag_id = " + tag_id);

        //item1 = ListView_Test.Find(tag_id);

        //List<string> test = new List<string>();
        //test.Add(tag_id);
        //test.Add(finalMessage);
        //ListView_Test.Add(test);

        //List<string> device_item = new List<string>();    //Tag ID


        //item1.Sort();
        Debug.Log("item1.Count = " + item1.Count);

		//////if (item1.Contains(tag_id))
  //////      {
		//////	Debug.Log("Contains what justAString is set to: " + tag_id);
		//////}
		//////else
  //////      {
		//////	Debug.Log("Contains what justAString is NULL: " + tag_id);
		//////	item1.Add(tag_id);
		//////	item1.Sort();
		//////	for (int i = 0; i < item1.Count; i++)
		//////	{
		//////		Debug.Log(item1[i]);
		//////		cache = string.Format("<color=red>{0}</color>\n", item1[i]);
		//////		mListMsg.Add(cache);
		//////	}

		//////	//cache = string.Format("<color=red>{0}</color>\n", tag_id);
		//////	//mListMsg.Add(cache);
		//////}

		tempIndex = tagActiveReportStatusList.FindIndex(z => z.TagID == tag_id);
		Debug.Log("tempIndex... : " + tempIndex);

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

			cache = string.Format("<color=red>{0}  |  {1} </color>\n", tagActiveReportStatus.TagID, tagActiveReportStatus.Counts);
			mListMsg.Add(cache);
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

            //tagActiveReportStatusList.Sort();
            for (int i = 0; i < tagActiveReportStatusList.Count; i++)
			{
				Debug.Log(tagActiveReportStatusList[i].TagID);
				cache = string.Format("<color=red>{0}  |  {1} </color>\n", tagActiveReportStatusList[i].TagID, tagActiveReportStatusList[i].Counts);
				mListMsg.Add(cache);
			}

			Debug.Log("tagActiveReportStatusList[tempIndex].Counts  = " + tagActiveReportStatusList[tempIndex].Counts);
			Debug.Log("tagActiveReportStatusList.Count = " + tagActiveReportStatusList.Count);


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
		Debug.Log(startTime);
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
			mListMsg.Add(cache);
		}
	}


	private void AddItem(ListView lv, DemoItem prefab, string msg)
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
			r = UnityEngine.Random.Range(0.3f, 1.0f),
            g = UnityEngine.Random.Range(0.3f, 1.0f),
            b = UnityEngine.Random.Range(0.3f, 1.0f),
            a = 0.5f,
        };
		Debug.Log("AddItem msg: " + msg);
		var item = Instantiate(prefab);

		if (msg == "")
		{
            item.SetContent(color.ToString(), color);
        }
		else
        {
			item.SetContent(msg, color);
		}


		lv.AddItem(item.gameObject);
        Debug.Log("lv.ItemCount= " + lv.ItemCount);

		//prefab.gameObject.GetComponent<Button>().onClick.AddListener(listViewOnClickTest);
    }

	public void listViewOnClick(Text msg)
    {
		Debug.Log("listViewOnClick: " + msg.text);
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
