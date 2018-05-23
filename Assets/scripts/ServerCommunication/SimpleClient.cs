using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ClientSockets
{
    public class SimpleClient
    {
        private const string StartKey = "|START|";
        private readonly int StartKeyLength = StartKey.Length;
        private const string EndKey = "|END|";
        private readonly int EndKeyLength = EndKey.Length;

        private const int READ_BUFFER_SIZE = 4096;
        private byte[] m_ReadBuffer = new byte[READ_BUFFER_SIZE];
        private TcpClient m_Client;
        private readonly StringBuilder m_StringBuilder = new StringBuilder();
        private string m_IpAddress;
        private int m_PortNumber;
        private bool m_IsAttemptingConnect;
        private readonly object m_InitializeLock;
        private bool m_HasDisconnected;

        private event Action<string> m_ServerCallback = null;

        public SimpleClient(string ipAddress, int portNumber, Action<string> callback)
        {
            m_ServerCallback = callback;
            m_PortNumber = portNumber;
            m_IpAddress = ipAddress;
            m_InitializeLock = new object();
        }

        public void Connect()
        {
            if (isConnectedToServer)
            {
                return;
            }
            lock(m_InitializeLock)
            {
                if(m_IsAttemptingConnect)
                {
                    return;
                }
            }
            Debug.Log("STARTING CONNECT");
            System.Threading.ThreadPool.QueueUserWorkItem((a) =>
            {
                lock (m_InitializeLock)
                {
                    m_IsAttemptingConnect = true;
                    try
                    {
                        m_Client = new TcpClient(m_IpAddress, m_PortNumber);
                        m_Client.SendTimeout = 2;
                        // Start an asynchronous read invoking DoRead to avoid lagging the user interface.
                        m_Client.GetStream().BeginRead(m_ReadBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
                        Debug.LogWarning("Connection successful to ip " + m_IpAddress + ":" + m_PortNumber);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                    m_IsAttemptingConnect = false;
                }
            });
        }

        public void Disconnect()
        {
			if(m_HasDisconnected)
			{
				return;
			}
            m_HasDisconnected = true;
            try
            {
                m_Client.Close();
            }
            catch (Exception e)
            {
                Debug.LogError("Error closing connection\n" + e);
                Debug.LogException(e);
            }
        }

        public void ReConnect()
        {
            Debug.Log("RECONNECT");
            if(isConnectedToServer)
            {
                m_Client.Close();
            }
            m_Client = null;
            Connect();
        }

        public bool isConnectedToServer
        {
            get
            {
				return !m_HasDisconnected && m_Client != null && m_Client.Connected;
            }
        }

        private void DoRead(IAsyncResult ar)
        {
			if (!isConnectedToServer)
            {
                return;
            }
            try
            {
                // Finish asynchronous read into readBuffer and return number of bytes read.
                int bytesRead = m_Client.GetStream().EndRead(ar);
                if (bytesRead < 1)
                {
                    // if no bytes were read server has closed.
                    return;
                }
                // Convert the byte array the message was saved into
                string message = Encoding.ASCII.GetString(m_ReadBuffer, 0, bytesRead);
                m_StringBuilder.Append(message);
                ProcessCommands();
				if(!isConnectedToServer)
				{
					return;
				}
                // Start a new asynchronous read into readBuffer.
                m_Client.GetStream().BeginRead(m_ReadBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
            }
            catch(Exception e)
            {
                Debug.LogWarning("Caught exception in do read " + e.Message);
                Debug.LogException(e);
            }
        }

        // Process the CommandHandler received from the server, and send it back to listener.
        private void ProcessCommands()
        {
            while (m_StringBuilder.Length > 0)
            {
                string currentString = m_StringBuilder.ToString();
                if (string.IsNullOrEmpty(currentString))
                {
                    return;
                }
                int startIndex = currentString.IndexOf(StartKey);
                if (startIndex < 0)
                {
                    return;
                }
                int lengthIndex = currentString.IndexOf("|", startIndex + StartKeyLength);
                if (lengthIndex < 0)
                {
                    return;
                }
                string lengthString = currentString.Substring(startIndex + StartKeyLength, lengthIndex - startIndex - StartKeyLength);
                int length;
                if (!int.TryParse(lengthString, out length))
                {
                    return;
                }
                //dont have the whole message yet
                if (currentString.Length < lengthIndex + 1 + length + EndKeyLength)
                {
                    return;
                }
                string actualEndKey = currentString.Substring(lengthIndex + 1 + length, EndKeyLength);
                if (!string.Equals(actualEndKey, EndKey))
                {
                    return;
                }
                string message = currentString.Substring(lengthIndex + 1, length);
                if (m_ServerCallback != null)
                {
                    m_ServerCallback(message);
                }
                //always remove from the beginning, we will never gain the beginning of a message but we will get the end later
                m_StringBuilder.Remove(0, lengthIndex + 1 + length + EndKeyLength);
            }
        }

        // Use a StreamWriter to send a message to server.
        public void SendData(string data)
        {
            if(!isConnectedToServer)
            {
                Debug.LogError("Can not send data because the client is not currently connected");
                return;
            }
            StreamWriter writer = new StreamWriter(m_Client.GetStream());
            writer.Write(data);
            writer.Flush();
        }
    }
}