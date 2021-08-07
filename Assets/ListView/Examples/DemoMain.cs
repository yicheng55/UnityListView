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

public class DemoMain : MonoBehaviour
{
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


	// Start is called before the first frame update
	void Start()
	{
		tbx_IpAddr.text = "127.0.0.1";
		tbx_Port.text = "21087";
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

		if (serverMessage != null)
		{
			AddItem(listViewVertical, itemVPrefab, serverMessage);
			serverMessage = null;
		}
	}

	public void ConnectButton()
	{
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
		ConnectToTcpServer();
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
				AddItem(listViewVertical, itemVPrefab, clientMessage);
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}


	}
	private void AddItem(ListView lv, DemoItem prefab, string msg)
    {
        var color = new Color()
        {
            r = UnityEngine.Random.Range(0.0f, 1.0f),
            g = UnityEngine.Random.Range(0.0f, 1.0f),
            b = UnityEngine.Random.Range(0.0f, 1.0f),
            a = 1.0f,
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
        //lv.RemoveBottom();
        lv.RemoveTop();
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
			Debug.Log("On client connect exception " + e);
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
            Byte[] bytes = new Byte[1024];
			while (true)
			{
				// Get a stream object for reading
				using (NetworkStream stream = socketConnection.GetStream())
				{
					int length;
					// Read incomming stream into byte arrary.
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incommingData = new byte[length];
						Array.Copy(bytes, 0, incommingData, 0, length);
						// Convert byte array to string message.
						serverMessage = Encoding.ASCII.GetString(incommingData);
						Debug.Log("server message received as: " + serverMessage);

					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
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
				//textView.text = clientMessage;
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

}
