using System;
using System.Net;
using System.Diagnostics;

using RestSharp;

namespace Ressive.Https
{

	//
	// SRC: http://www.codeproject.com/Articles/321291/Google-OAuth-on-Windows-Phone
	//
    public class RscHttpsAuth
    {

        public RscHttpsAuthResult AuthResult { get; set; }
        public string ApprovalEndPoint { get; set; }
        public string TokenEndPoint { get; set; }
        public string Scope { get; set; }
        public string AuthEndPoint { get; set; }
        public String BaseUri { get; set; }
        public string Secret { get; set; }
        public string ClientId { get; set; }
        public String RedirectUri { get; set; }
		
        RestClient client;

        public RscHttpsAuth( string sBaseUri )
        {
            BaseUri = sBaseUri;
			
            ApprovalEndPoint = "";
            TokenEndPoint = "";
            AuthEndPoint = "";

            ClientId = "";
            Secret = "";
            Scope = "";

            client = new RestClient(this.BaseUri);
        }

        public bool HasAuthenticated
        {
            get
            {
                return AuthResult != null && !string.IsNullOrEmpty(AuthResult.access_token);
            }
        }

        public void Revoke()
        {
            AuthResult = null;
        }

        public Uri AuthUri
        {
            get
            {
                UriBuilder builder = new UriBuilder(BaseUri);
                builder.Path = AuthEndPoint;
				
                builder.Query = string.Format("response_type=code&redirect_uri={0}&scope={1}&client_id={2}"
					, RedirectUri, Scope, ClientId);
				
                return builder.Uri;
            }
        }

        public void ExchangeCodeForToken(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                OnAuthenticationFailed();
            }
            else
            {
                var request = new RestRequest(this.TokenEndPoint, Method.POST);
                request.AddParameter("code", code);
                request.AddParameter("client_id", this.ClientId);
                request.AddParameter("client_secret", this.Secret);
                request.AddParameter("redirect_uri", this.RedirectUri);
                request.AddParameter("grant_type", "authorization_code");

                client.ExecuteAsync<RscHttpsAuthResult>(request, GetAccessToken);
            }
        }

        void GetAccessToken(IRestResponse<RscHttpsAuthResult> response)
        {
            if (response == null || response.StatusCode != HttpStatusCode.OK
                || response.Data == null || string.IsNullOrEmpty(response.Data.access_token))
            {
                OnAuthenticationFailed();
            }
            else
            {
                Debug.Assert(response.Data != null);
                AuthResult = response.Data;
                OnAuthenticated();
            }
        }

        public void RefreshAccessToken()
        {
            Debug.Assert(HasAuthenticated);

            var authorize = new RestRequest(this.TokenEndPoint, Method.POST);
            authorize.AddParameter("refresh_token", AuthResult.refresh_token);
            authorize.AddParameter("client_id", this.ClientId);
            authorize.AddParameter("client_secret", this.Secret);
            authorize.AddParameter("grant_type", "refresh_token");

            client.ExecuteAsync<RscHttpsAuthResult>(authorize, RefreshAccessToken);
        }

        void RefreshAccessToken(IRestResponse<RscHttpsAuthResult> response)
        {
            if (response == null || response.StatusCode != HttpStatusCode.OK
                || response.Data == null || string.IsNullOrEmpty(response.Data.access_token))
            {
                OnAuthenticationFailed();
            }
            else
            {
                Debug.Assert(response.Data != null);
                Debug.Assert(AuthResult != null);

                RscHttpsAuthResult r = response.Data;
                r.refresh_token = AuthResult.refresh_token;
                AuthResult = r;
                OnAuthenticated();
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
		
    }

}