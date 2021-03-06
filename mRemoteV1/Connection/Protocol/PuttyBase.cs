using mRemoteNG.App;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using mRemoteNG.App.Info;
using mRemoteNG.Security.SymmetricEncryption;


namespace mRemoteNG.Connection.Protocol
{
	public class PuttyBase : ProtocolBase
	{	
		private const int IDM_RECONF = 0x50; // PuTTY Settings Menu ID
		bool _isPuttyNg;

	    #region Public Properties
        public Putty_Protocol PuttyProtocol { get; set; }

	    public Putty_SSHVersion PuttySSHVersion { get; set; }

	    public IntPtr PuttyHandle { get; set; }

	    public Process PuttyProcess { get; set; }

	    public static string PuttyPath { get; set; }

	    public bool Focused
		{
			get
			{
				if (NativeMethods.GetForegroundWindow() == PuttyHandle)
					return true;
				return false;
			}
		}
        #endregion

        public PuttyBase() : base()
        {

        }

        #region Private Events & Handlers
		private void ProcessExited(object sender, EventArgs e)
		{
            Event_Closed(this);
		}
        #endregion
				
        #region Public Methods
		public override bool Connect()
		{
			try
			{
				_isPuttyNg = PuttyTypeDetector.GetPuttyType() == PuttyTypeDetector.PuttyType.PuttyNg;
						
				PuttyProcess = new Process();
				PuttyProcess.StartInfo.UseShellExecute = false;
				PuttyProcess.StartInfo.FileName = PuttyPath;
						
				CommandLineArguments arguments = new CommandLineArguments();
				arguments.EscapeForShell = false;
						
				arguments.Add("-load", InterfaceControl.Info.PuttySession);
						
				if (!(InterfaceControl.Info is PuttySessionInfo))
				{
					arguments.Add("-" + PuttyProtocol.ToString());
							
					if (PuttyProtocol == Putty_Protocol.ssh)
					{
						string username = "";
						string password = "";
								
						if (!string.IsNullOrEmpty(InterfaceControl.Info.Username))
						{
							username = InterfaceControl.Info.Username;
						}
						else
						{
							if (Settings.Default.EmptyCredentials == "windows")
							{
								username = Environment.UserName;
							}
							else if (Settings.Default.EmptyCredentials == "custom")
							{
								username = Convert.ToString(Settings.Default.DefaultUsername);
							}
						}
								
						if (!string.IsNullOrEmpty(InterfaceControl.Info.Password))
						{
							password = InterfaceControl.Info.Password;
						}
						else
						{
							if (Settings.Default.EmptyCredentials == "custom")
							{
                                var cryptographyProvider = new LegacyRijndaelCryptographyProvider();
                                password = cryptographyProvider.Decrypt(Convert.ToString(Settings.Default.DefaultPassword), GeneralAppInfo.EncryptionKey);
							}
						}
								
						arguments.Add("-" + (int)PuttySSHVersion);
								
						if (((int)Force & (int)ConnectionInfo.Force.NoCredentials) != (int)ConnectionInfo.Force.NoCredentials)
						{
							if (!string.IsNullOrEmpty(username))
							{
								arguments.Add("-l", username);
							}
							if (!string.IsNullOrEmpty(password))
							{
								arguments.Add("-pw", password);
							}
						}
					}
							
					arguments.Add("-P", InterfaceControl.Info.Port.ToString());
					arguments.Add(InterfaceControl.Info.Hostname);
				}
						
				if (_isPuttyNg)
				{
					arguments.Add("-hwndparent", InterfaceControl.Handle.ToString());
				}
						
				PuttyProcess.StartInfo.Arguments = arguments.ToString();
						
				PuttyProcess.EnableRaisingEvents = true;
				PuttyProcess.Exited += ProcessExited;
						
				PuttyProcess.Start();
				PuttyProcess.WaitForInputIdle(Convert.ToInt32(Settings.Default.MaxPuttyWaitTime * 1000));
						
				int startTicks = Environment.TickCount;
				while (PuttyHandle.ToInt32() == 0 & Environment.TickCount < startTicks + (Settings.Default.MaxPuttyWaitTime * 1000))
				{
					if (_isPuttyNg)
					{
						PuttyHandle = NativeMethods.FindWindowEx(
                            InterfaceControl.Handle, new IntPtr(0), null, null);
					}
					else
					{
						PuttyProcess.Refresh();
						PuttyHandle = PuttyProcess.MainWindowHandle;
					}
					if (PuttyHandle.ToInt32() == 0)
					{
						Thread.Sleep(0);
					}
				}
						
				if (!_isPuttyNg)
				{
					NativeMethods.SetParent(PuttyHandle, InterfaceControl.Handle);
				}
						
				Runtime.MessageCollector.AddMessage(MessageClass.InformationMsg, Language.strPuttyStuff, true);
				Runtime.MessageCollector.AddMessage(MessageClass.InformationMsg, string.Format(Language.strPuttyHandle, PuttyHandle.ToString()), true);
				Runtime.MessageCollector.AddMessage(MessageClass.InformationMsg, string.Format(Language.strPuttyTitle, PuttyProcess.MainWindowTitle), true);
				Runtime.MessageCollector.AddMessage(MessageClass.InformationMsg, string.Format(Language.strPuttyParentHandle, InterfaceControl.Parent.Handle.ToString()), true);
						
				Resize(this, new EventArgs());
				base.Connect();
				return true;
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strPuttyConnectionFailed + Environment.NewLine + ex.Message);
				return false;
			}
		}
				
		public override void Focus()
		{
			try
			{
				if (ConnectionWindow.InTabDrag)
				{
					return ;
				}
				NativeMethods.SetForegroundWindow(PuttyHandle);
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strPuttyFocusFailed + Environment.NewLine + ex.Message, true);
			}
		}
				
		public override void Resize(object sender, EventArgs e)
		{
			try
			{
				if (InterfaceControl.Size == Size.Empty)
				{
					return ;
				}
                NativeMethods.MoveWindow(PuttyHandle, Convert.ToInt32(-SystemInformation.FrameBorderSize.Width), Convert.ToInt32(-(SystemInformation.CaptionHeight + SystemInformation.FrameBorderSize.Height)), InterfaceControl.Width + (SystemInformation.FrameBorderSize.Width * 2), InterfaceControl.Height + SystemInformation.CaptionHeight + (SystemInformation.FrameBorderSize.Height * 2), true);
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strPuttyResizeFailed + Environment.NewLine + ex.Message, true);
			}
		}
				
		public override void Close()
		{
			try
			{
				if (PuttyProcess.HasExited == false)
				{
					PuttyProcess.Kill();
				}
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strPuttyKillFailed + Environment.NewLine + ex.Message, true);
			}
					
			try
			{
				PuttyProcess.Dispose();
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strPuttyDisposeFailed + Environment.NewLine + ex.Message, true);
			}
					
			base.Close();
		}
				
		public void ShowSettingsDialog()
		{
			try
			{
                NativeMethods.PostMessage(PuttyHandle, NativeMethods.WM_SYSCOMMAND, IDM_RECONF, 0);
                NativeMethods.SetForegroundWindow(PuttyHandle);
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strPuttyShowSettingsDialogFailed + Environment.NewLine + ex.Message, true);
			}
		}
        #endregion
				
        #region Enums
		public enum Putty_Protocol
		{
			ssh = 0,
			telnet = 1,
			rlogin = 2,
			raw = 3,
			serial = 4
		}
				
		public enum Putty_SSHVersion
		{
			ssh1 = 1,
			ssh2 = 2
		}
        #endregion
	}
}