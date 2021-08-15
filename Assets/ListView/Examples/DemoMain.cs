using System;
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

public class DemoMain : MonoBehaviour
{
	public Action<DemoMain> OnConnected = delegate { };
	public Action<DemoMain> OnDisconnected = delegate { };
	public Action<string> OnLog = delegate { };

	public ListView listViewVertical;
    public ListView listViewHorizontal;
    public DemoItem itemVPrefab;
    public DemoItem itemHPrefab;

	public InputField[] tbx_Txt;

	public InputField tbx_IpAddr;
	public InputField tbx_Port;

	#region private members
	private TcpClient socketConnection;
    private Thread clientReceiveThread;
	#endregion
	private string serverMessage;
	private bool running;
	private static System.Timers.Timer aTimer;

	public Toggle m_ToggleConnect;

    List<string> mListMsg = new List<string>();


	private object cacheLock = new object();
	private string cache;

	private void Awake()
	{
		Debug.Log("Awake()");
		OnLog += OnClientLog;
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
			foreach (string myStringList in mListMsg)
			{
				Debug.Log(myStringList);
				AddItem(listViewVertical, itemVPrefab, myStringList);
			}
			mListMsg.Clear();
		}

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


	void OnDisable()
	{
		aTimer.Dispose();
		print(aTimer == null);
	}

	public void ConnectButton()
	{
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
		Debug.Log("ItemCount: " + listViewVertical.ItemCount);
		if (listViewVertical.ItemCount > 0)
        {
			RemoveItemAll(listViewVertical);
		}

		ConnectToTcpServer();
	}

	void ToggleValueChanged(Toggle change)
	{
		//m_Text.text = "Toggle is : " + m_Toggle.isOn;
		//Debug.Log("Toggle is : " + change.isOn);
		Debug.Log("Toggle is : " + change.isOn);
		if(change.isOn == true)
		{
			if (socketConnection == null)
			{
				Debug.Log("ConnectButton();");
				ConnectButton();
			}
		}
		else
		{
			if (socketConnection != null)
			{
				Debug.Log("socketConnection.Close();");
				socketConnection.Close();
				clientReceiveThread.Abort();
				string clientMessage = "Socket Connection is diasble!!!";
				AddItem(listViewVertical, itemVPrefab, clientMessage);
			}
			//DisConnectButton();
		}

	}

	//public void SendMessageButton()
	//{
	//	//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
	//	SendMessage();
	//}

	public void SendButton(int index)
	{
		Debug.Log("SendButton= " + index);
		if (socketConnection == null)
		{
			string clientMessage = "SocketConnection is diasble!!!";
			AddItem(listViewVertical, itemVPrefab, clientMessage);
			return;
		}
		try
		{
			// Get a stream object for writing.
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				//string clientMessage = "This is a message from one of your clients.";
				string clientMessage = tbx_Txt[index].text;
				// Convert string message to byte array.
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server: " + clientMessage);
				//AddItem(listViewVertical, itemVPrefab, clientMessage);
				OnLog(clientMessage);
			}
		}
		catch (SocketException socketException)
		{
			string clientMessage = "Socket Connection is fail !!!";
			Debug.Log("Socket exception: " + socketException);
			AddItem(listViewVertical, itemVPrefab, clientMessage);
		}


	}

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

}
