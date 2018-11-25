using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace RscXtests
{
	
    public partial class RscJsV13 : PhoneApplicationPage
    {
		
        public RscJsV13()
        {
            InitializeComponent();
			
            wbc.Navigated += new EventHandler<NavigationEventArgs>(WebControlNavigatesTo);
            wbc.ScriptNotify += new EventHandler<NotifyEventArgs>(JavaScriptNotify);
            wbc.IsScriptEnabled = true;
            wbc.IsGeolocationEnabled = true;
			
			string strHtml = @"<html><head>
			<meta http-equiv=""content-type"" content=""text/html; charset=ISO-8859-1"">

			<meta name=""Viewport"" content=""width=320; user-scaleable=no; 
			initial-scale=1.0"">

			<style type=""text/css""> 
			body {
				background: black;
							color: #80c0c0; 
			} 
			</style>

			<script>
			function callNative()
				{
				window.external.notify(""Hello, native!"");
				}
			function test(message)
				{
				window.external.notify(""test called: "" + message);
				}
			function test2(oPar)
				{
				window.external.notify(""oPar: "" + oPar.toString());
				}
			function addToBody(text)
				{
				document.body.innerHTML = document.body.innerHTML + ""<br>"" + text;
				}
			</script>
			
			</head>
			<body>
			
			This text and the button below are inside the WebBrowser 
			control. If you press the button, the code on ""C#-side"" (In class 
			MainPage) will be called. (JavaScript code calls C# code.) It shows a 
			MessageBox dialog. If you look the code, you will find out that 
			passing parameters is also possible.<br><br>
			
			<button type=""button"" onclick=""callNative();"">Call to Native 
			Code!</button>
			
			<br><br>
			If you press the application bar button on the left, JavaScript function 
			will be called from the C# side. (C# code calls JavaScript) The code adds 
			some text dynamically to WebBrowser's DOM tree.
			
			</body></html>";
			
			wbc.NavigateToString( strHtml );
        }

        void JavaScriptNotify(Object sender, NotifyEventArgs notifyArgs)
        {
            txLog.Text = "JavaScriptNotify: " + notifyArgs.Value;
        }

        void WebControlNavigatesTo(Object sender, NavigationEventArgs navArgs)
        {
			txLog.Text = "NavTo: " + navArgs.Uri.ToString();
        }
				
		private void btnCall_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            String currentDateTime = DateTime.Now.ToString();

            try
            {
                wbc.InvokeScript("eval", new string[] { "addToBody(' " + currentDateTime + " ');" });
				wbc.InvokeScript("test", new String[] { "called from client code" });
            }
            catch (Exception)
            {
                // Not handled
            }
		}
		
    }
	
}
