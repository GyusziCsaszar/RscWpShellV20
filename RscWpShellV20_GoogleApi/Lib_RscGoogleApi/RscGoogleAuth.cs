using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Threading.Tasks;

using RestSharp;
using RestSharp.Authenticators;

using Ressive.Formats;

using Ressive.Https;

namespace Ressive.GoogleApi
{
	
	//SRC: http://stackoverflow.com/questions/10153749/how-should-i-implement-executeasync-with-restsharp-on-windows-phone-7
	/*
	public Task<T> ExecuteAsync<T>(RestRequest request) where T : new()
    {
        var client = new RestClient();
        var taskCompletionSource = new TaskCompletionSource<T>();
        client.BaseUrl = BaseUrl;
        client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
        request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment);
        client.ExecuteAsync<T>(request, (response) => taskCompletionSource.SetResult(response.Data));
        return taskCompletionSource.Task;
    }
	private async Task DoWork()
    {
        var api = new HarooApi("MyAcoountId", "MySecret");
        var request = new RestRequest();
        var myClass = await api.ExecuteAsync<MyClass>(request);

        // Do something with myClass
    }
	*/
	//SRC: http://stackoverflow.com/questions/10153749/how-should-i-implement-executeasync-with-restsharp-on-windows-phone-7
	
	public class RscGoogleAuthResult
	{
		public RscHttpsAuthResult Auth;
	}

	//
	// SRC: http://www.codeproject.com/Articles/321291/Google-OAuth-on-Windows-Phone
	//
	public class RscGoogleAuth
	{
		
        private RscHttpsAuth m_httpsAuth = null;
		
        private object m_oSync = new object();
		
		private void RaisePropertyChanged( string sPropertyName )
		{
			//TODO...
		}
		
		public RscGoogleAuth( RscJSonItem jsonClientSecret, string sGoogleScopes, WebBrowser wbAuthPage )
		{
				
			string sClientID = jsonClientSecret.GetChildPropertyValue( "installed", "client_id" );
			string sClientSecret = jsonClientSecret.GetChildPropertyValue( "installed", "client_secret" );
			
			if( sClientID.Length == 0 || sClientSecret.Length == 0 )
				throw new Exception( "Missing critical client_secret data!" );
			
			m_httpsAuth = new RscHttpsAuth( "https://accounts.google.com" )
			{
				
				ApprovalEndPoint = "/o/oauth2/approval",
				TokenEndPoint = "/o/oauth2/token",
				AuthEndPoint = "/o/oauth2/auth",
				RedirectUri = "http://localhost",
				
				ClientId = sClientID,
				Secret = sClientSecret,

				Scope = sGoogleScopes
				
			};
			
			m_httpsAuth.Authenticated += new EventHandler(m_httpsAuth_Authenticated);
			m_httpsAuth.AuthenticationFailed += new EventHandler(m_httpsAuth_AuthenticationFailed);
			
			if( wbAuthPage != null ) //If null, non-UI mode, Tiles for example...
			{
				wbAuthPage.Navigating += new EventHandler<NavigatingEventArgs>(wbAuthPage_Navigating);
            	wbAuthPage.Navigated += new EventHandler<NavigationEventArgs>(wbAuthPage_Navigated);
				//wbAuthPage.NavigationFailed += new NavigationFailedEventHandler(wbAuthPage_NavigationFailed);
			}
		}

        private void wbAuthPage_Navigating(object sender, NavigatingEventArgs e)
        {
			WebBrowser wbAuthPage = (WebBrowser) sender;
			
            if (e.Uri.Host.Equals("localhost"))
            {
                wbAuthPage.Visibility = Visibility.Collapsed;
                e.Cancel = true;
                int pos = e.Uri.Query.IndexOf("=");

                // setting this text will bind it back to the view model
                Code = pos > -1 ? e.Uri.Query.Substring(pos + 1) : null;
            }
        }

        private void wbAuthPage_Navigated(object sender, NavigationEventArgs e)
        {
			WebBrowser wbAuthPage = (WebBrowser) sender;
			
            wbAuthPage.Visibility = Visibility.Visible;
        }
		
        public RscGoogleAuthResult AuthResult
		{
			set
			{
				if( m_httpsAuth != null )
				{
					if( value == null )
						m_httpsAuth.AuthResult = null;
					else
						m_httpsAuth.AuthResult = value.Auth;
				}
			}
			get
			{
				if( m_httpsAuth != null )
				{
					RscGoogleAuthResult auth = new RscGoogleAuthResult();
					auth.Auth = m_httpsAuth.AuthResult;
					return auth;
				}
				
				return null;
			}
		}
		
        public bool HasAuthenticated
        {
            get
            {
                lock (m_oSync)
                    return m_httpsAuth.HasAuthenticated;
            }
        }

        private Uri _authUri = new Uri("about:blank");
        public Uri AuthUri
        {
            get
            {
                return _authUri;
            }
            set
            {
                if (_authUri != value)
                {
                    _authUri = value;
                    RaisePropertyChanged("AuthUri");
                }
            }
        }

        private string _code;
        public string Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                m_httpsAuth.ExchangeCodeForToken(Code);
            }
        }

        public void Logout()
        {
            lock (m_oSync)
            {
                m_httpsAuth.Revoke();
            }
            RaisePropertyChanged("HasAuthenticated");
        }
		
		public void SendRequest( string sBaseUrl, string sUriResource )
		{			
			GetAccessCode(s => LoadResult(s, sBaseUrl, sUriResource, null));
		}
		
		public Task<Object> SendRequestTask( string sBaseUrl, string sUriResource )
		{			
        	var taskCompletionSource = new TaskCompletionSource<Object>();
			
			GetAccessCode(s => LoadResult(s, sBaseUrl, sUriResource, taskCompletionSource));
			
			return taskCompletionSource.Task;
		}
		
        private bool _isAuthenticating;

        private Queue<Action<string>> _queuedRequests = new Queue<Action<string>>();
        private void GetAccessCode(Action<string> callback)
        {
            lock (m_oSync)
            {
                if (_isAuthenticating)
                {
                    _queuedRequests.Enqueue(callback);
                }
                else if (HasAuthenticated)
                {
                    if (!m_httpsAuth.AuthResult.IsExpired)
                    {
                        callback(m_httpsAuth.AuthResult.access_token);
                    }
                    else
                    {
                        _isAuthenticating = true;
                        _queuedRequests.Enqueue(callback);
                        m_httpsAuth.RefreshAccessToken();
                    }
                }
                else
                {
                    _isAuthenticating = true;
                    _queuedRequests.Enqueue(callback);

                    //((PhoneApplicationFrame)App.Current.RootVisual).Navigate(new Uri("/AuthenticationPage.xaml", UriKind.Relative));
                    AuthUri = m_httpsAuth.AuthUri;
					
					//ShowAuthPage();
					OnShowAuthPage();
                }
            }
        }

        void m_httpsAuth_Authenticated(object sender, EventArgs e)
        {
            lock (m_oSync)
            {
                _isAuthenticating = false;

                while (_queuedRequests.Count > 0)
                    _queuedRequests.Dequeue()(m_httpsAuth.AuthResult.access_token);

				/*
                //ViewModelLocator.SaveSetting("auth", _process.AuthResult);
				//DispatcherHelper.UIDispatcher.BeginInvoke(() =>
				//{
					SaveAuthResult( m_httpsAuth.AuthResult );
				//});
				*/
				OnAuthenticated();
           }

            RaisePropertyChanged("HasAuthenticated");
        }

        void m_httpsAuth_AuthenticationFailed(object sender, EventArgs e)
        {
            lock (m_oSync)
            {
                _isAuthenticating = false;
				
                m_httpsAuth.Revoke();
                RaisePropertyChanged("HasAuthenticated");

                AuthUri = new Uri("about:blank");
                AuthUri = m_httpsAuth.AuthUri;
				
                //MessageBox.Show("Please try again", "Login failed", MessageBoxButton.OK);
				OnAuthenticationFailed();
            }
        }
		
        private bool _loadingResult;
		private TaskCompletionSource<Object> _loadingTask;
       	private void LoadResult(string access_token, string sBaseUrl, string sUriResource, TaskCompletionSource<Object> tsk)
        {
            if (!_loadingResult)
            {
                //Debug.WriteLine("loading");
				//Log( "Loading..." );

                _loadingResult = true;
				
                RestClient client = new RestClient(sBaseUrl);
                client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(access_token);
                var request = new RestRequest(sUriResource, Method.GET);
				
				_loadingTask = tsk;
				
				if( tsk == null )
				{
               		client.ExecuteAsync<Object>(request, ResultLoaded);
				}
				else
				{
					/*
        			//client.ExecuteAsync<Object>(request, (response) => tsk.SetResult(response.Data));
        			//client.ExecuteAsync<Object>(request, (response) => tsk.SetResult(response.Content));
        			client.ExecuteAsync<Object>(request, (response) => tsk.SetResult(response));
					
					/
					tsk.SetResult( "abc" );
					_loadingResult = false;
					*/
					
					//IRestResponse<Object> response = client.Execute<Object>(request);
					
               		client.ExecuteAsync<Object>(request, ResultLoaded);
				}
            }
        }
        private void ResultLoaded(IRestResponse<Object> response)
        {
            _loadingResult = false;
			
			if( _loadingTask != null )
			{
				_loadingTask.SetResult( response.Content );
				
				_loadingTask = null;
			}
			else
			{
				OnResponseReceived( response.ResponseUri.ToString(), response.Content, response.ContentType );
			}
		}
		
		

        public event EventHandler Authenticated;
        protected virtual void OnAuthenticated()
        {
            var e = Authenticated;
            if (e != null)
                e(this, EventArgs.Empty);
        }

        public event EventHandler AuthenticationFailed;
        protected virtual void OnAuthenticationFailed()
        {
            var e = AuthenticationFailed;
            if (e != null)
                e(this, EventArgs.Empty);
        }

        public event EventHandler ShowAuthPage;
        protected virtual void OnShowAuthPage()
        {
            var e = ShowAuthPage;
            if (e != null)
                e(this, EventArgs.Empty);
        }
		
		public delegate void ResponseReceived_EventHandler(object sender, RscGoogleAuthEventArgs e);
		public event ResponseReceived_EventHandler ResponseReceived;
        protected virtual void OnResponseReceived( string sUri, string sContent, string sContentType )
        {
            var e = ResponseReceived;
            if (e != null)
			{	
				RscGoogleAuthEventArgs args = new RscGoogleAuthEventArgs();
				args.Uri = sUri;
				args.Content = sContent;
				args.ContentType = sContentType;
				
                e(this, args);
			}
        }
		
	}
	
    public class RscGoogleAuthEventArgs : EventArgs
    {
		
		public string Uri = "";
		public string Content = "";
		public string ContentType = "";
		
        public RscGoogleAuthEventArgs()
        {
        }
		
    }
	
}