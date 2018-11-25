using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.ComponentModel;
using System.Net.Sockets;

using Ressive.Utils;

namespace Ressive.FTP
{
	
    public class RscFtpCommandEventArgs : EventArgs
    {
		
		public RscFtpClientCommand ClientCommand;
		public RscFtpServerResponse ServerResponse;
		public RscFtpServerData ServerData;
		
		public string Message;
		
		public int ProgressMax;
		public int ProgressPos;
		
        public RscFtpCommandEventArgs()
        {
			ClientCommand = null;
			ServerResponse = null;
			ServerData = null;
			
			Message = "";
			
			ProgressMax = 0;
			ProgressPos = 0;
        }
		
    }
	
	public class RscFtpClient
	{
		
		public RscFtpStat StatUpCmd = new RscFtpStat();
		public RscFtpStat StatUpDat = new RscFtpStat();
		public RscFtpStat StatDnCmd = new RscFtpStat();
		public RscFtpStat StatDnDat = new RscFtpStat();
		
		public const string CommandDelimiter = "\r\n";
		
		public int m_iServerResponseBufferSize_INF = (4 * 1024);
		public int m_iServerResponseBufferSize_DAT = (65 * 1024);
		public void SetFastConnection( bool bLevel1, bool bLevel2 )
		{
			if( bLevel1 && bLevel2 )
			{
				m_iServerResponseBufferSize_INF = (4 * 1024);
				m_iServerResponseBufferSize_DAT = (5 * 1024 * 1024);
			}
			else if( bLevel1 || bLevel2 )
			{
				m_iServerResponseBufferSize_INF = (4 * 1024);
				m_iServerResponseBufferSize_DAT = (1 * 1024 * 1024);
			}
			else
			{
				m_iServerResponseBufferSize_INF = (4 * 1024);
				m_iServerResponseBufferSize_DAT = (65 * 1024);
			}
			
			//BUGFIX: unkLISTresLENfix
			int iFix = RscRegistry.ReadDWORD( HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscLib\\RscFpt", "unkLISTresLENfix", 0 );
			m_iServerResponseBufferSize_INF += iFix;
		}
		
		public string IPAddress = "";
		public int Port = 0;
		public string UserName = "";
		public string PassWord = "";
		
		public bool AutoLogOn = true;
		public bool AutoPassiveMode = true;
		
		System.Net.Sockets.Socket m_Socket21 = null;
		System.Net.Sockets.Socket m_Socket20 = null;
		
		public string BackSlashInPath = "?";
		public string WorkingDirectory = "";
		public string ServerTitle = "";
		
		public bool UTF8 = true;
		
		public delegate void CommandSocketConnectedAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event CommandSocketConnectedAsync_EventHandler CommandSocketConnectedAsync;
		
		public delegate void ServerResponseAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event ServerResponseAsync_EventHandler ServerResponseAsync;
		
		public delegate void CommandSentAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event CommandSentAsync_EventHandler CommandSentAsync;
		
		public delegate void DataSocketConnectedAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event DataSocketConnectedAsync_EventHandler DataSocketConnectedAsync;
		
		public delegate void ServerDataReceivedAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event ServerDataReceivedAsync_EventHandler ServerDataReceivedAsync;
		
		public delegate void ServerDataSentAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event ServerDataSentAsync_EventHandler ServerDataSentAsync;
		
		public delegate void DataSocketClosingAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event DataSocketClosingAsync_EventHandler DataSocketClosingAsync;
		
		public delegate void CommandDoneAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event CommandDoneAsync_EventHandler CommandDoneAsync;
		
		public delegate void LogAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event LogAsync_EventHandler LogAsync;
		
		public delegate void ProgressAsync_EventHandler(object sender, RscFtpCommandEventArgs e);
		public event ProgressAsync_EventHandler ProgressAsync;
		
		public RscFtpClient()
		{
			//FIX!!! - Download / Upload DATA time included int CMD Download!!!
			StatUpDat.SetToReStartTo( StatDnCmd );
			StatDnDat.SetToReStartTo( StatDnCmd );
			//FIX!!! - Download / Upload DATA time included int CMD Download!!!
		}
		
		public long BytesPerSecUp
		{
			get
			{
				double dSec = StatUpCmd.Seconds + StatUpDat.Seconds;
				if( dSec == 0 ) return 0;
				
				double dSiz = ((double) StatUpCmd.ByteCount) + ((double) StatUpDat.ByteCount);
				
				double dRes = dSiz / dSec;
				dRes = Math.Round( dRes, 0 );
				
				return ((long) dRes);
			}
		}
		
		public long BytesPerSecDn
		{
			get
			{
				double dSec = StatDnCmd.Seconds + StatDnDat.Seconds;
				if( dSec == 0 ) return 0;
				
				double dSiz = ((double) StatDnCmd.ByteCount) + ((double) StatDnDat.ByteCount);
				
				double dRes = dSiz / dSec;
				dRes = Math.Round( dRes, 0 );
				
				return ((long) dRes);
			}
		}
		
		public void Connect()
		{
			
			//BUGFIX: unkLISTresLENfix
			int iFix = RscRegistry.ReadDWORD( HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscLib\\RscFpt", "unkLISTresLENfix", 0 );
			m_iServerResponseBufferSize_INF += iFix;
		
			StatUpCmd.Clear();
			StatUpDat.Clear();
			StatDnCmd.Clear();
			StatDnDat.Clear();
			
			m_Socket21 = new System.Net.Sockets.Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			
			SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
			
			//WP8
			//socketEventArg.RemoteEndPoint = new DnsEndPoint(IPAddress, Port);
			socketEventArg.RemoteEndPoint = new System.Net.DnsEndPoint(IPAddress, Port);
			
			socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
				SocketEventArg21_Connected);
			socketEventArg.UserToken = null;
			
			m_Socket21.ConnectAsync(socketEventArg);
		}
		
		private void SocketEventArg21_Connected(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
		{
			RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
			
			newe.ClientCommand = null; //No Command while connecting...
			
			if( CommandSocketConnectedAsync != null )
				CommandSocketConnectedAsync(this, newe);
		}
		
		public void ReadServerResponse( RscFtpClientCommand cmd )
		{
			SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
			socketEventArg.RemoteEndPoint = m_Socket21.RemoteEndPoint;
			socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
				SocketEventArg21_Response);
			socketEventArg.UserToken = cmd;
			
			byte[] buff = new byte[m_iServerResponseBufferSize_INF];
			socketEventArg.SetBuffer(buff, 0, buff.Length);
			
			StatDnCmd.Start();
			m_Socket21.ReceiveAsync(socketEventArg);			
		}
		
		private void SocketEventArg21_Response(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
		{
			StatDnCmd.End( (long) e.BytesTransferred );
			
			if( e.BytesTransferred > 0 )
			{
			
				RscFtpClientCommand cmd = null;
				if( e.UserToken != null ) cmd = (RscFtpClientCommand) e.UserToken;

				string sRes;
				//BugFix... ...to work with fixed CsFtp... ...test with Mocha.WM65!!!
				//sRes = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
				if( UTF8 )
					sRes = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
				else
					sRes = RscEncode.AsciiToString(e.Buffer, e.Offset, e.BytesTransferred);
				
				//More response can arrive at once!!!
				int iStart = 0;
				int iPos;
				string sPart;
				int iCnt = 0;
				RscFtpServerResponse respSupressed = null;
				while( true )
				{
					iPos = sRes.IndexOf( RscFtpClient.CommandDelimiter, iStart );
					if( iPos < 0 ) iPos = sRes.Length;
					
					sPart = sRes.Substring( iStart, iPos - iStart );
					if( sPart.Length > 0 )
					{
						iCnt++;
						
						RscFtpServerResponse resp = new RscFtpServerResponse( cmd, sPart );
						
						if( cmd != null) cmd.ResponseCountArrived++;
						
						bool bSupress = false;
						if( resp.Code == 200 )
						{
							if( cmd != null )
							{
								bSupress = cmd.SupressOk200;
							}
						}
						
						if( bSupress )
						{
							//Supressing response...
							respSupressed = resp;
						}
						else
						{
							RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
			
							newe.ClientCommand = cmd;
							newe.ServerResponse = resp;
							
							if( ServerResponseAsync != null )
								ServerResponseAsync(this, newe);
						}
					}
					
					iStart = iPos + RscFtpClient.CommandDelimiter.Length;
					if( iStart >= sRes.Length ) break;
				}
				
				if( (respSupressed != null) && (iCnt == 1) )
				{
					//ServerResponseAsync was not called!!!
					RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
					
					newe.ClientCommand = cmd;
					newe.ServerResponse = respSupressed;
					
					if( CommandDoneAsync != null )
						CommandDoneAsync(this, newe);
				}
				else
				{
					if( cmd != null )
					{
						if( cmd.ResponseCountArrived < cmd.ResponseCount )
						{
							_Log( cmd,  "Trying to read response " + (cmd.ResponseCountArrived + 1).ToString() +
								" / " + cmd.ResponseCount.ToString() + "." );
							
							ReadServerResponse( cmd );
						}
					}
				}
			}
		}
		
		private void _Log( RscFtpClientCommand cmd, string sMsg )
		{
			RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
			
			newe.ClientCommand = cmd;
			
			newe.Message = sMsg;
			
			if( LogAsync != null )
				LogAsync(this, newe);
		}
		
		public void SendCommandToServer(RscFtpClientCommand ClientCommand)
		{
			SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
			socketEventArg.RemoteEndPoint = m_Socket21.RemoteEndPoint;
			socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
				SocketEventArg21_Sent);
			socketEventArg.UserToken = ClientCommand;
			
			string sCmd = ClientCommand.ToString( );
			sCmd += RscFtpClient.CommandDelimiter;
			
			Byte[] ayCmd;
			if( UTF8 )
				ayCmd = Encoding.UTF8.GetBytes(sCmd.ToCharArray());
			else
				ayCmd = RscEncode.StringToAscii(sCmd);
			
			socketEventArg.SetBuffer(ayCmd, 0, ayCmd.Length);
			
			StatUpCmd.Start();
			m_Socket21.SendAsync(socketEventArg);
		}
		
		private void SocketEventArg21_Sent(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
		{
			StatUpCmd.End( (long) e.BytesTransferred );
			
			RscFtpClientCommand cmd = null;
			if( e.UserToken != null ) cmd = (RscFtpClientCommand) e.UserToken;
			
			RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
			
			newe.ClientCommand = cmd;
			
			if( CommandSentAsync != null )
				CommandSentAsync(this, newe);
		}
		
		public void DispatchResponse( RscFtpClientCommand ClientCommand, RscFtpServerResponse ServerResponse )
		{
			bool bNewCmdSent = false;
			
			if( ServerResponse.DataPortSettings )
			{
				OpenServerDataPort( ClientCommand, ServerResponse.ServerIP, ServerResponse.ServerPort );
			}
			else if( ServerResponse.ServerSendingData )
			{
				ReceiveServerData( ClientCommand );
			}
			else if( ServerResponse.ServerReceivingData )
			{
				SendServerData( ClientCommand );
			}
			else
			{
				if( ServerResponse.CanGoOn )
				{
					if( ClientCommand == null ) //Connection...
					{
						//Auto LogOn - UserName...
						if( ServerResponse.Code == 220 )
						{
							if( AutoLogOn && (UserName.Length > 0) )
							{
								bNewCmdSent = true;
								
								RscFtpClientCommand cmdLogOn = RscFtpClientCommand.LogInUser(UserName);
								
								//On some server, below settings must be perceded by successful LogOn!!!
								
								RscFtpClientCommand cmdOptsUtf8On = new RscFtpClientCommand(1, "OPTS", "UTF8 ON");
								cmdLogOn.Parent = cmdOptsUtf8On;
								
								RscFtpClientCommand cmdTypeI = new RscFtpClientCommand(1, "TYPE", "I");
								cmdOptsUtf8On.Parent = cmdTypeI;
								
								RscFtpClientCommand cmdSyst = new RscFtpClientCommand(1, "SYST");
								cmdTypeI.Parent = cmdSyst;
								
								SendCommandToServer( cmdLogOn );
							}
						}
					}
					else
					{
						switch( ClientCommand.Command )
						{
							
							case "OPTS" :
							{
								if( ClientCommand.Arg1 == "UTF8 ON" )
								{
									UTF8 = (ServerResponse.Code == 200 || ServerResponse.Code == 202);
								}
								break;
							}
							
							case "SYST" :
							{
								ServerTitle = ServerResponse.Message;
								break;
							}
							
							case "USER" :
							{
								if( ServerResponse.Code == 331 )
								{
									if( AutoLogOn )
									{
										bNewCmdSent = true;
										
										RscFtpClientCommand cmd = RscFtpClientCommand.LogInPassWord(PassWord);
										
										//To support nested commands...
										cmd.Parent = ClientCommand.Parent;
										
										SendCommandToServer( cmd );
									}
								}
								break;
							}
							
							case "PASS" :
							{
								if( ServerResponse.Code == 220 || ServerResponse.Code == 230 )
								{
									if( AutoLogOn )
									{
										bNewCmdSent = true;
										
										RscFtpClientCommand cmdPwd = new RscFtpClientCommand(1, "PWD");
										
										if( ClientCommand.Parent == null )
										{
											SendCommandToServer( cmdPwd );
										}
										else
										{
											//To support nested commands...
											
											//ATT: PWD can be affected by UTF8 setting!!!
											RscFtpClientCommand cmd = ClientCommand.Parent;
											for(;;)
											{
												if( cmd.Parent == null ) break;
												cmd = cmd.Parent;
											}
											
											cmd.Parent = cmdPwd;
											
											SendCommandToServer( ClientCommand.Parent );
										}
									}
								}
								break;
							}
							
							case "PWD" :
							{
								if( ServerResponse.Code == 257 )
								{
									string sPath = ServerResponse.Message;
									
									//MUST NOT!!!
									/*
									//FIX (for PC|CSFtp(David McClarnon, dmcclarnon@ntlworld.com))
									sPath = sPath.Replace("\\", RscFtpClient.BackSlashInPath);
									sPath = sPath.Replace("/", RscFtpClient.BackSlashInPath);
									*/
									
									//First call...
									//if( WorkingDirectory.Length == 0 )
									if( BackSlashInPath == "?" )
									{
										if( sPath.IndexOf('\\') >= 0 )
											BackSlashInPath = "\\";
										else if( sPath.IndexOf('/') >= 0 )
											BackSlashInPath = "/";
										//else, unexpected...
									}
									
									WorkingDirectory = sPath;
								}
								break;
							}
							
							case "CWD" :
							{
								if( ServerResponse.Code == 250 )
								{
									bNewCmdSent = true;
									
									RscFtpClientCommand cmd = new RscFtpClientCommand(1, "PWD");
									
									//To support nested commands...
									cmd.Parent = ClientCommand.Parent;
									
									SendCommandToServer( cmd );
								}
								break;
							}
							
						}
					}
				}
			}
				
			if( ServerResponse.CanGoOn && (!bNewCmdSent) && (ClientCommand != null) )
			{
				switch( ClientCommand.Command )
				{
					
					case "PASV" :
					case "LIST" :
					case "RETR" :
						//MUST NOT report as DONE!
						//Data Socket communication is in progress!!!
						break;
						
					case "STOR" :
						//Server sends ack after DataSocket has been closed, so handle here!
						//see: _CloseDataSocket
					default :
					{
						if( ClientCommand.Parent != null )
						{
							//Introduced for MKD+PASV(LIST) situation for RscFtpBackUpV11...
							SendCommandToServer( ClientCommand.Parent );
						}
						else
						{
							RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
							
							newe.ClientCommand = ClientCommand;
							newe.ServerResponse = ServerResponse;
							
							if( CommandDoneAsync != null )
								CommandDoneAsync(this, newe);
						}
						
						break;
					}
					
				}
			}
		}
		
		private void OpenServerDataPort( RscFtpClientCommand ClientCommand, string strIP, int iPort )
		{
			if( m_Socket20 != null ) return;
			
			m_Socket20 = new System.Net.Sockets.Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			
			SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
			
			//WP8
			//socketEventArg.RemoteEndPoint = new DnsEndPoint(strIP, iPort);
			socketEventArg.RemoteEndPoint = new System.Net.DnsEndPoint(strIP, iPort);
			
			socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
				SocketEventArg20_Connected);
			socketEventArg.UserToken = ClientCommand;
			
			m_Socket20.ConnectAsync(socketEventArg);
		}
		
		private void SocketEventArg20_Connected(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
		{
			RscFtpClientCommand cmd = null;
			if( e.UserToken != null ) cmd = (RscFtpClientCommand) e.UserToken;
			
			RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
			
			newe.ClientCommand = cmd;
			
			if( DataSocketConnectedAsync != null )
				DataSocketConnectedAsync(this, newe);
			
			if( AutoPassiveMode )
			{
				if( cmd.Parent != null )
				{
					
					/*
					 * If PASV automatically percede a command
					 * let's send that command now!!!
					 */
					
					SendCommandToServer( cmd.Parent );
				}
			}
		}
		
		private void ReceiveServerData( RscFtpClientCommand ClientCommand )
		{
			if( m_Socket20 == null ) return;
			
			//MUST NOT!!!
			/*
			if( ClientCommand.Command == "RETR" )
			{
				//Zero len file download...
				if( ClientCommand.DataSize == 0 )
				{
					byte[] buffEmpty = new byte[1];
					SocketEventArg20_Received_Ex( ClientCommand, buffEmpty, 0, 0 );
					return;
				}
			}
			*/
			
			SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
			socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
			socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
				SocketEventArg20_Received);
			socketEventArg.UserToken = ClientCommand;
			
			int iSize;
			switch( ClientCommand.Command )
			{
				
				case "LIST" :
					iSize = m_iServerResponseBufferSize_INF;
					break;
					
				case "RETR" :
					iSize = m_iServerResponseBufferSize_DAT;
					if( ClientCommand.DataSize < iSize )
					{
						iSize = (int) ClientCommand.DataSize;
					}
					break;
					
				default :
					iSize = m_iServerResponseBufferSize_INF;
					break;
					
			}
			
			//_Log( ClientCommand, "Downloading... (PocketSize=" + iSize.ToString() + ")");
			
			byte[] buff = new byte[iSize];
			socketEventArg.SetBuffer(buff, 0, buff.Length);
			
			switch( ClientCommand.Command )
			{
				case "LIST" :
					StatDnCmd.Start();
					break;
				default :
					StatDnDat.Start();
					break;
			}
			m_Socket20.ReceiveAsync(socketEventArg);			
		}
		
		private void SocketEventArg20_Received(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
		{
			RscFtpClientCommand cmd = (RscFtpClientCommand) e.UserToken;
			
			switch( cmd.Command )
			{
				case "LIST" :
					StatDnCmd.End( (long) e.BytesTransferred );
					break;	
				default :
					StatDnDat.End( (long) e.BytesTransferred );
					break;
			}
			
			//_Log(cmd, "20 Received: " + e.BytesTransferred.ToString() + " / " + e.Count.ToString() );
			
			SocketEventArg20_Received_Ex( e.UserToken, e.Buffer, e.Offset, e.BytesTransferred );
		}
		
		private void SocketEventArg20_Received_Ex(object UserToken, byte [] Buffer, int Offset, int BytesTransferred)
		{
			//LIST can read Zero Bytes!!!
			//if( e.BytesTransferred > 0 )
			{	
				RscFtpClientCommand cmd = (RscFtpClientCommand) UserToken;
				RscFtpServerData sd = new RscFtpServerData( cmd );
				
				switch(cmd.Command )
				{
					
					case "LIST" :
					{
						
						string sRes;
						
						if( cmd.MemStream == null && BytesTransferred == 0 )
						{
							//Simulate zero item list with terminating CRLF!!!
							sRes = CommandDelimiter;
						}
						else
						{
							if( cmd.MemStream == null )
							{
								cmd.MemStream = new System.IO.MemoryStream();
								if( BytesTransferred > 0 )
									cmd.MemStream.Write(Buffer, Offset, BytesTransferred);
							}
							else
							{
								if( BytesTransferred > 0 )
									cmd.MemStream.Write(Buffer, Offset, BytesTransferred);
							}
							
							int iMax = ((((int) cmd.MemStream.Length) / m_iServerResponseBufferSize_INF) + 1)
									* m_iServerResponseBufferSize_INF;
							bool bHitEOF = false;
							
							if( cmd.DataSize > 0 )
							{
								iMax = (int) cmd.DataSize;
								
								//Case of known data size from LIST's response with code 150 (Mocha FTP Server - WinMo)
								if( cmd.MemStream.Length >= cmd.DataSize )
									bHitEOF = true;
							}
							else
							{								
								cmd.MemStream.Seek(-2, System.IO.SeekOrigin.Current);
								int iB0 = cmd.MemStream.ReadByte();
								int iB1 = cmd.MemStream.ReadByte();
								_Log(cmd, "LIST EofTest: " + iB0.ToString() + ", " + iB1.ToString() );
								if( (iB0 == ((int) (CommandDelimiter[0]))) && (iB1 == ((int) (CommandDelimiter[1]))) )
									bHitEOF = true;
							}
							
							if( !bHitEOF )
							{
								RscFtpCommandEventArgs newep = new RscFtpCommandEventArgs();
								newep.ProgressMax = iMax;
								newep.ProgressPos = (int) cmd.MemStream.Length;
								if( ProgressAsync != null )
									ProgressAsync(this, newep);
								
								SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
								socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
								socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
									SocketEventArg20_Received);
								socketEventArg.UserToken = cmd;
								
								byte[] buff = new byte[m_iServerResponseBufferSize_INF];
								socketEventArg.SetBuffer(buff, 0, buff.Length);
								
								StatDnCmd.Start();
								m_Socket20.ReceiveAsync(socketEventArg);
								
								return;
							}
							
							/*
								BUG: Possible data loss!!!
								If part of LIST result ends with CRLF,
								part of LIST is treated as whole LIST result.
								DATA LOSS!!!
							*/
							if( cmd.DataSize <= 0 )
							{
								if( cmd.MemStream.Length == m_iServerResponseBufferSize_INF )
								{
									_Log( cmd, "unkLISTresLEN" );
			
									//BUGFIX: unkLISTresLENfix
									int iFix = RscRegistry.ReadDWORD( HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscLib\\RscFpt", "unkLISTresLENfix", 0 );
									iFix++;
									if( iFix > 7 ) iFix = 0;
									RscRegistry.WriteDWORD( HKEY.HKEY_CURRENT_USER, "Software\\Ressive.Hu\\RscLib\\RscFpt", "unkLISTresLENfix", iFix );
								}
							}
							
							byte [] ay = cmd.MemStream.ToArray();
							if( UTF8 )
								sRes = Encoding.UTF8.GetString(ay, 0, ay.Length);
							else
								sRes = RscEncode.AsciiToString(ay, 0, ay.Length);
						
							_Log(cmd, "20 Received LIST DataSize: " + cmd.DataSize.ToString() );
							_Log(cmd, "20 Received LIST res len: " + ay.Length.ToString() );
						}
						
						sd.Parse( sRes, BackSlashInPath );
						
						RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
						
						newe.ClientCommand = cmd;
						newe.ServerData = sd;
						
						if( ServerDataReceivedAsync != null )
							ServerDataReceivedAsync(this, newe);
						
						if( sd.DoCloseSocket )
						{
							CloseDataSocket( cmd, sd );
						}
						
						break;
					}
					
					case "RETR" :
					{
						if( cmd.HasFileStream )
						{
							cmd.FileStream.Write(Buffer, Offset, BytesTransferred);
							
							cmd.FileStream.Flush(); //WP81 FIX...
							
							cmd.StreamDone += ((long) BytesTransferred);
							
							if( cmd.StreamDone < cmd.DataSize )
							{
								RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
								newe.ProgressMax = (int) cmd.DataSize;
								newe.ProgressPos = (int) cmd.StreamDone;
								if( ProgressAsync != null )
									ProgressAsync(this, newe);
								
								SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
								socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
								socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
									SocketEventArg20_Received);
								socketEventArg.UserToken = cmd;
								
								int iSize = m_iServerResponseBufferSize_DAT;
								if( ((int) (cmd.DataSize - cmd.StreamDone)) < iSize )
									iSize = ((int) (cmd.DataSize - cmd.StreamDone));
								//_Log( cmd, "Downloading... (PocketSize=" + iSize.ToString() + ")");
								byte[] buff = new byte[iSize];
								socketEventArg.SetBuffer(buff, 0, buff.Length);
								
								StatDnDat.Start();
								m_Socket20.ReceiveAsync(socketEventArg);			
							}
							else
							{
								_Log(cmd, "File downloaded (FileStream)...");
								
								//ATT!!!
								cmd.FileStream.Close();
																
								sd.FileStream = cmd.FileStream;

								RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
								
								newe.ClientCommand = cmd;
								newe.ServerData = sd;
								
								if( ServerDataReceivedAsync != null )
									ServerDataReceivedAsync(this, newe);
								
								if( sd.DoCloseSocket )
								{
									CloseDataSocket( cmd, sd );
								}
							}
						}
						else
						{
							if( cmd.MemStream == null )
							{
								cmd.MemStream = new System.IO.MemoryStream((int) cmd.DataSize);
								if( BytesTransferred > 0 )
									cmd.MemStream.Write(Buffer, Offset, BytesTransferred);
							}
							else
							{
								if( BytesTransferred > 0 )
									cmd.MemStream.Write(Buffer, Offset, BytesTransferred);
							}
							
							if( cmd.MemStream.Length < cmd.DataSize )
							{
								RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
								newe.ProgressMax = (int) cmd.DataSize;
								newe.ProgressPos = (int) cmd.MemStream.Length;
								if( ProgressAsync != null )
									ProgressAsync(this, newe);
								
								SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
								socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
								socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
									SocketEventArg20_Received);
								socketEventArg.UserToken = cmd;
								
								int iSize = m_iServerResponseBufferSize_DAT;
								if( ((int) (cmd.DataSize - cmd.StreamDone)) < iSize )
									iSize = ((int) (cmd.DataSize - cmd.StreamDone));
								//_Log( cmd, "Downloading... (PocketSize=" + iSize.ToString() + ")");
								byte[] buff = new byte[iSize];
								socketEventArg.SetBuffer(buff, 0, buff.Length);
								
								StatDnDat.Start();
								m_Socket20.ReceiveAsync(socketEventArg);			
							}
							else
							{
								_Log(cmd, "File downloaded (MemStream)...");
								
								sd.MemStream = cmd.MemStream;
								
								RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
								
								newe.ClientCommand = cmd;
								newe.ServerData = sd;
								
								if( ServerDataReceivedAsync != null )
									ServerDataReceivedAsync(this, newe);
								
								if( sd.DoCloseSocket )
								{
									CloseDataSocket( cmd, sd );
								}
							}
						}
						
						break;
					}
					
				}
			}
		}
		
		public void SendServerData(RscFtpClientCommand ClientCommand)
		{
			switch( ClientCommand.Command )
			{
				
				//Sending TXT file...
				//... RscFtpClientCommand.UploadTxt
				case "STOR" :
				{
					if( ClientCommand.HasUserData )
					{
						SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
						socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
						socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
							SocketEventArg20_SentFIX);
							//SocketEventArg20_Sent);
						socketEventArg.UserToken = ClientCommand;
						
						//Byte[] ayMsg = Encoding.UTF8.GetBytes(ClientCommand.UserData.ToCharArray());
						Byte[] ayMsg = RscEncode.StringToAscii(ClientCommand.UserData);
						
						socketEventArg.SetBuffer(ayMsg, 0, ayMsg.Length);
						
						StatUpDat.Start();
						m_Socket20.SendAsync(socketEventArg);
					}
					else if( ClientCommand.HasMemStream || ClientCommand.HasFileStream )
					{
						
						SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
						socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
						socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
							SocketEventArg20_SentFIX);
							//SocketEventArg20_Sent);
						socketEventArg.UserToken = ClientCommand;
						
						ClientCommand.StreamDone = 0;
			
						if( ClientCommand.HasMemStream )
						{
							System.IO.MemoryStream ms;
							ms = ClientCommand.MemStream;
							
							int iSize = m_iServerResponseBufferSize_DAT;
							if( ((int) ms.Length) < iSize ) iSize = ((int) ms.Length);
							byte[] buff = new byte[iSize];
							ms.Read(buff, 0, iSize );
							
							//_Log(ClientCommand, "Sending MEM " + iSize.ToString() + " byte(s)..." );
							socketEventArg.SetBuffer(buff, 0, iSize);
							
							StatUpDat.Start();
							m_Socket20.SendAsync(socketEventArg);
						}
						else if( ClientCommand.HasFileStream )
						{
							System.IO.Stream fs;
							fs = ClientCommand.FileStream;
							
							int iSize = m_iServerResponseBufferSize_DAT;
							if( ((int) fs.Length) < iSize ) iSize = ((int) fs.Length);
							byte[] buff = new byte[iSize];
							fs.Read(buff, 0, iSize );
							
							//_Log(ClientCommand, "Sending FLE " + iSize.ToString() + " byte(s)..." );
							socketEventArg.SetBuffer(buff, 0, iSize);
							
							StatUpDat.Start();
							m_Socket20.SendAsync(socketEventArg);
						}
					}
					
					break;
				}
				
			}
		}
		
		private void SocketEventArg20_SentFIX(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
		{
			StatUpDat.End( (long) e.BytesTransferred );
			
			RscFtpClientCommand cmd = null;
			if( e.UserToken != null ) cmd = (RscFtpClientCommand) e.UserToken;
			
			//_Log(cmd, "SocketEventArg20_SentFIX..." );
			
			SocketAsyncEventArgs socketEventArg = null;
			
			if( cmd.HasMemStream )
			{
				//_Log(cmd, "cmd.HasMemStream..." );
				cmd.StreamDone += ((long) e.BytesTransferred);
				//_Log(cmd, "cmd.StreamDone = " + cmd.StreamDone.ToString() );
				if( cmd.StreamDone < cmd.MemStream.Length )
				{
					RscFtpCommandEventArgs newep = new RscFtpCommandEventArgs();
					newep.ProgressMax = (int) cmd.MemStream.Length;
					newep.ProgressPos = (int) cmd.StreamDone;
					if( ProgressAsync != null )
						ProgressAsync(this, newep);
						
					socketEventArg = new SocketAsyncEventArgs();
					socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
					socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
						SocketEventArg20_SentFIX);
						//SocketEventArg20_Sent);
					socketEventArg.UserToken = cmd;
		
					System.IO.MemoryStream ms;
					ms = cmd.MemStream;
					int iSize = m_iServerResponseBufferSize_DAT;
					if( ((int) (ms.Length - cmd.StreamDone)) < iSize ) iSize = ((int) (ms.Length - cmd.StreamDone));
					byte[] buff = new byte[iSize];
					ms.Read(buff, 0, iSize );
					
					//_Log(cmd, "Sending " + iSize.ToString() + " byte(s)..." );
					socketEventArg.SetBuffer(buff, 0, iSize);
					
					StatUpDat.Start();
					m_Socket20.SendAsync(socketEventArg);
					
					return;
				}
				else
				{
					cmd.MemStream.Close();
				}
			}
			else if( cmd.HasFileStream )
			{
				//_Log(cmd, "cmd.HasFileStream..." );		
				cmd.StreamDone += ((long) e.BytesTransferred);
				//_Log(cmd, "cmd.StreamDone = " + cmd.StreamDone.ToString() );
				if( cmd.StreamDone < cmd.FileStream.Length )
				{
					RscFtpCommandEventArgs newep = new RscFtpCommandEventArgs();
					newep.ProgressMax = (int) cmd.FileStream.Length;
					newep.ProgressPos = (int) cmd.StreamDone;
					if( ProgressAsync != null )
						ProgressAsync(this, newep);
						
					socketEventArg = new SocketAsyncEventArgs();
					socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
					socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
						SocketEventArg20_SentFIX);
						//SocketEventArg20_Sent);
					socketEventArg.UserToken = cmd;
		
					System.IO.Stream fs;
					fs = cmd.FileStream;
					
					int iSize = m_iServerResponseBufferSize_DAT;
					if( ((int) (fs.Length - cmd.StreamDone)) < iSize ) iSize = ((int) (fs.Length - cmd.StreamDone));
					byte[] buff = new byte[iSize];
					fs.Read(buff, 0, iSize );
					
					//_Log(cmd, "Sending " + iSize.ToString() + " byte(s)..." );
					socketEventArg.SetBuffer(buff, 0, iSize);
					
					StatUpDat.Start();
					m_Socket20.SendAsync(socketEventArg);
					
					return;
				}
				else
				{
					cmd.FileStream.Close();
				}
			}
						
			socketEventArg = new SocketAsyncEventArgs();
			socketEventArg.RemoteEndPoint = m_Socket20.RemoteEndPoint;
			socketEventArg.Completed +=new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(
				SocketEventArg20_Sent);
			socketEventArg.UserToken = cmd;
			
			Byte[] ayMsg = new Byte[1];
			
			//_Log(cmd, "Sending FIX zero byte..." );
			socketEventArg.SetBuffer(ayMsg, 0, 0);
			
			StatUpDat.Start();
			m_Socket20.SendAsync(socketEventArg);
		}
			
		private void SocketEventArg20_Sent(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
		{
			StatUpDat.End( (long) e.BytesTransferred );
			
			RscFtpClientCommand cmd = null;
			if( e.UserToken != null ) cmd = (RscFtpClientCommand) e.UserToken;
			
			RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
			
			newe.ClientCommand = cmd;
			
			if( ServerDataSentAsync != null )
				ServerDataSentAsync(this, newe);
			
			//Close marks the end of sending...
			/*
			 * For WM6.5|Mocha FTP Server end of sent data has been lost!
			 * Possible FIX!!!
			 *
			 * MUST NOT HERE!!!
			 */
			//CloseDataSocket( cmd );
		}
	
		public void CloseDataSocket( RscFtpClientCommand ClientCommand, RscFtpServerData ServerData = null )
		{
			RscFtpCommandEventArgs newe = new RscFtpCommandEventArgs();
			
			newe.ClientCommand = ClientCommand;
			newe.ServerData = ServerData;
			
			if( DataSocketClosingAsync != null )
				DataSocketClosingAsync(this, newe);
			
			_StopDataSocket( );

			switch( ClientCommand.Command )
			{
				
				case "STOR" :	//Server sends ack after DataSocket has been closed, so handle here!
								//see: DispatchResponse
					break;
					
				default :
				{
					/*RscFtpCommandEventArgs*/ newe = new RscFtpCommandEventArgs();
					
					newe.ClientCommand = ClientCommand;
					newe.ServerData = ServerData;
					
					if( CommandDoneAsync != null )
						CommandDoneAsync(this, newe);
					
					break;
				}
			}
		}
		
		private void _StopDataSocket( )
		{
			if( m_Socket20 != null )
			{
				
				/*
				 * For WM6.5|Mocha FTP Server end of sent data has been lost!
				 * Possible FIX!!!
				 * NOT the FIX!!!
				 */
				//m_Socket20.Shutdown(SocketShutdown.Both);
				
				m_Socket20.Close();
				m_Socket20.Dispose();
				m_Socket20 = null;
			}
		}
		
		public void CloseAllSockets()
		{
			_StopDataSocket( );
			
			if( m_Socket21 != null )
			{
				m_Socket21.Shutdown(SocketShutdown.Both);
				m_Socket21.Close();
				m_Socket21.Dispose();
				m_Socket21 = null;
			}
			
			WorkingDirectory = "";
			ServerTitle = "";
		}
		
	}
	
	public class RscFtpServerDataItemFileInfo : RscFtpServerDataItem
	{
		
		string m_sAttr;
		
		string m_sUnk1;
		string m_sUnk2;
		string m_sUnk3;
		
		public string m_sSize;
		
		string m_sDtM;
		string m_sDtD;
		string m_sDtTM;
		
		string m_sName;
		
		public RscFtpServerDataItemFileInfo( string sInf, string sSvrBackSlashInPath )
		{
			
			char [] acDelim = new char [1];
			acDelim[ 0 ] = ' ';
			string [] astr;
			astr = sInf.Split(acDelim, StringSplitOptions.RemoveEmptyEntries );
			
			m_sAttr = astr[0];
			
			m_sUnk1 = astr[1];
			m_sUnk2 = astr[2];
			m_sUnk3 = astr[3];
			
			m_sSize = astr[4];
			
			m_sDtM = astr[5];
			m_sDtD = astr[6];
			m_sDtTM = astr[7];
			
			m_sName = astr[8];
			for( int i = 9; i < astr.Length; i++ )
			{
				m_sName += " ";
				m_sName += astr[ i ];
			}
			
			//FIX (for PC|CSFtp(David McClarnon, dmcclarnon@ntlworld.com))
			m_sName = m_sName.Replace("\\", sSvrBackSlashInPath);
			m_sName = m_sName.Replace("/", sSvrBackSlashInPath);
		}
		
		public override string GetItemTitle( )
		{
			return m_sName;
		}
		
		public bool IsFolder
		{
			get
			{
				if( m_sAttr.Length > 0 )
				{
					return (m_sAttr[0] == 'd' || m_sAttr[0] == 'D');
				}
				
				return false;
			}
		}
		
	}
	
	public class RscFtpServerDataItem
	{
		
		public RscFtpServerDataItem( )
		{
		}
		
		public virtual string GetItemTitle( )
		{
			return "";
		}
		
	}
	
	public class RscFtpServerData
	{
		
		RscFtpClientCommand m_cmd;
		
		bool m_bDoCloseSocket;
		
		public bool ParseOk;
		
		List<RscFtpServerDataItem> m_a = new List<RscFtpServerDataItem>();
		
		System.IO.MemoryStream m_mem = null;
		
		System.IO.Stream m_fs = null;
		
		public RscFtpServerData( RscFtpClientCommand cmd )
		{
			m_cmd = cmd;
			m_bDoCloseSocket = false;
			ParseOk = false;
		}
		
		public System.IO.MemoryStream MemStream
		{
			set
			{
				m_mem = value;
				ParseOk = true;
				m_bDoCloseSocket = true;
			}
			get{ return m_mem; }
		}
		
		public System.IO.Stream FileStream
		{
			set
			{
				m_fs = value;
				ParseOk = true;
				m_bDoCloseSocket = true;
			}
			get{ return m_fs; }
		}
		
		public bool DoCloseSocket
		{
			get{ return m_bDoCloseSocket; }
		}
		
		public int Count
		{
			get{ return m_a.Count; }
		}
		
		public RscFtpServerDataItem GetItem( int nIdx )
		{
			return m_a[ nIdx ];
		}
		
		public string GetItemTitle( int nIdx )
		{
			return m_a[ nIdx ].GetItemTitle( );
		}
		
		public bool Parse( string sData, string sSvrBackSlashInPath )
		{
			ParseOk = false;
			
			switch( m_cmd.Command )
			{
				
				case "LIST" :
				{
					
					m_bDoCloseSocket = true;
					
					string [] astrDelim = new string [1];
					astrDelim[ 0 ] = "\r\n";
					string [] astr;
					astr = sData.Split(astrDelim, StringSplitOptions.None);
					
					bool bLstOver = false;
					foreach( string str in astr )
					{
						if( str.Length == 0 )
						{
							if( !bLstOver ) bLstOver = true; //Set once...
						}
						else
						{
							bLstOver = false;
							RscFtpServerDataItemFileInfo fi = new RscFtpServerDataItemFileInfo( str, sSvrBackSlashInPath );
							m_a.Add(fi);
						}
					}
					
					if( !bLstOver ) return false; //Final string must be empty!!!
					
					break;
				}
				
				default :
					return false;
				
			}
			
			ParseOk = true;
			return true;
		}
		
		public int GetIndexByTitle( string sTitle )
		{
			for( int iIdx = 0; iIdx < m_a.Count; iIdx++ )
			{
				if( sTitle.ToUpper() == m_a[iIdx].GetItemTitle().ToUpper() ) return iIdx;
			}
			
			return -1;
		}
		
	}
	
	public class RscFtpClientCommand
	{
		
		public static RscFtpClientCommand NOOP( bool bSupressOk200 = false )
		{
			RscFtpClientCommand cmd = new RscFtpClientCommand( 1, "NOOP" );
			
			cmd.SupressOk200 = bSupressOk200;
			
			return cmd;
		}
		
		public static RscFtpClientCommand LogInUser( string sUserName )
		{
			return new RscFtpClientCommand( 1, "USER", sUserName );
		}
		
		public static RscFtpClientCommand LogInPassWord( string sPassWord )
		{
			return new RscFtpClientCommand( 1, "PASS", sPassWord );
		}
		
		public static RscFtpClientCommand EnterPassiveMode()
		{
			return new RscFtpClientCommand( 1, "PASV" );
		}
		
		public static RscFtpClientCommand ListFilesAndFolders(string sFolderPath = "", object oTag = null)
		{
			return new RscFtpClientCommand( 2, "LIST", sFolderPath, oTag );
		}
		
		public static RscFtpClientCommand UploadTxt( string sName, string sTxt )
		{
			RscFtpClientCommand cmd = new RscFtpClientCommand( 2, "STOR", sName );
			
			cmd.UserData = sTxt;
			
			return cmd;
		}
		
		public static RscFtpClientCommand UploadBin( string sName, System.IO.MemoryStream ms, System.IO.Stream fs = null )
		{
			RscFtpClientCommand cmd = new RscFtpClientCommand( 2, "STOR", sName );
			
			cmd.MemStream = ms;
			cmd.FileStream = fs;
			cmd.StreamDone = 0;
			
			return cmd;
		}
		
		public static RscFtpClientCommand UploadBin( string sName, System.IO.MemoryStream ms )
		{
			RscFtpClientCommand cmd = new RscFtpClientCommand( 2, "STOR", sName );
			
			cmd.MemStream = ms;
			cmd.StreamDone = 0;
			
			return cmd;
		}
		
		public static RscFtpClientCommand DownloadBin(string sName, long lDataSize, string sUserData = "", object oTag = null, System.IO.Stream fs = null)
		{
			RscFtpClientCommand cmd = new RscFtpClientCommand( 2, "RETR", sName, oTag );
			
			cmd.DataSize = lDataSize;
			cmd.UserData = sUserData;
			
			cmd.FileStream = fs;
			cmd.StreamDone = 0;
			
			return cmd;
		}
		
		public static RscFtpClientCommand DeleteFile( string sName )
		{
			return new RscFtpClientCommand( 1, "DELE", sName );
		}
		
		public static RscFtpClientCommand GetLastModifiedFileDateTime( string sName )
		{
			return new RscFtpClientCommand( 1, "MDTM", sName );
		}
		
		public static RscFtpClientCommand CreateFolder( string sName )
		{
			return new RscFtpClientCommand( 1, "MKD", sName );
		}
		
		public static RscFtpClientCommand DeleteFolder( string sName )
		{
			return new RscFtpClientCommand( 1, "RMD", sName );
		}
		
		public static RscFtpClientCommand ChangeWorkingDirecory( string sName )
		{
			return new RscFtpClientCommand( 1, "CWD", sName );
		}
		
		public static RscFtpClientCommand PrintWorkingDirecory( )
		{
			return new RscFtpClientCommand( 1, "PWD" );
		}
		
		public RscFtpClientCommand Parent;	//For Auto PASV...
		
		public int ResponseCount;
		public int ResponseCountArrived;
		
		string m_strCmd;
		
		string m_strArg1;
		
		string m_strUserData;
		
		public object Tag = null;
		
		System.IO.MemoryStream m_ms;
		System.IO.Stream m_fs;
		public long StreamDone = 0;
		
		public bool SupressOk200;
		
		public long DataSize = -1;
		
		
		public RscFtpClientCommand( int iResponseCount, string strCmd, string strArg1 = "", object oTag = null )
		{
			Parent = null;
			
			ResponseCount = iResponseCount;
			ResponseCountArrived = 0;
			
			m_strCmd = strCmd;
			m_strArg1 = strArg1;
			
			m_strUserData = "";
			
			m_ms = null;
			m_fs = null;
			StreamDone = 0;
			
			Tag = oTag;
			
			SupressOk200 = false;
			
			DataSize = -1;
		}
		
		public string Command
		{
			get{ return m_strCmd; }
		}
		
		public string Arg1
		{
			get{ return m_strArg1; }
		}
		
		public bool HasUserData
		{
			get{ return (m_strUserData.Length > 0); }
		}
		
		public string UserData
		{
			get{ return m_strUserData; }
			set{ m_strUserData = value; }
		}
		
		public bool HasMemStream
		{
			get{ return (m_ms != null); }
		}
		
		public System.IO.MemoryStream MemStream
		{
			get{ return m_ms; }
			set{ m_ms = value; }
		}
		
		public bool HasFileStream
		{
			get{ return (m_fs != null); }
		}
		
		public System.IO.Stream FileStream
		{
			get{ return m_fs; }
			set{ m_fs = value; }
		}
		
		public override string ToString()
		{
			string sFullCmd = m_strCmd;
			
			if( m_strArg1.Length > 0 )
			{
				sFullCmd += " ";
				sFullCmd += m_strArg1;
			}
			else
			{
				if( m_strCmd == "PASS" )
				{
					//BugFix: WM6.5|Mocha FTP Server requires!!!
					sFullCmd += " ";
				}
			}
			
			return sFullCmd;
		}
		
	}
	
	public class RscFtpServerResponse
	{
		
		public enum ServerResponseType {
			Unknown = -1,
			Success = 0,
			Data = 1,
			Warning = 2,
			Error = 3
		};
		
		public ServerResponseType Type
		{
			get{ return m_type; }
		}
		
		public string GetTypeAsString()
		{
			switch( m_type )
			{
				case ServerResponseType.Unknown : return "<UNKNOWN>";
				case ServerResponseType.Success : return "OK";
				case ServerResponseType.Data : return "Data Input";
				case ServerResponseType.Warning : return "Warning";
				case ServerResponseType.Error : return "ERROR";
					
				default : return "<UNEXPECTED>";
			}
		}
		
		ServerResponseType m_type;
		
		string m_strResponse;
		
		int m_iCode;
		string m_strMsg;
		
		string m_strServerIP;
		int m_iServerPort;
		
		bool m_bServerSendingData;
		bool m_bServerReceivingData;
		
		public RscFtpServerResponse( RscFtpClientCommand cmd, string strResponse )
		{
			m_type = ServerResponseType.Unknown;
			m_strResponse = strResponse;
			m_iCode = -1;
			m_strMsg = "";
			m_strServerIP = "";
			m_iServerPort = -1;
			m_bServerSendingData = false;
			m_bServerReceivingData = false;
			
			string sRes;
			int iStart;
			int iPos, iPos2;
			string strTag;
			
			sRes = m_strResponse;
			
			//DONE EARLYER!!!
			/*
			if( sRes[ sRes.Length - 1 ] == '\r' ) sRes = sRes.Substring(0, sRes.Length - 1 );
			if( sRes[ sRes.Length - 1 ] == '\n' ) sRes = sRes.Substring(0, sRes.Length - 1 );
			if( sRes[ sRes.Length - 1 ] == '\r' ) sRes = sRes.Substring(0, sRes.Length - 1 );
			if( sRes[ sRes.Length - 1 ] == '\n' ) sRes = sRes.Substring(0, sRes.Length - 1 );
			*/
			
			iStart = 0;
			iPos = sRes.IndexOf(' ', iStart);
			if( iPos < 0 ) return;
			
			strTag = sRes.Substring(iStart, (iPos - iStart) + 1);
			iStart = iPos + 1;
			if( !Int32.TryParse( strTag, out m_iCode ) ) return;
			
			sRes = sRes.Substring( iStart );
			
			switch( m_iCode )
			{
				
				case 150 :
					
					switch( cmd.Command )
					{
						
						case "LIST" :
						{
							m_bServerSendingData = true;
							
							//Mocha FTP Server (for Windows Mobile) reports LIST res len
							//Must use, because part of list res can be lost!!!
							iPos = sRes.IndexOf( " bytes)" );
							if( iPos >= 0 )
							{
								string sRes2 = sRes.Substring(0, iPos);
								iPos2 = sRes2.LastIndexOf('(');
								if( iPos2 >= 0 )
								{
									sRes2 = sRes2.Substring(iPos2 + 1);
									if( sRes2.Length > 0 )
									{
										long lLen = 0;
										if( long.TryParse( sRes2, out lLen ) )
										{
											cmd.DataSize = lLen;
										}
									}
								}
							}
							
							break;
						}
							
						case "STOR" :
							m_bServerReceivingData = true;
							break;
						
						case "RETR" :
							m_bServerSendingData = true;
							break;
							
					}
					
					m_strMsg = sRes;
					
					m_type = ServerResponseType.Data;
					break;
				
				case 200 :
					m_strMsg = "Ok";
					m_type = ServerResponseType.Success;
					break;
				
				case 202 : //UTF8 is default, regardless of OPTS UTF8 ON
					if( cmd.Command == "OPTS" && cmd.Arg1 == "UTF8 ON" )
					{
						m_strMsg = sRes;
						m_type = ServerResponseType.Success;
					}
					break;
							
				case 215 : //SYST
					m_strMsg = sRes;
					m_type = ServerResponseType.Success;
					break;
				
				case 220 :
					m_strMsg = sRes;
					m_type = ServerResponseType.Success;
					break;
							
				case 226 : //LIST, STOR, RETR succeeded
					m_strMsg = sRes;
					m_type = ServerResponseType.Success;
					break;
					
				case 227 :
				{
					
					m_strMsg = sRes;
					
					if( sRes.Length < 12 ) return;
					
					if( sRes[ 0 ] == '=' )
					{
						//Fix for PC|CSFtp(David McClarnon, dmcclarnon@ntlworld.com)!
						sRes = sRes.Substring(1);
					}
					else
					{
						//Fix for Android - Ftp server
						iPos = sRes.IndexOf('(');
						if( iPos < 0 ) return;
						iPos2 = sRes.IndexOf(')', iPos);
						if( iPos2 < 0 ) return;
						sRes = sRes.Substring(iPos + 1, (iPos2 - iPos) - 1 );
					}
					
					string [] astrTags;
					astrTags = sRes.Split(',');
					if( astrTags.Length != 6 ) return;
					
					m_strServerIP = String.Format("{0}.{1}.{2}.{3}", astrTags);
					
					int iVal = 0;
					if( !Int32.TryParse(astrTags[4], out iVal ) ) return;
					m_iServerPort = iVal * 256;
					if( !Int32.TryParse(astrTags[5], out iVal ) ) return;
					m_iServerPort += iVal;
					
					m_type = ServerResponseType.Data;
					break;
				}
				
				case 230 : //User logged in.
					m_strMsg = sRes;
					m_type = ServerResponseType.Success;
					break;
							
				case 250 : //CWD succeeded
					m_strMsg = sRes;
					m_type = ServerResponseType.Success;
					break;
				
				case 257 : //MKD, PWD succeeded
				{
					switch( cmd.Command )
					{
						
						case "MKD" :
						{
							m_strMsg = sRes;
							m_type = ServerResponseType.Success;
							break;
						}
							
						case "PWD" :
						{
							m_strMsg = sRes;
							
							iPos = sRes.IndexOf("\"");
							if( iPos >= 0 )
							{
								iPos2 = sRes.IndexOf("\"", iPos + 1);
								if( iPos2 >= 0 )
								{
									m_strMsg = sRes.Substring(iPos + 1, (iPos2 - iPos) - 1);
									
									if( m_strMsg.Length == 0 ) m_strMsg = "/";
									if( m_strMsg[ m_strMsg.Length - 1 ] != '/' ) m_strMsg += "/";
								}
							}
							
							m_type = ServerResponseType.Success;
							break;
						}
						
					}
							
					break;
				}
				
				case 331 : //User logged on, but needs password
					m_strMsg = sRes;
					m_type = ServerResponseType.Warning;
					break;
							
				case 500 :
				{
					switch( cmd.Command )
					{
						
						case "OPTS" :
						{
							if( cmd.Arg1 == "UTF8 ON" )
							{
								m_strMsg = sRes;
								m_type = ServerResponseType.Success; //Fix for PC|CSFtp(David McClarnon, dmcclarnon@ntlworld.com)
							}
							else
							{
								m_strMsg = sRes;
								m_type = ServerResponseType.Error;
							}
							break;
						}
							
						default :
							m_strMsg = sRes;
							m_type = ServerResponseType.Error;
							break;
							
					}
					break;
				}
							
				case 530 : //Usr / Pwd incorrect.
					m_strMsg = sRes;
					m_type = ServerResponseType.Error;
					break;
							
				case 550 :
				{
					switch( cmd.Command )
					{
						
						case "OPTS" :
						{
							if( cmd.Arg1 == "UTF8 ON" )
							{
								m_strMsg = sRes;
								m_type = ServerResponseType.Success; //Fix for PC|CSFtp(David McClarnon, dmcclarnon@ntlworld.com)
							}
							else
							{
								m_strMsg = sRes;
								m_type = ServerResponseType.Error;
							}
							break;
						}
						
						case "SYST" :
							m_strMsg = "";
							m_type = ServerResponseType.Success; //Fix for PC|CSFtp(David McClarnon, dmcclarnon@ntlworld.com)
							break;
						
						case "MKD" : //Directory already exists.
							m_strMsg = sRes;
							m_type = ServerResponseType.Success; //Fix for WM6.5|Mocha FTP Server
							break;
							
						default :
							m_strMsg = sRes;
							m_type = ServerResponseType.Error;
							break;
							
					}
					break;
				}
					
				default :
					m_strMsg = sRes;
					return; //Unexpected...
			}
		}
		
		public int Code
		{
			get{ return m_iCode; }
		}
		
		public string Message
		{
			get{ return m_strMsg; }
		}
		
		public bool DataPortSettings
		{
			get{ return ((m_iCode == 227) && (m_strServerIP.Length != 0) && (m_iServerPort > 0)); }
		}
		
		public string ServerIP
		{
			get{ return m_strServerIP; }
		}
		
		public int ServerPort
		{
			get{ return m_iServerPort; }
		}
		
		public bool ServerSendingData
		{
			get{ return (m_bServerSendingData && (m_type != ServerResponseType.Unknown)); }
		}
		
		public bool ServerReceivingData
		{
			get{ return (m_bServerReceivingData && (m_type != ServerResponseType.Unknown)); }
		}
		
		public bool CanGoOn
		{
			get{ return ((m_type == ServerResponseType.Success) || (m_type == ServerResponseType.Warning)); }
		}
		
		public override string ToString()
		{
			string strRet = "{ ";
			
			strRet += GetTypeAsString( );
			strRet += ": ";
			
			strRet += m_iCode.ToString();
			strRet += " | ";
			
			strRet += m_strMsg;
			strRet += " | ";
			
			strRet += m_strServerIP;
			strRet += ":";
			strRet += m_iServerPort.ToString();
			
			strRet += " }";
			
			return strRet;
		}
		
	}
	
	public class RscFtpStat
	{
		
		RscFtpStat m_statToReStart = null;
		
		long m_lByteCount = 0;
		
		bool m_bInStopper = false;
		DateTime m_dtStart = DateTime.Now;
		
		double m_dSeconds = 0;
		
		public RscFtpStat()
		{
			m_statToReStart = null;
			
			m_lByteCount = 0;
			m_dSeconds = 0;
			
			m_bInStopper = false;
		}
		
		public void SetToReStartTo( RscFtpStat stat )
		{
			m_statToReStart = stat;
		}
		
		public long ByteCount
		{
			get
			{
				return m_lByteCount;
			}
		}
		
		public long Seconds
		{
			get
			{
				//DEBUG...
				/*
				if( m_bInStopper )
					return (-1 * (1 + ((long) Math.Round( m_dSeconds, 0 ))));
				*/
				
				return (long) Math.Round( m_dSeconds, 0 );
			}
		}
		
		public void Clear()
		{
			m_lByteCount = 0;
			m_dSeconds = 0;
			
			m_bInStopper = false;
		}
		
		public bool Started { get{ return m_bInStopper; } }
		
		public void Start()
		{
			m_dtStart = DateTime.Now;
			
			m_bInStopper = true;
			
			//FIX!!! - Download / Upload DATA time included int CMD Download!!!
			if( m_statToReStart != null )
			{
				if( m_statToReStart.Started )
				{
					m_statToReStart.Start();
				}
			}
			//FIX!!! - Download / Upload DATA time included int CMD Download!!!
		}
		
		public void End( long lBytesTransferred )
		{
			//FIX!!! - Download / Upload DATA time included int CMD Download!!!
			if( m_statToReStart != null )
			{
				if( m_statToReStart.Started )
				{
					m_statToReStart.Start();
				}
			}
			//FIX!!! - Download / Upload DATA time included int CMD Download!!!
			
			m_bInStopper = false;
			
			m_lByteCount += lBytesTransferred;
			
			DateTime dt = DateTime.Now;
			TimeSpan ts = dt - m_dtStart;
			
			m_dSeconds += ts.TotalSeconds;
		}
		
	}
	
}